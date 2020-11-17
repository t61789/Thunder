Shader "PostProcessing/GaussBlurShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("BlurSize",Float) = 1
    }
    SubShader
    {
        CGINCLUDE

        #include "UnityCG.cginc"

        struct appdata
        {
            float4 vertex : POSITION;
            half2 uv: TEXCOORD0;
        };

        struct v2f
        {
            half2 uv[5] : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };

        half4 _MainTex_TexelSize;
        float _BlurSize;

        sampler2D _MainTex;
        float4 _MainTex_ST;

        v2f vertv (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv[0] = v.uv;
            o.uv[1] = v.uv + float2(0,_MainTex_TexelSize.y*1) * _BlurSize;
            o.uv[2] = v.uv - float2(0,_MainTex_TexelSize.y*1) * _BlurSize;
            o.uv[3] = v.uv + float2(0,_MainTex_TexelSize.y*2) * _BlurSize;
            o.uv[4] = v.uv - float2(0,_MainTex_TexelSize.y*2) * _BlurSize;

            return o;
        }

        v2f verth (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv[0] = v.uv;
            o.uv[1] = v.uv + float2(_MainTex_TexelSize.x*1,0) * _BlurSize;
            o.uv[2] = v.uv - float2(_MainTex_TexelSize.x*1,0) * _BlurSize;
            o.uv[3] = v.uv + float2(_MainTex_TexelSize.x*2,0) * _BlurSize;
            o.uv[4] = v.uv - float2(_MainTex_TexelSize.x*2,0) * _BlurSize;

            return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {
            float weight[3] = {0.4026,0.2442,0.0545};
            fixed3 sum = tex2D(_MainTex,i.uv[0]).rgb * weight[0];

            for(int it=1;it<3;it++)
            {
                sum += tex2D(_MainTex,i.uv[2*it-1]).rgb * weight[it];
                sum += tex2D(_MainTex,i.uv[2*it]).rgb * weight[it];
            }

            return fixed4(sum,1);
        }
        ENDCG

        ZTest Always Cull Off ZWrite Off

        Pass
        {
            NAME "GAUSS_BLUR_VERTICAL"

            CGPROGRAM

            #pragma vertex vertv
            #pragma fragment frag

            ENDCG
        }

        Pass
        {
            NAME "GAUSS_BLUR_HORIZON"

            CGPROGRAM

            #pragma vertex verth
            #pragma fragment frag

            ENDCG
        }
    }
}
