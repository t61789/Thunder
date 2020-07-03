using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CombineWindow
{
    public class CombineWindow : EditorWindow
    {
        public CombineWindow()
        {
            titleContent = new GUIContent("CombineWindow");
        }

        [MenuItem("Tools/CombineWindow")]
        public static void ShowWindow()
        {
            GetWindow<CombineWindow>();
        }

        [SerializeField]
        public List<Sprite> list = new List<Sprite>();

        public SerializedObject serializedObject;

        public SerializedProperty serializedProperty;

        private string from = "";
        private string to = "";

        private void OnEnable()
        {
            serializedObject = new SerializedObject(this);
            serializedProperty = serializedObject.FindProperty("list");
        }

        private void OnGUI()
        {
            serializedObject.Update();

            GUILayout.Label("From");
            from = GUILayout.TextField(from);
            GUILayout.Label("To");
            to = GUILayout.TextField(to);
            if (GUILayout.Button("Combine"))
            {
                if (list.Count != 0)
                {
                    if (from == "")
                    {
                        Debug.Log(123);
                        AnimationCombine.Start(list, to);
                    }
                    else
                    {
                        Debug.Log(312);
                        AnimationCombine.Start(from, to);
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedProperty, true);
            if (list.Count != 0)
                from = "";
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
