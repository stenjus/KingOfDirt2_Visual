// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "King of Dirt 2/Wires Normals"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4	vertex	: POSITION;
                float2	uv		: TEXCOORD0;
				float3	normal	: NORMAL;
				float4	vcolor	: COLOR;
            };

            struct v2f
            {
                float2	uv		: TEXCOORD0;
				half3	normal	: TEXCOORD1;
				half4	vcolor	: COLOR;
                float4	vertex	: SV_POSITION;
				UNITY_FOG_COORDS(1)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.vcolor = v.vcolor;
				half3 worldNormals = v.normal;
				//worldNormals *= half3(1, 0, 0) * (1 - v.uv.y);
				worldNormals *= half3(1, 0, 0) * v.uv.y;
				o.normal = mul((float4x4)unity_ObjectToWorld, worldNormals);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				//Lights
				float3 normalDirection = i.normal;
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);

				half NDotL = saturate(dot(normalDirection, lightDirection));

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
				return NDotL;
            }
            ENDCG
        }
    }
}
