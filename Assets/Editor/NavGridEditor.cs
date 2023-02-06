using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NavGrid))]
public class NavGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Generate"))
            (target as NavGrid).GenerateGrid();
        if (GUILayout.Button("Clear"))
            (target as NavGrid).Clear();
        if (GUILayout.Button("PathFinding"))
            (target as NavGrid).FindPath();
        SceneView.RepaintAll();
    }
}
