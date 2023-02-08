using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridGenerator))]
public class GridGeneratorEditor : Editor
{
    GridGenerator targetC = null;
    Transform start = null, end = null;
    List<Vector3> path = new();
    bool canTick = false;
    private void OnEnable()
    {
        targetC = target as GridGenerator;
        start = end = null;
        canTick = true;
    }
    private void OnDisable()
    {
        canTick = false;
    }
    public override void OnInspectorGUI()
    {
        if (!canTick)
            return;
        base.OnInspectorGUI();
        EditorGUILayout.HelpBox("Grid", MessageType.None);
        if (GUILayout.Button("Generate Grid"))
            targetC.GenerateNodes();
        if (GUILayout.Button("Clear Grid"))
            targetC.ClearNodes();
        EditorGUILayout.HelpBox("Testing", MessageType.None);
        start = (EditorGUILayout.ObjectField("Start", start?.gameObject, typeof(GameObject), true) as GameObject)?.transform;
        end = (EditorGUILayout.ObjectField("End", end?.gameObject, typeof(GameObject), true) as GameObject)?.transform;
        SceneView.RepaintAll();
        if (start == null || end == null)
            return;
        if (GUILayout.Button("Require Path"))
            path = targetC.RequirePath(start.position, end.position);
        if (GUILayout.Button("Clear Path"))
            path?.Clear();
    }
    void OnSceneGUI()
    {
        if (path == null || path.Count < 2)
            return;
        Handles.color = Color.yellow;
        Vector3 _last = Vector3.zero;
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 _curr = path[i];
            Handles.DrawWireCube(_curr + Vector3.up * .5f, Vector3.one * .5f);
            Handles.DrawLine(_curr, _curr + Vector3.up * .5f);
            if (i > 1)
                Handles.DrawLine(_curr, _last);
            _last = _curr;
        }
    }
}
