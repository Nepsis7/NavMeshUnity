using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using SF = UnityEngine.SerializeField;

public class GridGenerator : MonoBehaviour
{
    [SF] GridSettings settings = null;
    [SF] bool drawDebug = true, drawGrid = true, drawZone = true;
    [SF, HideInInspector] List<List<GridNode>> nodes = new();
    public void GenerateNodes()
    {
        if (!settings)
            return;
        ClearNodes();
        Vector2 _cacheNodePosXY = Vector2.zero;
        Vector3 _offset = transform.position - (Vector3.right * ((settings.GridSize.x - 1) / 2f) * settings.Gap) - (Vector3.forward * ((settings.GridSize.x - 1) / 2f) * settings.Gap);
        for (int iX = 0; iX < settings.GridSize.x; iX++)
        {
            nodes.Add(new());
            for (int iY = 0; iY < settings.GridSize.y; iY++)
            {
                _cacheNodePosXY.x = settings.Gap * iX;
                _cacheNodePosXY.y = settings.Gap * iY;
                nodes[iX].Add(new(GetNodePos(_cacheNodePosXY, _offset)));
            }
        }
        {
            bool _left = false, _right = false, _up = false, _down = false;
            for (int iX = 0; iX < settings.GridSize.x; iX++)
                for (int iY = 0; iY < settings.GridSize.y; iY++)
                {
                    _left = iX > 0;
                    _up = iY > 0;
                    _right = iX < (settings.GridSize.x - 1);
                    _down = iY < (settings.GridSize.y - 1);
                    GridNode _node = nodes[iX][iY];
                    if (_left) _node.LinkedNodes.Add(nodes[iX - 1][iY]);
                    if (_right) _node.LinkedNodes.Add(nodes[iX + 1][iY]);
                    if (_up)
                    {
                        _node.LinkedNodes.Add(nodes[iX][iY - 1]);
                        if (_left) _node.LinkedNodes.Add(nodes[iX - 1][iY - 1]);
                        if (_right) _node.LinkedNodes.Add(nodes[iX + 1][iY - 1]);
                    }
                    if (_down)
                    {
                        _node.LinkedNodes.Add(nodes[iX][iY + 1]);
                        if (_left) _node.LinkedNodes.Add(nodes[iX - 1][iY + 1]);
                        if (_right) _node.LinkedNodes.Add(nodes[iX + 1][iY + 1]);
                    }
                }
        }
        CollisionCheck();
    }
    void CollisionCheck()
    {
        foreach (List<GridNode> _line in nodes)
            foreach (GridNode _node in _line)
                _node.IsUsable = Physics.OverlapSphere(_node.Position, settings.Gap / 2, settings.ObstacleLayer).Length < 1;
    }
    public void ClearNodes()
    {
        nodes.Clear();
    }
    public List<Vector3> RequirePath(Vector3 _start, Vector3 _end) => RequirePath(GetClosestNode(_start), GetClosestNode(_end));
    public List<Vector3> RequirePath(GridNode _start, GridNode _end)
    {
        if (_start == null || _end == null || nodes.Count < 1)
            return null;
        //foreach (List<GridNode> _line in nodes)
        //    foreach (GridNode _node in _line)
        //        _node.Reset();
        List<GridNode> _openList = new() { _start }, _closedList = new();
        float _min = 0f;
        GridNode _current = null;
        while (_openList.Count > 0)
        {
            _current = null;
            _min = -1f;
            foreach (GridNode _node in _openList)
            {
                _node.ComputeCost(_start.Position, _end.Position);
                if (_min == -1f || _node.Cost < _min)
                {
                    _min = _node.Cost;
                    _current = _node;
                }
            }
            _openList.Remove(_current);
            _closedList.Add(_current);
            if (_current == _end)
            {
                _openList.Clear();
                continue;
            }
            foreach (GridNode _linkedNode in _current.LinkedNodes)
            {
                if (!_linkedNode.IsUsable || _closedList.Contains(_linkedNode))
                    continue;
                if (_openList.Contains(_linkedNode))
                {
                    float _currPathLength = _linkedNode.ComputePathLength(_start.Position);
                    float _newPathLength = _current.ComputePathLength(_start.Position) + Vector3.Distance(_current.Position, _linkedNode.Position);
                    if (_newPathLength > _currPathLength)
                        continue;
                }
                _linkedNode.ComputeCost(_start.Position, _end.Position);
                _linkedNode.Origin = _current;
                if (!_openList.Contains(_linkedNode))
                    _openList.Add(_linkedNode);
            }
        }
        List<Vector3> _path = new();
        while (_current != null)
        {
            _path.Add(_current.Position);
            _current = _current.Origin;
        }
        _path.Reverse();
        return _path;
    }
    GridNode GetClosestNode(Vector3 _position)
    {
        GridNode _closest = null;
        float _leastDistance = -1f;
        float _mag = 0;
        foreach (List<GridNode> _line in nodes)
            foreach (GridNode _node in _line)
                if ((_mag = (_position - _node.Position).sqrMagnitude) < _leastDistance || _leastDistance == -1f)
                {
                    _leastDistance = _mag;
                    _closest = _node;
                }
        return _closest;
    }
    Vector3 GetNodePos(Vector2 _posCYNoOffset, Vector3 _offset)
    {
        Vector3 _nocast = _offset + new Vector3(_posCYNoOffset.x, 0, _posCYNoOffset.y);
        Vector3 _heightOffset = Vector3.up * (settings.NavMeshHeight / 2);
        if (!Physics.Raycast(_nocast + _heightOffset, -Vector3.up, out RaycastHit _hit, settings.NavMeshHeight, settings.TerrainLayer))
            return _nocast;
        return _hit.point + Vector3.up * settings.NodeHeightOffset;
    }
    private void OnDrawGizmos()
    {
        if (!drawDebug)
            return;
        if (drawGrid)
            foreach (List<GridNode> _line in nodes)
                foreach (GridNode _node in _line)
                {
                    if (!_node.IsUsable)
                        continue;
                    Gizmos.color = Color.black;
                    Gizmos.DrawWireCube(_node.Position, Vector3.one * .5f);
                    Gizmos.color = Color.red;
                    foreach (GridNode _linkedNode in _node.LinkedNodes)
                        if (_linkedNode.IsUsable)
                            Gizmos.DrawLine(_node.Position, _linkedNode.Position);
                }
        if (!drawZone || !settings)
            return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, (Vector3.up * settings.NavMeshHeight / 2) + (Vector3.right * settings.Gap * settings.GridSize.x) + (Vector3.forward * settings.Gap * settings.GridSize.y));
        Gizmos.color = new Color(0, 1, 0, .1f);
        Gizmos.DrawCube(transform.position, (Vector3.up * settings.NavMeshHeight / 2) + (Vector3.right * settings.Gap * settings.GridSize.x) + (Vector3.forward * settings.Gap * settings.GridSize.y));
    }
}
