Shader "PostProcessing/EdgeDetectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EdgesOnly ("EdgesOnly",Range(0,1)) = 1
        _EdgeColor ("EdgeColor",Color) = (1,1,1,1)
        _BackgroundColor ("BackgroundColor",Color) = (1,1,1,1)
        _EdgeFactor ("EdgeFactor",Range(0,3)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        ZTest Always
        Cull Off
        ZWrite Off
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
                float4 vertex : POSITION;
                half2  uv: TEXCOORD0;
            };

            struct v2f
            {
                half2 uv[9] : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            half4 _MainTex_TexelSize;
            fixed _EdgesOnly;
            fixed4 _EdgeColor;
            fixed4 _BackgroundColor;
            fixed _EdgeFactor;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                half2 uv = v.uv;

                o.uv[0] = uv + _MainTex_TexelSize.xy * half2(-1,-1);
                o.uv[1] = uv + _MainTex_TexelSize.xy * half2(-1,0);
                o.uv[2] = uv + _MainTex_TexelSize.xy * half2(-1,1);
                o.uv[3] = uv + _MainTex_TexelSize.xy * half2(0,-1);
                o.uv[4] = uv + _MainTex_TexelSize.xy * half2(0,0);
                o.uv[5] = uv + _MainTex_TexelSize.xy * half2(0,1);
                o.uv[6] = uv + _MainTex_TexelSize.xy * half2(1,-1);
                o.uv[7] = uv + _MainTex_TexelSize.xy * half2(1,0);
                o.uv[8] = uv + _MainTex_TexelSize.xy * half2(1,1);
                
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                const half Gx[9] = 
                {-1,-2,-1,
                0,0,0,
                1,2,1};

                const half Gy[9] = 
                {-1,0,1,
                -2,0,2,
                -1,0,1};

                half texColor;
                half edgeX = 0;
                half edgeY = 0;
                for(int it = 0;it<9;it++)
                {
                    fixed4 temp = tex2D(_MainTex,i.uv[it]);
                    texColor = 0.2125 * temp.r + 0.7154 * temp.g + 0.0721 * temp.b;
                    edgeX += texColor * Gx[it];
                    edgeY += texColor * Gy[it];
                }

                half edge = abs(edgeX)+abs(edgeY);

                fixed4 withEdgeColor = lerp(tex2D(_MainTex,i.uv[4]),_EdgeColor,edge*_EdgeFactor);
                fixed4 onlyEdgeColor = lerp(_BackgroundColor,_EdgeColor,edge*_EdgeFactor);

                return lerp(withEdgeColor,onlyEdgeColor,_EdgesOnly);
            }
            ENDCG
        }
    }
}
