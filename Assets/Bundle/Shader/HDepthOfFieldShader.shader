Shader "PostProcessing/HDepthOfFieldShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _IterationTex ("Texture", 2D) = "white"{}

        _BlurVertical ("BlurVertical",Int) = 1
        _MaxBlurSize("MaxBlurSize",Float) = 1 
        _FocalDepth("MiddleDepth",Range(0,1)) = 0.5
        _SegMin ("SegMin", Float) = 0
        _SetMax ("SegMax", Float) = 1
    }
    SubShader
    {
        CGINCLUDE

        #include "UnityCG.cginc"

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

        ENDCG


        Pass
        {
            NAME "Blur"

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment blurFrag

            sampler2D _MainTex;
            fixed4 _MainTex_TexelSize;
            sampler2D _CameraDepthTexture;

            int _BlurVertical;
            fixed _MaxBlurSize;
            fixed _FocalDepth;
            fixed _SegMin;
            fixed _SegMax;

            fixed4 segTest(fixed4 col,float depth)
            {
                float range1 = sign(depth - _SegMin);
                float range2 = sign(depth - _SegMax);
                range1 = abs(range1+range2);
                range1 = sign(range1);

                return (1-range1)*col + range1 * fixed4(1,1,1,0);
            }

            fixed4 gaussSampling(fixed2 uv,fixed weight)
            {
                fixed4 curColor = tex2D(_MainTex,uv);
                float samplingDepth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,uv)));
                curColor = segTest(curColor,samplingDepth);
                curColor *= weight;

                return curColor;
            }

            fixed4 blurFrag(v2f i) : SV_TARGET
            {
                fixed2 uv[5];
                
                uv[0] = i.uv;
                
                float middleDepth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,uv[0])));
                fixed blurSize = abs(_FocalDepth - middleDepth)*_MaxBlurSize;

                int xswitch = _BlurVertical;
                int yswitch = 1-_BlurVertical;

                uv[1] = i.uv + float2(
                xswitch * _MainTex_TexelSize.x*1,
                yswitch * _MainTex_TexelSize.y*1) * blurSize;

                uv[2] = i.uv - float2(
                xswitch * _MainTex_TexelSize.x*1,
                yswitch * _MainTex_TexelSize.y*1) * blurSize;

                uv[3] = i.uv + float2(
                xswitch * _MainTex_TexelSize.x*2,
                yswitch * _MainTex_TexelSize.y*2) * blurSize;

                uv[4] = i.uv - float2(
                xswitch * _MainTex_TexelSize.x*2,
                yswitch * _MainTex_TexelSize.y*2) * blurSize;

                float weight[3] = {0.4026,0.2442,0.0545};
                fixed4 sum = tex2D(_MainTex,uv[0]);
                sum = segTest(sum,middleDepth);
                sum *=  weight[0];

                for(int it=1;it<3;it++)
                {
                    int curIndex = 2*it-1;
                    sum += gaussSampling(uv[curIndex],weight[it]);

                    curIndex = curIndex+1;
                    sum += gaussSampling(uv[curIndex],weight[it]);
                }

                return sum;
            }

            ENDCG
        }

        Pass
        {
            NAME "Combine"

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment combineFrag

            sampler2D _MainTex;
            sampler2D _IterationTex;

            fixed4 combineFrag(v2f i) : SV_TARGET
            {
                fixed4 src = tex2D(_IterationTex,i.uv);
                fixed4 dest = tex2D(_MainTex,i.uv);
                return fixed4(src.rgb * src.a + dest.rgb * (1-src.a),1);
            }

            ENDCG
        }

        Pass
        {
            NAME "Init"

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment initFrag

            sampler2D _MainTex;

            fixed4 initFrag(v2f i) : SV_TARGET
            {
                return fixed4(1,1,1,1);
            }

            ENDCG
        }
    }
}
