Shader "PostProcessing/DepthTextureShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        //1.摄像机设置好正确的模式
        //Camera.main.depthTextureMode = DepthTextureMode.DepthNormals;
        //2.声明正确命名的变量
        //sampler2D _CameraDepthTexture;
        //3.解码
        //float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv);
        //float liearDepth = Linear01Depth(depth);
        //法线纹理的获取方法
        //1.摄像机设置好正确的模式
        //Camera.main.depthTextureMode = DepthTextureMode.DepthNormals;
        //2.声明正确命名的变量
        //sampler2D _CameraDepthNormalsTexture
        //3.解码
        //fixed3 normal = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv));
        //获取上面的图片只需要简单的输出他们即可

        //inline void DecodeDepthNormal(float4 _CameraDepthNormalsTexture,out float linear01Depth,out float3 normal)


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

            sampler2D _CameraDepthTexture;

            v2f vert (appdata i)
            {
                v2f o;
                o.vertex =  UnityObjectToClipPos(i.vertex);
                o.uv = i.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float d = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,i.uv));
                d = Linear01Depth(d);
                return fixed4(d,d,d,1);
            }
            ENDCG
        }
    }
}
