using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class Level
{
    public List<RawEdge> edges;
    public List<RawNode> nodes;

    public static Level CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<Level>(jsonString);
    }
}

[Serializable]
public class RawNode
{
  public string node;
  public string data;
}

[Serializable]
public class RawEdge
{
  public string node;
  public List<string> neighbors;
}