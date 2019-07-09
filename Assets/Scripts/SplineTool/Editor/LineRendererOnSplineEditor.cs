using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LineRendererOnSpline))]
public class LineRendererOnSplineEditor : Editor
{
    SerializedProperty _Width;
    SerializedProperty _Res;
    SerializedProperty _VertexColor;

    private Mesh _savedMesh;

    public void OnEnable()
    {
        _Width = serializedObject.FindProperty("_Width");
        _Res = serializedObject.FindProperty("resolution");
        _VertexColor = serializedObject.FindProperty("_VertexColor");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.HelpBox("Generates geometry based on spline on Runtime.", MessageType.Info);
        EditorGUILayout.PropertyField(_Width);
        EditorGUILayout.PropertyField(_Res);
        EditorGUILayout.PropertyField(_VertexColor);

        if (GUILayout.Button("Bake And Save Mesh"))
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Generated mesh:", "Line_Render_Baked", "asset", "Choose directory for saving asset", "/Assets");
            Debug.Log(path);
            AssetDatabase.CreateAsset(LineRendererOnSpline()._BakedMesh, path);
            AssetDatabase.SaveAssets();
            _savedMesh = LineRendererOnSpline()._BakedMesh;
        }
        if(_savedMesh != null)
        EditorGUILayout.HelpBox("Generated mesh Vertex count: " + LineRendererOnSpline()._BakedMesh.vertices.Length, MessageType.Info);

        serializedObject.ApplyModifiedProperties();
    }

    public LineRendererOnSpline LineRendererOnSpline() {
        return (LineRendererOnSpline)target;
    }
}
