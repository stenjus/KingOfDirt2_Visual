Shader "King of Dirt 2/Environment Base"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_NormalMap ("Normal", 2D) = "bump" {}
		_NormalIntencity("Normal Intencity", Range(0,3)) = 1.0
		_Roughness("Roughness", Range(0,1)) = 1.0
		_F0("Fresnle Offset", Range(0,1)) = 1.0
		_FresnelPower("Fresnel Power", Range(0, 10)) = 5.0
		_IndirectAlbedoBoost ("Indirect Albedo Boost", Range(0,100)) = 1.0
		[KeywordEnum(Sliders, VertexColors)] _UseLightInput("Light Input Values", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
			Tags { "LightMode"="ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityLightingCommon.cginc"
			#pragma multi_compile_fwdbase
			#pragma multi_compile _USELIGHTINPUT_SLIDERS _USELIGHTINPUT_VERTEXCOLORS

            struct appdata
            {
                float4	vertex	: POSITION;
				float3	vcolor	: COLOR;		//Base Color 
                float2	uv		: TEXCOORD0;	//Texturing UV
				float2	uv1		: TEXCOORD1;	//Lightmapping UV
				float2  uvRough	: TEXCOORD2;	//Roughness Color
				float2	uvAdd	: TEXCOORD3;	//Additional Color
				float3	normal	: NORMAL;
				float4	tangent : TANGENT;
            };

            struct v2f
            {
                float4	uv		: TEXCOORD0;	//uv.XY - UV1 for Texturing, uv.ZW - UV2 for Lightmapping
				half4	tspace0 : TEXCOORD1;	//tspace W component includes wPos.x
				half4	tspace1 : TEXCOORD2;	//tspace W component includes wPos.y
				half4	tspace2 : TEXCOORD3;	//tspace W component includes wPos.z
				half4	vcolor2	: TEXCOORD4;	//VColor R compontnt contains Rougness Vertex Color, G component for Additional Mask
				float4	vcolor	: COLOR;		//vcolor W component contains Fog factor
                float4	vertex	: SV_POSITION;
				
            };

            sampler2D _MainTex, _NormalMap;
            float4 _MainTex_ST;
			float _Roughness, _F0, _FresnelPower, _NormalIntencity;

			uniform half4 unity_FogStart;
			uniform half4 unity_FogEnd;
			uniform half4 unity_FogDensity;


			half3 _LightingFunc(half3 color, half3 normal, half3 wpos, half2 uv2, half2 rogh)
			{
				//Regular Variables
				float3 normalDirection = normal;
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - wpos.xyz);

				//Diffuse Reflections
				float3 NdotL = saturate(dot(normal, lightDirection));

				//Indirect Specular
				//Sample Enironment Cube or Reflection Probe
				#if _USELIGHTINPUT_SLIDERS
				half cubeLod = (_Roughness * _Roughness) * UNITY_SPECCUBE_LOD_STEPS;
				#endif
				#if _USELIGHTINPUT_VERTEXCOLORS
				half cubeLod = (rogh * rogh) * UNITY_SPECCUBE_LOD_STEPS;
				#endif
				half3 reflectionVector = reflect(-viewDirection, normal);
				half4 cube = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectionVector, cubeLod);
				half3 decodeCube = DecodeHDR(cube, unity_SpecCube0_HDR);

				//Adding sampled cube to the fresnel using f0
				half fresnel = saturate(dot(normalize(normal), normalize(viewDirection)));
				fresnel = 1 - fresnel;
				fresnel = pow(fresnel, _FresnelPower); //Power of 5
				fresnel = lerp(_F0, 1.0f, fresnel);
				half3 IndirectSpecular = decodeCube * fresnel;

				//Apply Indirect Probe
				float3 indirectDiffuse = max(0.0f, ShadeSH9(half4(normal, 1)));
				
				//Apply baked indirect lightmap
				#ifdef LIGHTMAP_ON
					half3 lightmap = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, uv2.xy));
					indirectDiffuse = lightmap;
				#endif

				//Final Lighting Color
				half3 finalLight = indirectDiffuse + NdotL * _LightColor0.rgb + IndirectSpecular;

				return finalLight * color;
			}

            v2f vert (appdata v)
            {
                v2f o;
				o.vcolor.rgb = v.vcolor.rgb;
                o.vertex = UnityObjectToClipPos(v.vertex);

				//Packing wPos
				half3 wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.tspace0.w = wpos.x;
				o.tspace1.w = wpos.y;
				o.tspace2.w = wpos.z;

                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);

				//FOG Calculations
				float cameraVertDist = length(mul(UNITY_MATRIX_MV, v.vertex).xyz);
				o.vcolor.w = saturate((unity_FogEnd.x - cameraVertDist) / (unity_FogEnd.x - unity_FogStart.x));

				//Calculating normal, tangent and bitangent for normalmapping
				half3 worldNormal = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);		// Convert local normal to world normal
				half3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);								//Convert Local Tangents to World Tangents
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;							//Convert W param of Tangent to Unity world coordinate system
				half3 worldBitangent = cross(worldNormal, worldTangent) * tangentSign;					//Calculate Bitangents
				o.tspace0.xyz = half3(worldTangent.x, worldBitangent.x, worldNormal.x);
				o.tspace1.xyz = half3(worldTangent.y, worldBitangent.y, worldNormal.y);
				o.tspace2.xyz = half3(worldTangent.z, worldBitangent.z, worldNormal.z);

				#ifdef LIGHTMAP_ON
				o.uv.zw = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				#if _USELIGHTINPUT_VERTEXCOLORS
				o.vcolor2.r = v.uvRough.r;
				o.vcolor2.g = v.uvAdd.r;
				#endif

                return o;
            }

            fixed3 frag (v2f i) : SV_Target
            {
				//Use MainTexture and Vertex color to colorize Albedo
                half3 col = tex2D(_MainTex, i.uv.xy) * i.vcolor;

				//Normal mapping calculations
				half3 nrm = UnpackNormal(tex2D(_NormalMap, i.uv.xy)) * _NormalIntencity;
				half3 worldNormal;
				worldNormal.x = dot(i.tspace0, nrm);
				worldNormal.y = dot(i.tspace1, nrm);
				worldNormal.z = dot(i.tspace2, nrm);
				
				//Saving temporary UV2 in case none lightmapped
				half2 tmpUV2 = 0;
				#ifdef LIGHTMAP_ON
				tmpUV2 = i.uv.zw;
				#endif

				//Unpacking wPos
				half3 wpos = half3(i.tspace0.w, i.tspace1.w, i.tspace2.w);

				//Apply Lighting
				col = _LightingFunc(col.rgb, worldNormal, wpos, tmpUV2, i.vcolor2.r);
				
				//Apply fog
				//UNITY_APPLY_FOG(i.vcolor.w, col);
				col = lerp(unity_FogColor.rgb, col.rgb, i.vcolor.w);
                return col;
            }
            ENDCG
        }

		Pass
		{
			Name "META"
			Tags {"LightMode" = "Meta"}
			Cull Off
			CGPROGRAM

			#include"UnityStandardMeta.cginc"
			#include"Custom_Lightmapping.cginc"
			#pragma vertex vert_metaCustom
			#pragma fragment frag_metaCustom

			float _IndirectAlbedoBoost;

			float4 frag_metaCustom(v2f_metaCustom i) : SV_Target
			{
				FragmentCommonData data = UNITY_SETUP_BRDF_INPUT(i.uv);
				UnityMetaInput o;
				UNITY_INITIALIZE_OUTPUT(UnityMetaInput, o);
				fixed4 MainTex = tex2D(_MainTex, i.uv);
				o.Albedo = pow((fixed3(MainTex.rgb) * i.color), _IndirectAlbedoBoost);
				o.Emission = Emission(i.uv.xy);
				return UnityMetaFragment(o);
			}
			ENDCG
		}
    }
	CustomEditor "Environment_Base_CustomEditor"
}
