using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DirectionalTile", menuName = "Tiles/DirectionalTile", order = 1)]
public class CompoundTile : ScriptableObject
{
    public int size;
    public DirectionalTile[] tiles;

    public DirectionalTile GetTile(TileDirection target)
    {
      return Array.Find(tiles, p => p.direction == target);
    }
}

[Serializable]
public class DirectionalTile
{
    public float offsetX;
    public float offsetY;
    public GameObject prefab;
    public TileDirection direction;
}


public enum TileDirection
{
    UP,
    DOWN,
    RIGHT,
    LEFT
}