using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public enum LightImput
{
    Sliders,
    Vertex_Colors
}



public class Environment_Base_CustomEditor : ShaderGUI
{
    public static GUIContent _textureMapText = new GUIContent("Detail Map", "Gray shadowed wich will be colored by Vertex Colors");
    public static GUIContent _normalMapText = new GUIContent("Normal Map", "");
    public static GUIContent _normalMapIntText = new GUIContent("Normal Intencity", "");
    public static string renderingMode = "Lighting Input:";
    public static readonly string[] LightInputNames = Enum.GetNames(typeof(LightImput));
    MaterialEditor _MaterialEditor;
    private bool _SlidersFoldOut = false;
    private bool _LightmapFoldOut = false;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        _MaterialEditor = materialEditor;
        Material _material = materialEditor.target as Material;

        EditorGUILayout.HelpBox("Shader Uses Vertex Color Stored in COLOR, TEXCOORD2 for Rougness Color and TEXCOORD3 for additional Mask", MessageType.Info);
        CustomUI.GuiLine(1);
        EditorGUILayout.Space();

        var sliderUsing = FindProperty("_UseLightInput", properties);
        materialEditor.ShaderProperty(sliderUsing, sliderUsing.displayName);
        EditorGUILayout.Separator();
        CustomUI.GuiLine(1);
        EditorGUILayout.Separator();

        var _detMap = FindProperty("_MainTex", properties);
        materialEditor.TexturePropertySingleLine(_textureMapText, _detMap);

        var _nrmMap = FindProperty("_NormalMap", properties);
        materialEditor.TexturePropertySingleLine(_normalMapText, _nrmMap);

        var _nrmIntence = FindProperty("_NormalIntencity", properties);
        materialEditor.ShaderProperty(_nrmIntence, _normalMapIntText);

        EditorGUILayout.Space();

        var _Rouhgness = FindProperty("_Roughness", properties);
        materialEditor.ShaderProperty(_Rouhgness, "Roughness");

        var _F0 = FindProperty("_F0", properties);
        materialEditor.ShaderProperty(_F0, "F0");

        var _FresnelPower = FindProperty("_FresnelPower", properties);
        materialEditor.ShaderProperty(_FresnelPower, "Fresnel Power");

        EditorGUILayout.Separator();

        _LightmapFoldOut = CustomUI.FoldOut("Lightmapping", _LightmapFoldOut);
        if (_LightmapFoldOut)
        {
            EditorGUILayout.Separator();
            CustomUI.GuiLine(1);
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Lightmapping:", EditorStyles.boldLabel);

            var _IndirectAlbedoBoost = FindProperty("_IndirectAlbedoBoost", properties);
            materialEditor.ShaderProperty(_IndirectAlbedoBoost, "Indirect Albedo Boost");
        }

        EditorGUILayout.Separator();
        CustomUI.GuiLine(1);
        EditorGUILayout.Separator();

        if (GUILayout.Button("Open Mesh Combiner")) MeshCombiner.ShowWindow();
    }
}