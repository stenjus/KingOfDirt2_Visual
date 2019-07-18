using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

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
            string path = EditorUtility.SaveFilePanelInProject("Save Generated mesh:", "Line_Render_Baked", "asset", "Choose directory for saving asset", "Assets/");
            Object saveObj = Instantiate(LineRendererOnSpline()._BakedMesh);
            AssetDatabase.CreateAsset(saveObj, path);
            AssetDatabase.SaveAssets();
            _savedMesh = LineRendererOnSpline()._BakedMesh;
        }

        if(GUILayout.Button("Instance Saved Object"))
        {
            GameObject _instancedObj = new GameObject() { name = "Combined Spline"};
            _instancedObj.AddComponent<MeshFilter>();
            _instancedObj.AddComponent<MeshRenderer>();
            //_instancedObj_MeshRenderer.material = new Material(Shader.Find("Diffuse"));
            Transform _targetTransform = ((LineRendererOnSpline)target).transform;
            _instancedObj.transform.position = _targetTransform.position;
            _instancedObj.transform.rotation = _targetTransform.rotation;
            MeshFilter _instancedObj_MeshFilter = _instancedObj.GetComponent<MeshFilter>();
            _instancedObj_MeshFilter.mesh = _savedMesh;
            Debug.Log(_savedMesh);
            //_instancedObj = Instantiate(_instancedO_savedMeshbj, _targetTransform.position, _targetTransform.rotation);
        }
        if(_savedMesh != null)
        EditorGUILayout.HelpBox("Generated mesh Vertex count: " + LineRendererOnSpline()._BakedMesh.vertices.Length, MessageType.Info);

        serializedObject.ApplyModifiedProperties();
    }

    public LineRendererOnSpline LineRendererOnSpline() {
        return (LineRendererOnSpline)target;
    }
}
