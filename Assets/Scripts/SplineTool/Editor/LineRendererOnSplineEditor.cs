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
    private GameObject _instancedObj;

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
            //Save Asset to AssetDatabase
            string path = EditorUtility.SaveFilePanelInProject("Save Generated mesh:", "Line_Render_Baked", "asset", "Choose directory for saving asset", "Assets/");
            Object saveObj = Instantiate(LineRendererOnSpline()._BakedMesh);
            AssetDatabase.CreateAsset(saveObj, path);
            AssetDatabase.SaveAssets();
            _savedMesh = (Mesh)saveObj;

            //Remove old instanced baked mesh
            if (_instancedObj)
            {
                Debug.Log("Destroy previous obj");
                Object.DestroyImmediate(_instancedObj.gameObject);
            }

            //Instance mesh to scene
            _instancedObj = new GameObject() { name = "Baked Spline" };

            _instancedObj.AddComponent<MeshFilter>();
            MeshRenderer _instancedObj_MeshRenderer = _instancedObj.AddComponent<MeshRenderer>();
            _instancedObj_MeshRenderer.material = new Material(Shader.Find("Standard"));
            Transform _targetTransform = ((LineRendererOnSpline)target).transform;
            _instancedObj.transform.position = _targetTransform.position;
            _instancedObj.transform.rotation = _targetTransform.rotation;
            MeshFilter _instancedObj_MeshFilter = _instancedObj.GetComponent<MeshFilter>();
            _instancedObj_MeshFilter.mesh = _savedMesh;
            MeshCollider _instancedObj_MeshCollider = _instancedObj.AddComponent<MeshCollider>();
            _instancedObj_MeshCollider.sharedMesh = _savedMesh;
        }
        Debug.Log(_instancedObj);

        if (_savedMesh != null)
            EditorGUILayout.HelpBox("Generated mesh Vertex count: " + LineRendererOnSpline()._BakedMesh.vertices.Length, MessageType.Info);

        serializedObject.ApplyModifiedProperties();
    }

    public LineRendererOnSpline LineRendererOnSpline()
    {
        return (LineRendererOnSpline)target;
    }
}
