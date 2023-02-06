using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using SF = UnityEngine.SerializeField;

public class Node
{
    public Node origin = null;
    public List<Node> children = new List<Node>();
    public bool isUsable = true;
    public Vector2 pos = Vector2.zero;
    public Vector2Int id = Vector2Int.zero;
    public float cost = 0, distToTarget = 0, distToStart = 0;
    public float ComputeCost(Vector2Int _startNode, Vector2Int _targetNode)
    {
        return cost = ComputeDistToTarget(_targetNode) + ComputeDistToStart(_startNode);
    }
    public float ComputeDistToTarget(Vector2Int _targetNode) => distToTarget = Vector2Int.Distance(id, _targetNode);
    public float ComputeDistToStart(Vector2Int _startNode) => distToStart = Vector2Int.Distance(id, _startNode);
    public float ComputePathLength(Vector2Int _startNode)
    {
        float _length = 0f;
        Node _curr = this;
        while(_curr.origin != null)
        {
            _length += Vector2Int.Distance(_curr.id, _curr.origin.id);
            _curr = _curr.origin;
        }
        _length += Vector2Int.Distance(_curr.id, _startNode);
        return _length;
    }
}

[ExecuteInEditMode]
public class NavGrid : MonoBehaviour
{
    [SF] bool realtimePathFinding = true, collisionCheck = true;
    [SF] float spaceBetweenNodes = 1f;
    [SF] Vector2Int gridSize = Vector2Int.one;
    [SF] Vector2Int start = new(0, 0), target = new(4, 6);
    [SF] float nodesRadius = .25f;
    [SF] Color defaultNodeColor = Color.black, unusableNodeColor = Color.red, startNodeColor = Color.blue, targetNodeColor = Color.magenta, linksColor = Color.black, pathColor = Color.yellow;
    List<List<Node>> nodes = new List<List<Node>>();
    List<Node> path = new List<Node>();
    float currSpaceBetweenNodes = 0f;
    Vector2Int currGridSize = Vector2Int.one;
    void Update() => FindPath();
    void OnDrawGizmos() => Draw();
    public Vector3 WorldPos(Node _node) => transform.position + new Vector3(_node.pos.x, 0, _node.pos.y);
    public void GenerateGrid()
    {
        if (gridSize.x < 1 || gridSize.y < 1)
            return;
        Clear();
        currGridSize = gridSize;
        currSpaceBetweenNodes = spaceBetweenNodes;
        Vector2 _totalSizeHalf = new Vector2((currGridSize.x - 1) * currSpaceBetweenNodes, (currGridSize.y - 1) * currSpaceBetweenNodes) / 2;
        for (int ix = 0; ix < currGridSize.x; ix++)
        {
            nodes.Add(new List<Node>());
            for (int iy = 0; iy < currGridSize.y; iy++)
            {
                Node _node = new Node();
                _node.pos = new Vector2(ix, iy) * currSpaceBetweenNodes - _totalSizeHalf;
                _node.id.x = ix;
                _node.id.y = iy;
                nodes[ix].Add(_node);
            }
        }
        {
            bool _canLeft, _canRight, _canUp, _canDown;
            Vector2Int _id = Vector2Int.zero;
            foreach (List<Node> _line in nodes)
                foreach (Node _node in _line)
                {
                    _id = _node.id;
                    _canLeft = _id.x > 0;
                    _canUp = _id.y > 0;
                    _canRight = _id.x < currGridSize.x - 1;
                    _canDown = _id.y < currGridSize.y - 1;
                    if (_canLeft)
                        _node.children.Add(nodes[_id.x - 1][_id.y]);
                    if (_canRight)
                        _node.children.Add(nodes[_id.x + 1][_id.y]);
                    if (_canUp)
                    {
                        _node.children.Add(nodes[_id.x][_id.y - 1]);
                        if (_canRight)
                            _node.children.Add(nodes[_id.x + 1][_id.y - 1]);
                        if (_canLeft)
                            _node.children.Add(nodes[_id.x - 1][_id.y - 1]);
                    }
                    if (_canDown)
                    {
                        _node.children.Add(nodes[_id.x][_id.y + 1]);
                        if (_canRight)
                            _node.children.Add(nodes[_id.x + 1][_id.y + 1]);
                        if (_canLeft)
                            _node.children.Add(nodes[_id.x - 1][_id.y + 1]);
                    }
                }
        }
    }
    public void Clear()
    {
        nodes.Clear();
        path.Clear();
    }
    public void FindPath()
    {
        if (!realtimePathFinding || nodes.Count < 1)
            return;
        if (collisionCheck)
            CollisionCheck();
        path.Clear();
        List<Node> _openNodes = new List<Node>(), _closedNodes = new List<Node>();
        _openNodes.Add(nodes[start.x][start.y]);
        Node _final = null;
        while (_openNodes.Count > 0)
        {
            float _minCost = -1f;
            Node _currNode = null;
            foreach (Node _node in _openNodes)
            {
                _node.ComputeCost(start, target);
                if (_minCost == -1f || _node.cost < _minCost)
                {
                    _minCost = _node.cost;
                    _currNode = _node;
                }
            }
            _openNodes.Remove(_currNode);
            _closedNodes.Add(_currNode);
            if (_currNode.id == target)
            {
                _openNodes.Clear();
                _final = _currNode;
                continue;
            }
            _minCost = -1f;
            Node _targetChild = null;
            foreach (Node _child in _currNode.children)
            {
                if(!_child.isUsable || _closedNodes.Contains(_child))
                    continue;
                if(_openNodes.Contains(_child))
                {
                    float _currentPathLength = _child.ComputePathLength(start);
                    float _newPathLength = _currNode.ComputePathLength(start) + Vector2Int.Distance(_currNode.id, _child.id);
                    if (_newPathLength > _currentPathLength)
                        continue;
                }
                _child.ComputeCost(start, target);
                _child.origin = _currNode;
                if(!_openNodes.Contains(_child))
                    _openNodes.Add(_child);
            }
            if (_targetChild == null)
                continue;
            _targetChild.origin = _currNode;

            _openNodes.Add(_targetChild);
        }
        Node _curr = _final;
        while (_curr.origin != null)
        {
            path.Add(_curr);
            _curr = _curr.origin;
        }
        path.Add(nodes[start.x][start.y]);
    }
    void CollisionCheck()
    {
        foreach (List<Node> _line in nodes)
            foreach (Node _node in _line)
            {
                Collider[] _hits = Physics.OverlapSphere(WorldPos(_node), nodesRadius);
                _node.isUsable = _hits.Length < 1;
            }
    }
    private void Draw()
    {
        Color _currNodeColor = defaultNodeColor;
        foreach (List<Node> _line in nodes)
            foreach (Node _node in _line)
            {
                _currNodeColor = defaultNodeColor;
                if (!_node.isUsable)
                    _currNodeColor = unusableNodeColor;
                else if (_node.id == target)
                    _currNodeColor = targetNodeColor;
                else if (_node.id == start)
                    _currNodeColor = startNodeColor;
                Gizmos.color = _currNodeColor;
                Gizmos.DrawWireSphere(WorldPos(_node), nodesRadius);
                Handles.Label(WorldPos(_node) + Vector3.forward * -.05f, $"{Mathf.CeilToInt(_node.ComputeCost(start, target) * 10)}");
                Handles.Label(WorldPos(_node) + Vector3.forward * .1f + Vector3.right * -.1f, $"{Mathf.CeilToInt(_node.ComputeDistToStart(start) * 10)}");
                Handles.Label(WorldPos(_node) + Vector3.forward * .1f + Vector3.right *.1f, $"{Mathf.CeilToInt(_node.ComputeDistToTarget(target) * 10)}");
                foreach (Node _child in _node.children)
                {
                    Gizmos.color = linksColor;
                    //Gizmos.DrawLine(WorldPos(_node), WorldPos(_child));
                }
            }
        {
            Node _last = null;
            foreach (Node _node in path)
            {
                if (_last == null)
                {
                    _last = _node;
                    continue;
                }
                Gizmos.color = pathColor;
                Gizmos.DrawLine(WorldPos(_last), WorldPos(_node));
                _last = _node;
            }
        }
        Debug.Log(path.Count);

    }
}
