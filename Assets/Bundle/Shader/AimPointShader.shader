Shader "Unlit/AimPointShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MiddleOffset ("MiddleOffset",Range(0,1)) = 0.5
        _TransparentMag ("TransparentMag",Range(0,1)) = 1
        _Color ("Color",Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        GrabPass{}

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
                float4 grabUV : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _MiddleOffset;
            float _TransparentMag;
            sampler2D _GrabTexture;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.grabUV = ComputeGrabScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 grabCol = tex2Dproj(_GrabTexture,i.grabUV);
                float gray = grabCol.r * 0.299 + grabCol.g * 0.587 + grabCol.b * 0.114;

                gray = gray *2 -1;
                gray = -(sign(gray) * (1-abs(gray)) * _MiddleOffset + gray);

                return fixed4(gray,gray,gray,col.a*_TransparentMag);
            }
            ENDCG
        }
    }
}
