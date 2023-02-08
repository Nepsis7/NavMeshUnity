using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SF = UnityEngine.SerializeField;

[CreateAssetMenu(fileName = "settings", menuName = "NavMesh/Settings")]
public class GridSettings : ScriptableObject
{
    [SF] Vector2Int gridSize = Vector2Int.one * 2;

    [SF] float gap = 1f;

    [SF] LayerMask obstacleLayer;

    [SF] LayerMask terrainLayer;

    [SF] float navMeshHeight = 4f;

    [SF] float nodeHeightOffset = .15f;
    public Vector2Int GridSize => gridSize;
    public float Gap => gap;
    public LayerMask ObstacleLayer => obstacleLayer;
    public LayerMask TerrainLayer => terrainLayer;
    public float NavMeshHeight => navMeshHeight;
    public float NodeHeightOffset => nodeHeightOffset;
}
