using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SF = UnityEngine.SerializeField;

[Serializable]
public class GridNode
{
    [SF, HideInInspector] Vector3 position = Vector3.zero;
    [SF, HideInInspector] GridNode origin = null;
    [SF, HideInInspector] List<GridNode> linkedNodes = new();
    float distToStart = 0f, distToEnd = 0f, cost = 0f;
    public bool IsUsable { get; set; } = true;
    public Vector3 Position => position;
    public GridNode Origin { get => origin; set => origin = value; }
    public List<GridNode> LinkedNodes => linkedNodes;
    public float Cost => cost;
    public float DistToStart => distToStart;
    public float DistToEnd => distToEnd;
    public GridNode(Vector3 _position)
    {
        position = _position;
    }
    public float ComputeCost(Vector3 _start, Vector3 _end) => cost = ComputeDistToEnd(_end) + ComputeDistToStart(_start);
    public float ComputeDistToStart(Vector3 _start) => distToStart = Vector3.Distance(_start, position);
    public float ComputeDistToEnd(Vector3 _end) => distToEnd = Vector3.Distance(_end, position);
    public float ComputePathLength(Vector3 _start)
    {
        GridNode _node = this;
        float _total = 0f;
        while(_node.origin != null)
        {
            _total += Vector3.Distance(_node.position, _node.origin.position);
            _node = _node.origin;
        }
        _total+= Vector3.Distance(_start, _node.position);
        return _total;
    }
    public void Reset()
    {
        cost = distToEnd = distToStart = 0f;
        origin = null;
    }
}
