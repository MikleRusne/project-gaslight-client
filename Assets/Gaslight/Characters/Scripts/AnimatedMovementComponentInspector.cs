
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimatedMovementComponent))]
public class AnimatedMovementComponentInspected : Editor
{
    private AnimatedMovementComponent targ;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        targ = (AnimatedMovementComponent)target;
        EditorGUILayout.LabelField("Custom controls");
    }
}
