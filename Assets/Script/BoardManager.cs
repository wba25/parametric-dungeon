﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Random = UnityEngine.Random;


public class BoardManager : MonoBehaviour
{
    [Serializable]
    public class Count
    {
        public int minimum;
        public int maximum;

        public Count (int min, int max)
        {
            minimum = min;
            maximum = max;
        }
    }

    #region Constantes
    public const float TileSize = 1f;
    public (float x, float y) BoardOffset;
    public const int Columns = 8;
    public const int Rows = 8;
    #endregion

    #region Prefabs
    public GameObject[] floorTiles;
    public CompoundTile outerWallTiles;
    public CompoundTile outerWallCorner;
    public CompoundTile outerWallGate;
    public AssetData objects;
    #endregion

    Dictionary<TileDirection, bool> hasGates;
    private Transform boardHolder;
    private List <Vector3> gridPositions = new List <Vector3> ();

    void InitialiseList ()
    {
        gridPositions.Clear ();

        for(int x = 1; x < Columns-1; x++)
        {
            for(int y = 1; y < Rows-1; y++)
            {
                gridPositions.Add (ToVectorCoords((x, y)));
            }
        }
    }

    public void Reset()
    {
        // gridPositions = new List <Vector3> ();
    }

    void BoardSetup ()
    {
        GameObject board = new GameObject ("Board");
        board.transform.SetParent (
            GameManager.Instance.levelAnchor
        );
        boardHolder = board.transform;
        //transform
        for (int x = -1; x < Columns + 1; x++)
        {
            for (int y = -1; y < Rows + 1; y++)
            {
                GameObject toInstantiate = floorTiles[Random.Range (0,floorTiles.Length)];
                (int x, int y) position = (x, y);
                (float x, float y) offset = (0, 0);

                // Corner
                if (x == -1 && y == -1) {
                    DirectionalTile tile = outerWallCorner.GetTile(TileDirection.LEFT);
                    offset = (tile.offsetX, tile.offsetY);
                    toInstantiate = tile.prefab;
                }
                else if (x == Rows && y == -1) {
                    DirectionalTile tile = outerWallCorner.GetTile(TileDirection.DOWN);
                    offset = (tile.offsetX, tile.offsetY);
                    toInstantiate = tile.prefab;
                }
                else if (x == -1 && y == Columns) {
                    DirectionalTile tile = outerWallCorner.GetTile(TileDirection.UP);
                    offset = (tile.offsetX, tile.offsetY);
                    toInstantiate = tile.prefab;
                }
                else if (x == Rows && y == Columns) {
                    DirectionalTile tile = outerWallCorner.GetTile(TileDirection.RIGHT);
                    offset = (tile.offsetX, tile.offsetY);
                    toInstantiate = tile.prefab;
                }

                // Wall
                else if (x == -1)
                {
                    if (
                        hasGates[TileDirection.LEFT] &&
                        (y >= Columns/2 - 2 && y <= Columns/2 + 2)
                    )
                    {
                        toInstantiate = null;
                    }
                    else {
                        DirectionalTile tile = outerWallTiles.GetTile(TileDirection.LEFT);
                        offset = (tile.offsetX, tile.offsetY);
                        toInstantiate = tile.prefab;
                    }
                }
                else if (x == Rows)
                {
                    if (
                        hasGates[TileDirection.RIGHT] &&
                        (y >= Columns/2 - 2 && y <= Columns/2 + 2)
                    )
                    {
                        toInstantiate = null;
                    }
                    else {
                        DirectionalTile tile = outerWallTiles.GetTile(TileDirection.RIGHT);
                        offset = (tile.offsetX, tile.offsetY);
                        toInstantiate = tile.prefab;
                    }
                }
                else if (y == -1)
                {
                    if (
                        hasGates[TileDirection.DOWN] &&
                        (x >= Rows/2 - 2 && x <= Rows/2 + 2)
                    )
                    {
                        toInstantiate = null;
                    }
                    else {
                        DirectionalTile tile = outerWallTiles.GetTile(TileDirection.DOWN);
                        offset = (tile.offsetX, tile.offsetY);
                        toInstantiate = tile.prefab;
                    }
                }
                else if (y == Columns)
                {
                    if (
                        hasGates[TileDirection.UP] &&
                        (x >= Rows/2 - 2 && x <= Rows/2 + 2)
                    )
                    {
                        toInstantiate = null;
                    }
                    else {
                        DirectionalTile tile = outerWallTiles.GetTile(TileDirection.UP);
                        offset = (tile.offsetX, tile.offsetY);
                        toInstantiate = tile.prefab;
                    }
                }

                // Gates
                if (
                    hasGates[TileDirection.LEFT] &&
                    x == -1 &&
                    y == Columns / 2

                )
                {
                    DirectionalTile tile = outerWallGate.GetTile(TileDirection.LEFT);
                    offset = (tile.offsetX, tile.offsetY);
                    toInstantiate = tile.prefab;
                }
                else if (
                    hasGates[TileDirection.RIGHT] &&
                    x == Rows &&
                    y == Columns / 2
                )
                {
                    DirectionalTile tile = outerWallGate.GetTile(TileDirection.RIGHT);
                    offset = (tile.offsetX, tile.offsetY);
                    toInstantiate = tile.prefab;
                }
                else if (
                    hasGates[TileDirection.DOWN] &&
                    x == Rows / 2 &&
                    y == -1
                )
                {
                    DirectionalTile tile = outerWallGate.GetTile(TileDirection.DOWN);
                    offset = (tile.offsetX, tile.offsetY);
                    toInstantiate = tile.prefab;
                }
                else if (
                    hasGates[TileDirection.UP] &&
                    x == Rows / 2 &&
                    y == Columns
                )
                {
                    DirectionalTile tile = outerWallGate.GetTile(TileDirection.UP);
                    offset = (tile.offsetX, tile.offsetY);
                    toInstantiate = tile.prefab;
                }

                // Cria tile
                if (toInstantiate)
                {
                    GameObject instance = Instantiate (toInstantiate, ToVectorCoords(position), Quaternion.identity) as GameObject;
                    instance.transform.SetParent (boardHolder);
                    Vector3 replacement = new Vector3(TileSize * offset.x, TileSize * offset.y, 0f);
                    instance.transform.position += replacement;
                }
            }
        }
    }

    Vector3 RandomPosition ()
    {
        int randomIndex = Random.Range (0, gridPositions.Count);
        Vector3 randomPosition = gridPositions[randomIndex];
        gridPositions.RemoveAt (randomIndex);
        return randomPosition;
    }

    void LayoutObjectAtRandom (GameObject[] tileArray, int minimum, int maximum)
    {
        int objectCount = Random.Range (minimum, maximum+1);
        for(int i = 0; i < objectCount; i++)
        {
            Vector3 randomPosition = RandomPosition();
            GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
            GameObject obj = Instantiate(tileChoice, randomPosition, Quaternion.identity);
            obj.transform.SetParent (
                GameManager.Instance.levelAnchor
            );
        }
    }

    public void SetupRoom ((int x, int y) originPoint, Dictionary<TileDirection, bool> gates)
    {
        hasGates = gates;

        (float x, float y) offset = (((Rows + 6) * TileSize), ((Columns + 6) * TileSize));
        BoardOffset = (offset.x * originPoint.x, offset.y * originPoint.y);

        //Creates the outer walls and floor.
        BoardSetup ();

        //Reset our list of gridpositions.
        InitialiseList ();
    }

    public void SetupContent ((int x, int y) originPoint, List<NodeData> contents)
    {
        (float x, float y) offset = (((Rows + 6) * TileSize), ((Columns + 6) * TileSize));
        BoardOffset = (offset.x * originPoint.x, offset.y * originPoint.y);

        // Cria conteudo
        foreach (NodeData data in contents)
        {
            try
            {
                Asset obj = objects.GetAsset(data.key, data.type);
                for (int i = 0; i < data.quantity; i++)
                {
                    AddObjectAtRandom(obj.prefab);
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Debug.Log(e);
#endif
            }
        }

        // Reset our list of gridpositions
        InitialiseList ();
    }

    #region Métodos Do Conteudo
    public void AddObjectAtRandom(GameObject obj)
    {
        Vector3 randomPosition = RandomPosition();
        AddObject(randomPosition,obj);
    }
    public void AddObject(Vector3 position, GameObject obj)
    {
        GameObject newObj = Instantiate(obj, position, Quaternion.identity);    
        newObj.transform.SetParent (
            GameManager.Instance.levelAnchor
        );
    }
    #endregion
    
    #region Métodos Utilitarios
    public float ToRealXValue(int val)
    {
        return ((val - 1) * TileSize) + BoardOffset.x;
    }

    public float ToRealYValue(int val)
    {
        return ((val - 1) * TileSize) + BoardOffset.y;
    }

    public int ToGridXValue(float val)
    {
        return Convert.ToInt32(((val - BoardOffset.x) / TileSize) + 1);
    }

    public int ToGridYValue(float val)
    {
        return Convert.ToInt32(((val - BoardOffset.y) / TileSize) + 1);
    }

	public (int, int) ToGridCoords(Vector3 rawPosition)
    {
        var conversion = GameManager.Instance.cam.ScreenToWorldPoint(rawPosition);
        return (this.ToGridXValue(conversion.x), this.ToGridYValue(conversion.y));
    }

	public Vector3 ToVectorCoords((int x, int y) gridPosition)
    {
        return new Vector3(this.ToRealXValue(gridPosition.x), this.ToRealYValue(gridPosition.y), 0f);
    }
    #endregion
}
