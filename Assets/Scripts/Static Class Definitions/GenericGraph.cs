using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Node<T> where T: UID
{
    public T Info { get; private set; }

    public Node(T info) : base()
    {
        Info = info;
    }

}

public class GenericGraph<T> where T : UID
{

    //This is an oriented graph by default

    private Dictionary<Node<T>, HashSet<Node<T>>> edges;

    public GenericGraph()
    {
        this.edges = new Dictionary<Node<T>, HashSet<Node<T>>>();
    }
    public GenericGraph(List<Node<T>> nodes)
    {
        this.edges = new Dictionary<Node<T>, HashSet<Node<T>>>();
        foreach(Node<T> node in nodes)
        {
            edges.Add(node, new HashSet<Node<T>>());
        }
    }

    public void AddNode(T info)
    {
        Node<T> toAdd = new Node<T>(info);
        this.edges.Add(toAdd, new HashSet<Node<T>>());
    }

    public void AddEdge(T nodeStart, T nodeEnd)
    {
        bool hasNodeStart = edges.Any(kv => kv.Key.Info.uid == nodeStart.uid);
        bool hasNodeEnd = edges.Any(kv => kv.Key.Info.uid == nodeEnd.uid);

        if(!hasNodeStart)
        {
            Debug.LogError($"Couldn't find node with UID: {nodeStart.uid}");
            return;
        }
        if (!hasNodeEnd)
        {
            Debug.LogError($"Couldn't find node with UID: {nodeEnd.uid}");
            return;
        }

        Node<T> nS = edges.FirstOrDefault(kv => kv.Key.Info.uid == nodeStart.uid).Key;
        Node<T> nE = edges.FirstOrDefault(kv => kv.Key.Info.uid == nodeEnd.uid).Key;
        edges[nS].Add(nE);
    }

    public int GetTotalNodes()
    {
        return edges.Keys.Count;
    }
}
