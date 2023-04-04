using System.Text;
using Characters;
using GameActions;
using UnityEditor;
using UnityEngine;

namespace Globs.Inspectors
{
    
    [CustomEditor(typeof(Character))]
    public class CharacterInspector: Editor
    {
            private Character targ;

            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();
                // EditorGUILayout.LabelField("Hey");
                targ = (Character)target;
                EditorGUILayout.LabelField("Current Directives");
                var sb = new StringBuilder();
                foreach (Directive targCurrentDirective in targ.CurrentDirectives)
                {
                    sb.AppendLine(targCurrentDirective.ToString());
                }
                
                EditorGUILayout.TextArea(sb.ToString());
            }

    }
}