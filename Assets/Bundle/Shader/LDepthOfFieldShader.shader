Shader "PostProcessing/LDepthOfFieldShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Gauss ("Gauss", 2D) = "white" {}

        _MiddleDepth("MiddleDepth",Range(0,1)) = 1 
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _Gauss;
            sampler2D _CameraDepthTexture;

            fixed _MiddleDepth;

            struct appdata
            {
                float4 vertex : POSITION;
                fixed2 uv: TEXCOORD0;
            };

            struct v2f
            {
                fixed2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata i)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(i.vertex);
                o.uv = i.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,i.uv));
                depth = Linear01Depth(depth);
                depth = abs(depth - _MiddleDepth);

                return lerp(tex2D(_MainTex,i.uv),tex2D(_Gauss,i.uv),depth);
            }

            ENDCG
        }
    }
}
