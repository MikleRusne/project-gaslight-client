using System;
using Tiles;
using UnityEditor;
using UnityEngine;

namespace Globs.Inspectors
{
[CustomEditor(typeof(BlockLinks))]

    public class TileInspector:Editor
    {
         private BlockLinks targ;
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();
                targ = (BlockLinks) target;
                EditorGUILayout.LabelField("ID: " + targ.MyID.y + ", " + targ.MyID.x);
                EditorGUILayout.LabelField("IsOccupied: " + targ.isOccupied);
                if (targ.isOccupied)
                {
                    EditorGUILayout.LabelField("Occupied by: " + targ.OccupiedBy.transform.name);
                }
                EditorGUILayout.LabelField("Neighbors:" );
                foreach (BlockLinks.ELinkDirection value in Enum.GetValues(typeof(BlockLinks.ELinkDirection)))
                {
                    if (targ.connected[value])
                    {
                        EditorGUILayout.LabelField(value.ToString() + ": " + targ.Neighbors[value].Tile.name);
                    }
                }
                
                // if (GUILayout.Button("Spawn Level"))
                // {
                //     // targ.Spawn();
                //     // serializedObject.FindProperty("O")
                //     EditorUtility.SetDirty(targ);
                //     // EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                // }
                
                // EditorGUILayout.LabelField("Instantiated Tiles");
                // List<string> InstantiatedTiles = targ.DisplayInstantiated();
                // foreach (var instantiatedTile in InstantiatedTiles)
                // {
                //     EditorGUILayout.LabelField(instantiatedTile);
                // }
            }
    }
}