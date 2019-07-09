#if !defined(CUSTOM_LIGHTMAPPING_INCLUDED)
#define CUSTOM_LIGHTMAPPING_INCLUDED

struct VertexInputCustom
{
	float4 vertex   : POSITION;
	half3 normal    : NORMAL;
	float2 uv0      : TEXCOORD0;
	float3 color	: COLOR;
	float2 uv1      : TEXCOORD1;
	float2 uv2      : TEXCOORD2;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f_metaCustom
{
	float4 pos      : SV_POSITION;
	float4 uv       : TEXCOORD0;
	float3 color	: COLOR;
};

float4 TexCoordsCustom(VertexInputCustom v)
{
	float4 texcoord;
	texcoord.xy = TRANSFORM_TEX(v.uv0, _MainTex); 
	texcoord.zw = TRANSFORM_TEX(((_UVSec == 0) ? v.uv0 : v.uv1), _DetailAlbedoMap);
	return texcoord;
}

v2f_metaCustom vert_metaCustom(VertexInputCustom v)
{
	v2f_metaCustom o;
	o.pos = UnityMetaVertexPosition(v.vertex, v.uv1.xy, v.uv2.xy, unity_LightmapST, unity_DynamicLightmapST);
	o.uv = TexCoordsCustom(v);
	o.color = v.color;
	return o;
}
#endif