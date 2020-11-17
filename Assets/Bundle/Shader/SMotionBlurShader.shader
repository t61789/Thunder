Shader "PostProcessing/SMotionBlurShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurAmount("BlurAmount",Float) = 1
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
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };

        sampler2D _MainTex;
        float _BlurAmount;

        v2f vert(appdata i)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(i.vertex);
            o.uv = i.uv;
            return o;
        }

        fixed4 fragRGB(v2f i) : SV_TARGET
        {
            return fixed4(tex2D(_MainTex,i.uv).rgb,_BlurAmount);
        }

        half4 fragA(v2f i) : SV_TARGET
        {
            return tex2D(_MainTex,i.uv);
        }

        ENDCG

        ZTest Always Cull Off ZWrite Off
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask RGB

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragRGB
            ENDCG
        }
        Pass
        {
            Blend One Zero
            ColorMask A
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragA
            ENDCG
        }
    }
}
