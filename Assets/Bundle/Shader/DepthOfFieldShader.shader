Shader "PostProcessing/DepthOfFieldShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _BlurVertical ("BlurVertical",Int) = 1
        _MaxBlurSize("MaxBlurSize",Float) = 1 
        _FocalDepthPos("FocalDepthPos",Vector) = (0.5,0.5,0,0)
        _DistanceThreshold ("DistanceThreshold",Float) = 0.1
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
            fixed4 _MainTex_TexelSize;
            sampler2D _CameraDepthTexture;

            int _BlurVertical;
            fixed _MaxBlurSize;
            fixed _DistanceThreshold;
            fixed4 _FocalDepthPos;

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

            fixed3 mixTest(fixed2 uv,fixed weight,fixed middleDepth,fixed3 middleCol)
            {
                //float depth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,uv)));
                //depth = depth - middleDepth;
                //depth = clamp(sign(depth),0,1);

                //return tex2D(_MainTex,uv).rgb * weight * depth +
                //middleCol* weight*(1-depth);
                //float depth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,uv)));
                //depth = depth - middleDepth;
                //float switcher = clamp(sign(depth),0,1);
                fixed3 col = tex2D(_MainTex,uv).rgb;
                //col = col * switcher + middleCol * (1-switcher);
                //lerp(col,middleCol,abs(depth) * _DistanceThreshold)*(1-switcher);

                return col* weight;
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                fixed2 uv[5];
                
                uv[0] = i.uv;
                
                float middleDepth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,uv[0])));
                float focalDepth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,_FocalDepthPos.xy)));
                fixed blurSize = abs(focalDepth - middleDepth)*_MaxBlurSize;

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
                fixed3 sum = tex2D(_MainTex,uv[0]).rgb;
                fixed3 middleCol = sum;
                sum *=  weight[0];

                for(int it=1;it<3;it++)
                {
                    int curIndex = 2*it-1;
                    //sum += tex2D(_MainTex,uv[curIndex]).rgb * weight[it];
                    sum += mixTest(uv[curIndex],weight[it],middleDepth,middleCol);

                    curIndex = curIndex+1;
                    //sum += tex2D(_MainTex,uv[curIndex]).rgb * weight[it];
                    sum += mixTest(uv[curIndex],weight[it],middleDepth,middleCol);
                }
                return fixed4(sum,1);
            }

            ENDCG
        }
    }
}
