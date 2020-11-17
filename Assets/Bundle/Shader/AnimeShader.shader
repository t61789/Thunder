// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/AnimeShader"
{
    Properties
    {
        _Outline("Outline",float) = 0
        _MainTex ("Texture", 2D) = "white" {}
        _Light ("Light",Color) = (1,1,1,1)
        //_Diffuse ("Diffuse",Color) = (1,1,1,1)
        _Specular ("Specular",Color) = (1,1,1,1)
        _SpecularStep ("SpecularStep",Range(0,1)) = 0.5
        _Gloss ("Gloss",float) = 1
        _Heri("Heri",float) = 3
        _HeriFactor("HeriFactor",Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
        LOD 100

        Pass
        {
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float _Outline;

            v2f vert (appdata v)
            {
                v2f o;

                v.vertex = mul(UNITY_MATRIX_MV , v.vertex);
                v.normal = mul(UNITY_MATRIX_IT_MV,v.normal);
                v.normal = normalize(v.normal) * _Outline;
                v.vertex.xyz += v.normal;
                o.vertex = mul(UNITY_MATRIX_P,v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(0,0,0,1.0);
            }
            ENDCG
        }

        Pass
        {
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
           // #pragma multi_compile_fwdbase	


            #include "UnityCG.cginc"
            //#include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                fixed3 worldPos : TEXCOORD1;
                float3 normal : TEXCOORD2;
                float4 vertex : SV_POSITION;
                //SHADOW_COORDS(3)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Light;
            //fixed3 _Diffuse;
            fixed3 _Specular;
            float _Gloss;
            float _Heri;
            float _HeriFactor;
            float _SpecularStep;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld,v.vertex);
                o.normal = mul(v.normal,unity_WorldToObject);
                //TRANSFER_SHADOW(o);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 tex = tex2D(_MainTex, i.uv);
                //fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT;
                i.normal = normalize(i.normal);
                fixed3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

                //ambient += _Light * _Diffuse * max(0,dot(i.normal,lightDir));
                fixed3 h = normalize(lightDir + normalize(_WorldSpaceCameraPos-i.worldPos));
                //ambient+= _Light * _Specular * pow(max(0,dot(i.normal,h)),_Gloss);
                fixed highLight = _Specular * pow(max(0,dot(i.normal,h)),_Gloss);
                highLight = step(_SpecularStep,highLight) * _SpecularStep;

                float halfLambert = 0.5 * dot(i.normal,lightDir) + 0.5;
                halfLambert = ceil(halfLambert* _Heri)/_Heri;
                //ambient *= halfLambert;
                halfLambert += (1-halfLambert)*_HeriFactor;
                //fixed s= SHADOW_ATTENUATION(i);
                return tex  * _Light* halfLambert + highLight;//* s;
            }
            ENDCG
            
        }
        
    }
    FallBack "Diffuse"
}
