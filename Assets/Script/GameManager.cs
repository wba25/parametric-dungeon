using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;
using SimpleFileBrowser;

public class GameManager : Singleton<GameManager>
{
    public Camera cam;
    [SerializeField] private BoardManager boardScript;
    [SerializeField] private Player player;
    [SerializeField] public Transform levelAnchor;
    private Dictionary<string, Vertex<(int x, int y)>> graph = new Dictionary<string, Vertex<(int x, int y)>>();
    private TextAsset jsonLevel;

    void Awake()
    {
        //InitGame();
    }

    void Start()
    {
        FileBrowser.SetFilters(
            true,
            new FileBrowser.Filter( "Text Files", ".txt" )
        );
        FileBrowser.SetDefaultFilter( ".txt" );
    }

    IEnumerator ShowLoadDialogCoroutine()
	{
		yield return FileBrowser.WaitForLoadDialog(
            FileBrowser.PickMode.FilesAndFolders,
            true,
            null,
            null,
            "Carregar Arquivos e Pastas",
            "Carregar"
        );

		if( FileBrowser.Success )
		{
			string rawFileData = FileBrowserHelpers.ReadTextFromFile( FileBrowser.Result[0] );
            jsonLevel = new TextAsset(rawFileData);
            UIManager.Instance.SetMainMenuActive(false);
            InitGame();
		}
	}

    public void LoadFile()
    {
        StartCoroutine( ShowLoadDialogCoroutine() );
    }

    void InitGame()
    {
        ResetGame();
        Level levelGraph = Level.CreateFromJSON(jsonLevel.text);
        CreateLevel(levelGraph);
        player.SetActive(true);
    }

    private void ResetGame()
    {
        graph = new Dictionary<string, Vertex<(int x, int y)>>();
        player.SetActive(false);
        boardScript.Reset();
        foreach (Transform child in levelAnchor) {
            GameObject.Destroy(child.gameObject);
        }
    }
    
    void CreateLevel(Level levelGraph)
    {
        // Mapeai as salas
        foreach (RawNode n in levelGraph.nodes)
        {
            graph.Add(
                n.node,
                new Vertex<(int x, int y)>((0, 0))
            );
        }

        // Mapeai as portas
        foreach (RawEdge edge in levelGraph.edges)
        {
            graph[edge.node].neighbors = new List<Vertex<(int x, int y)>>();
            foreach (string key in edge.neighbors)
            {
                Vertex<(int x, int y)> neighbor = graph[key];
                if (neighbor != null)
                    graph[edge.node].neighbors.Add(neighbor);
            }
        }  
        
        // Verificar se é um grafo planar

        // Obtem o posicionamento das salas
        Vertex<(int x, int y)>[,] map = GetEmbedOrthogonalGraph(graph);
        // Convete cada vertice numa sala
        for(int i = 0; i < map.GetLength(0); i++)
        {
            for(int j = 0; j < map.GetLength(1); j++)
            {
                if (map[i, j] != null)
                {
                    var hasGate = new Dictionary<TileDirection, bool>();
                    hasGate.Add(
                        TileDirection.UP,
                        j < map.GetLength(1) - 1 ? map[i, j + 1] != null : false
                    );
                    hasGate.Add(
                        TileDirection.DOWN,
                        j > 0 ? map[i, j - 1] != null : false
                    );
                    hasGate.Add(
                        TileDirection.RIGHT,                        
                        i < map.GetLength(0) - 1 ? map[i + 1, j] != null : false
                    );
                    hasGate.Add(
                        TileDirection.LEFT,
                        i > 0 ? map[i - 1, j] != null : false
                    );

                    boardScript.SetupRoom(
                        (i, j),
                        hasGate
                    );
                }
            }
        }
        // Adiciona o conteudo de cada sala
        // TODO: colocar dentro do outro for
        for(int j = 0; j < map.GetLength(1); j++)
        {
            var validYPositions = new List<int>();
            for(int i = 0; i < map.GetLength(0); i++)
            {
                if (map[i, j] != null)
                {
                    validYPositions.Add(i);
                }
            }
            if(validYPositions.Count > 0)
            {
                boardScript.SetupContent(
                    (Random.Range(0, validYPositions.Count), j),
                    levelGraph.nodes[j].data
                );
            }
        }
    }

    // Estrategia para desenho de grafo planar ortogonal de Storer
    Vertex<(int x, int y)>[,] GetEmbedOrthogonalGraph(Dictionary<string, Vertex<(int x, int y)>> G)
    {
        Vertex<(int x, int y)>[,] grid = new Vertex<(int x, int y)>[G.Count, G.Count];
        // Segmentação dos vértices
        for (int i = 0; i < G.Count; i++)
        {
            var element = G.ElementAt(i);
            var v = element.Value;
            v.data = (0, i);
        }

        // Segmentação de arestas
        (int v, int w) [] markedEdges = new (int v, int w) [] {};
        var poorVisibilityGraph = new Dictionary<string, Vertex<(int x, int y)>>(G);
        poorVisibilityGraph.Remove(poorVisibilityGraph.Keys.First());
        poorVisibilityGraph.Remove(poorVisibilityGraph.Keys.Last());
        bool shouldRunOnLimits = true;

        do
        {
            var minorDegreeVertex = poorVisibilityGraph.Where(
                v => v.Value.neighbors.Count == poorVisibilityGraph.Min(x => x.Value.neighbors.Count)
            ).FirstOrDefault();
            
            if (minorDegreeVertex.Value != null)
            {
                // Para cada aresta
                var neighbors = minorDegreeVertex.Value.neighbors;
                foreach (var neighbor in neighbors)
                {
                    int v = minorDegreeVertex.Value.data.y;
                    int w = neighbor.data.y;
                    if (markedEdges.Contains((v, w)) || markedEdges.Contains((w, v)))
                        continue;
                    
                    int x = Math.Max(GetNextX(v, grid), GetNextX(w, grid));
                    // Add pontos no grid
                    grid[x, v] = new Vertex<(int x, int y)>((x, v));
                    grid[x, w] = new Vertex<(int x, int y)>((x, w));
                    // Add uma adjacencia entre os pontos
                    grid[x, v].neighbors = new List<Vertex<(int x, int y)>>();
                    grid[x, v].neighbors.Add(grid[x, w]);
                    grid[x, w].neighbors = new List<Vertex<(int x, int y)>>();
                    grid[x, w].neighbors.Add(grid[x, v]);

                    markedEdges.Append((v, w));
                }
                
                poorVisibilityGraph.Remove(minorDegreeVertex.Key);
            }
            
            if (poorVisibilityGraph.Count == 0 && shouldRunOnLimits)
            {
                shouldRunOnLimits = false;
                var first = G.First();
                var last = G.Last();
                poorVisibilityGraph.Add(
                    first.Key,
                    first.Value
                );
                poorVisibilityGraph.Add(
                    last.Key,
                    last.Value
                );
                /*
                */
            }
        } while (poorVisibilityGraph.Count > 1);

        // Adiciona os vertices das pontas

        return grid;
    }

    int GetNextX(int vertex, Vertex<(int x, int y)>[,] grid)
    {
        int lastIndex = 0;
        while (grid[lastIndex, vertex] != null)
        {
            // TODO: verificar se (lastIndex, vertex) cruza aresta. Pra cima ou baixo?
            // tem que pecorrer ate o fim, para o caso de haver intermediarios vazios
            lastIndex++;
        }
        return lastIndex;
    }
}