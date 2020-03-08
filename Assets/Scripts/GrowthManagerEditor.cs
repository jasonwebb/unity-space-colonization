using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GrowthManager))]
public class GrowthManagerEditor : Editor {
  public override void OnInspectorGUI() {
    GrowthManager manager = (GrowthManager)target;

    DrawDefaultInspector();

    EditorGUILayout.BeginHorizontal();

      if(GUILayout.Button("Grow")) {
        manager.GrowInEditor();
      }

      if(GUILayout.Button("Reset")) {
        manager.ResetScene();
      }

      if(GUILayout.Button("Export")) {
        manager.ExportOBJ();
      }

    EditorGUILayout.EndHorizontal();
  }
}