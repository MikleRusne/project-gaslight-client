using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(Spawner))]
public class SpawnerInspector : Editor
{
    private Spawner targ;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        // EditorGUILayout.LabelField("Hey");
        targ = (Spawner) target;
        if (GUILayout.Button("Spawn Level"))
        {
            targ.Spawn();
            // serializedObject.FindProperty("O")
            EditorUtility.SetDirty(targ);
            // EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
        
        EditorGUILayout.LabelField("Instantiated Tiles:");
        // EditorGUILayout.LabelField(targ.DisplayInstantiated());
        List<string> InstantiatedTiles = targ.DisplayInstantiated();
        var sb = new StringBuilder();
        foreach (var instantiatedTile in InstantiatedTiles)
        {
            sb.AppendLine(instantiatedTile);
        }

        EditorGUILayout.TextArea(sb.ToString());
    }
}
