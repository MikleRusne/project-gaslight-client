using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Orchestrator))]
public class OrchestratorInspector : Editor  
{
    private Orchestrator targ;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        // EditorGUILayout.LabelField("Hey");
        targ = (Orchestrator) target;
       
        EditorGUILayout.LabelField("Pending actions:");
        // EditorGUILayout.LabelField(targ.DisplayInstantiated());
        string[] InstantiatedTiles = targ.DisplayActions();
        var sb = new StringBuilder();
        foreach (var instantiatedTile in InstantiatedTiles)
        {
            sb.AppendLine(instantiatedTile);
        }

        EditorGUILayout.TextArea(sb.ToString());
    }
}
