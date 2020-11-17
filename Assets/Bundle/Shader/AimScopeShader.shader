Shader "UI/AimScopeShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AimScope ("AimScope", 2D) = "white" {}
        _UVXScale ("UVXScale", Float) = 1
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

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _AimScope;
            float _UVXScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                i.uv.x = ((i.uv.x*2-1)*_UVXScale+1)/2;
                fixed4 scopeCol = tex2D(_AimScope, i.uv);

                return fixed4(lerp(col.rgb,scopeCol.rgb,scopeCol.a),1);
                //return col;
            }
            ENDCG
        }
    }
}
