Shader "Unlit/spriteShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BumpMap("BumpMax",2D) = "bump"{}
        _BumpScale("BumpScale",float) = 1.0
        _Color("Color",Color) = (0,0,0,1)
        _LineColor("LineColor",Color) = (0,0,0,1)
        _Offset("LineOffset",Range(0,0.02)) = 0
        _AplhaThreaShord("AplhaThreaShord",Range(0,1)) = 0
        _Specluar("Specluar",Color) = (0,0,0,1)
        _Gloss("Gloss",Range(8.0,256)) = 20
        _GlobalAlpha("GlobalAlpha",Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "LightMode"="ForwardBase" "IgnoreProjector"="True" "RenderType "="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            sampler2D _MainTex;
            sampler2D _BumpMap;
            float4 _LineColor;
            float4 _Color;
            float _Offset;
            float _AplhaThreaShord;
            float _BumpScale;
            float4 _Specluar;
            float _Gloss;
            float _GlobalAlpha;

            struct a2v
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float3 normal:NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 lightDir : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };

            void vert(in a2v v, out v2f o)
            {
                TANGENT_SPACE_ROTATION;

                o.lightDir = mul(rotation,ObjSpaceLightDir(v.vertex)).xyz;
                o.viewDir = mul(rotation,ObjSpaceViewDir(v.vertex)).xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv.xy;
            }

            float4 frag (v2f i) : SV_Target
            {
                i.lightDir = normalize(i.lightDir);
                i.viewDir = normalize(i.viewDir);

                fixed3 tangentNormal = UnpackNormal(tex2D(_BumpMap,i.uv));
                tangentNormal.xy *= _BumpScale;
                tangentNormal.z = sqrt(1.0-saturate(dot(tangentNormal.xy,tangentNormal.xy)));

                fixed4 albedo = tex2D(_MainTex,i.uv)*_Color;

                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo.rgb;

                fixed3 diffuse = _LightColor0.rgb * albedo.rgb * max(0,dot(tangentNormal,i.lightDir));

                fixed3 halfDir = normalize(i.lightDir+i.viewDir);

                fixed3 specluar = _LightColor0.rgb * _Specluar.rgb * pow(max(0,dot(tangentNormal,halfDir)),_Gloss);

                float4 origin = float4(ambient+diffuse+specluar,albedo.a);

                fixed alpha = origin.a;
                if(alpha<_AplhaThreaShord)
                {
                    alpha= 
                    tex2D(_MainTex,i.uv+ float2(0,-_Offset)).a+ 
                    tex2D(_MainTex,i.uv+ float2(0,_Offset)).a+ 
                    tex2D(_MainTex,i.uv+ float2(-_Offset,0)).a+ 
                    tex2D(_MainTex,i.uv+ float2(_Offset,0)).a;
                    origin.rgb = _LineColor;
                    origin.a = alpha;
                }

                return float4(origin.rgb,origin.a*_GlobalAlpha);
            }
            ENDCG
        }
    }
    FALLBACK "Diffuse"
}
