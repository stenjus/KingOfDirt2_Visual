using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MeshCombiner : EditorWindow
{
    [SerializeField] static Mesh _BaseColor_mesh;
    [SerializeField] static Mesh _RoughnessColor_mesh;
    [SerializeField] static Mesh _AdditionalColor_mesh;

    static int _UVsetRoughness = 3;
    static int _UVsetAdditional = 4;
    
    [MenuItem("Tools/Vertex Color to UV converter")]

    public static void ShowWindow()
    {
        GetWindow<MeshCombiner>("Color to UV converter");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("Vertex Color to UV converter Tool v0.1", EditorStyles.boldLabel);
        GUILayout.Space(1);
        GUILayout.Label("This tool require 3 meshFilters with vertex color assigned.", EditorStyles.miniLabel);
        GUILayout.Label("Tool will convert Roughness and Additional colors to chosen TEXCOORD", EditorStyles.miniLabel);
        GUILayout.Label("Combined mesh will be saved as an .asset mesh in Assets folder", EditorStyles.miniLabel);
        GUILayout.Space(5);
        _BaseColor_mesh = (Mesh)EditorGUILayout.ObjectField("Base Color Mesh", _BaseColor_mesh, typeof(Mesh), true);
        _RoughnessColor_mesh = (Mesh)EditorGUILayout.ObjectField("Roughness Color Mesh", _RoughnessColor_mesh, typeof(Mesh), true);
        _UVsetRoughness = EditorGUILayout.IntSlider("UV Set Roughness", _UVsetRoughness, 0, 8);
        _AdditionalColor_mesh = (Mesh)EditorGUILayout.ObjectField("Additional Color Mesh", _AdditionalColor_mesh, typeof(Mesh), true);
        _UVsetAdditional = EditorGUILayout.IntSlider("UV Set Additional", _UVsetAdditional, 0, 8);
        GUILayout.Space(5);
        if (GUILayout.Button("Combine and Save")) Combine();
    }

    public static void Combine()
    {
        //Returns when UV Coords are same
        if(_UVsetRoughness == _UVsetAdditional)
        {
            EditorUtility.DisplayDialog("UVSets Conflict!", "Can't save two Vertex Colors to same TEXCOORD", "Ok");
            return;
        }
        //Returns is no mesh assigned
        if (!_BaseColor_mesh || !_RoughnessColor_mesh || !_AdditionalColor_mesh)
        {
            EditorUtility.DisplayDialog("Assign meshes", "Please, assign meshes.", "Ok");
            return;
        }

        var roughnessColor2list = new List<Vector4>();
        var additionalColor2list = new List<Vector4>();
        foreach (var c in _RoughnessColor_mesh.colors)
        {
            roughnessColor2list.Add(c);
        }
        foreach (var c in _AdditionalColor_mesh.colors)
        {
            additionalColor2list.Add(c);
        }

        Mesh _NewMesh = new Mesh();

        //_BaseColor_mesh.mesh.SetUVs(_UVsetRoughness, uv2list);
        _NewMesh.vertices = _BaseColor_mesh.vertices;
        _NewMesh.triangles = _BaseColor_mesh.triangles;

        _NewMesh.uv = _BaseColor_mesh.uv;
        _NewMesh.uv2 = _BaseColor_mesh.uv2;

        _NewMesh.normals = _BaseColor_mesh.normals;
        _NewMesh.tangents = _BaseColor_mesh.tangents;

        _NewMesh.colors = _BaseColor_mesh.colors;
        _NewMesh.SetUVs(_UVsetRoughness, roughnessColor2list);
        _NewMesh.SetUVs(_UVsetAdditional, additionalColor2list);

        string SavePath = EditorUtility.SaveFilePanelInProject("Save Combined Mesh", "Combined", "asset", "Specify where to save the mesh.");
        AssetDatabase.CreateAsset(_NewMesh, SavePath);
        Debug.Log("Successfully Combined Meshes: " + _BaseColor_mesh + " as Base Color, " + _RoughnessColor_mesh + " as Roughness Color and " + _AdditionalColor_mesh + "as Additional color");
    }
}
