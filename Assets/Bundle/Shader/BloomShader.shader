Shader "PostProcessing/BloomShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Bloom ("Bloom", 2D) = "white" {}
        _LuminanceThreshold ("LiminanceThreshold",Range(0,4)) = 0.6
        _BlurSize ("BlurSize",Float) = 1
    }
    SubShader
    {
        CGINCLUDE

        sampler2D _MainTex;
        half4 _MainTex_TexelSize;
        sampler2D _Bloom;
        float _LuminanceThreshold;
        float _BlurSize;

        struct appdata
        {
            float4 vertex : POSITION;
            half2  uv: TEXCOORD0;
        };

        struct v2f
        {
            float4 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };

        v2f vertBright(appdata i)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(i.vertex);
            o.uv.xy = o.uv.zw =i.uv;
            return o;
        }

        fixed4 fragBright(v2f i) : SV_TARGET
        {
            fixed4 c = tex2D(_MainTex,i.uv.xy);
            fixed luminance = 0.2125 * c.r + 0.7154 * c.g + 0.0721 * c.b;
            fixed4 val = clamp(c-_LuminanceThreshold,0,1);

            return c*val;
        }

        v2f vertBloom(appdata i)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(i.vertex);
            o.uv.xy = o.uv.zw = i.uv;
            #if UNITY_UV_STARTS_AT_TOP
            if(_MainTex_TexelSize.y<0)
                o.uv.w = 1- o.uv.w;
            #endif
            return o;
        }

        fixed4 fragBloom(v2f i) : SV_TARGET
        {
            return tex2D(_MainTex,i.uv.xy)+tex2D(_Bloom,i.uv.zw);
        }

        ENDCG

        ZTest Always Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vertBright
            #pragma fragment fragBright
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vertBloom
            #pragma fragment fragBloom
            ENDCG
        }
    }
}
