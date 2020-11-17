Shader "PostProcessing/SsaoShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Noise ("Noise",2D) = "white" {}
        _SamplerRange("SamplerRange",Range(0,1)) = 0.01
        _SamplerFactor ("SamplerFactor",Float) = 1
        _Contrast ("Contrast",Float) = 1
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
            sampler2D _Noise;
            sampler2D _CameraDepthTexture;
            sampler2D _CameraDepthNormalsTexture;

            float _SamplerRange;
            float _SamplerFactor;
            float _Contrast;

            float4x4 _InverseProjection;

            v2f vert (appdata i)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(i.vertex);
                o.uv = i.uv;
                return o;
            }

            fixed getOcclusion(fixed3 normal,fixed depth,fixed3 rand,float4x4 gs)
            {
                rand -= fixed3(0.5,0.5,0.5);
                rand.z = abs(rand.z);
                rand = normalize(rand)*_SamplerRange;
                fixed4 offsetUV = mul(fixed4(rand,1),gs);
                offsetUV = offsetUV/offsetUV.w;
                fixed offsetDepth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,offsetUV.xy)));
                fixed distance = offsetUV.z - offsetDepth ;
                return clamp(sign(distance),0,1)*(1/(1+distance*_SamplerFactor));
            }

            float4 frag (v2f i) : SV_Target
            {
                float depth;
                float3 normal;
                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture,i.uv),depth,normal);
                normal.z = -normal.z;

                float4 middle = mul(_InverseProjection,fixed4(normal,1));
                middle = middle/middle.w;
                middle = mul(UNITY_MATRIX_P,middle);
                middle = middle/middle.w;

                middle.z = -middle.z;

                //fixed3 z = normal;
                //fixed3 x = normalize(fixed3(0,0,1)-(normal * dot(normal,fixed3(0,0,1))));
                //fixed3 y = cross(x,z);
                //float4x4 gs = float4x4(float4(x,0),float4(y,0),float4(z,0),float4(float3(i.uv,depth),1));

                //fixed count = 0;
                //fixed4 rand = fixed4(tex2D(_Noise,i.uv+0.01).rgb,1);
                //rand = mul(rand,UNITY_MATRIX_P);
                //rand = rand/rand.w;
                //count += getOcclusion(normal,depth,rand,gs);

                //rand = tex2D(_Noise,i.uv+0.02).rgb;
                //count += getOcclusion(normal,depth,rand,gs);

                //rand = tex2D(_Noise,i.uv+0.03).rgb;
                //count += getOcclusion(normal,depth,rand,gs);

                //rand = tex2D(_Noise,i.uv+0.04).rgb;
                //count += getOcclusion(normal,depth,rand,gs);

                //rand = tex2D(_Noise,i.uv+0.05).rgb;
                //count += getOcclusion(normal,depth,rand,gs);

                //rand = tex2D(_Noise,i.uv+0.06).rgb;
                //count += getOcclusion(normal,depth,rand,gs);

                //rand = tex2D(_Noise,i.uv+0.07).rgb;
                //count += getOcclusion(normal,depth,rand,gs);

                //rand = tex2D(_Noise,i.uv+0.08).rgb;
                //count += getOcclusion(normal,depth,rand,gs);

                //count = count/8;
                //count = 1-count;

                //return fixed4(count,count,count,1);

                //float offsetDepth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,i.uv+float2(0.01*_SamplerRange,0))));
                //offsetDepth = abs(depth - offsetDepth);
                //((-((((x^y*2)-1)^2))+1)^z
                //offsetDepth = pow((-(pow(((pow(offsetDepth,_SamplerFactor))*2)-1,2)))+1,_Contrast);
                
                return middle;
            }
            
            //fixed getOcclusion(fixed2 uv,fixed2 uvOffset,fixed3 normal,fixed depth)
            //{
            //    fixed offsetDepth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,uv+uvOffset)));
            //    fixed3 samplerDir = fixed3(uv+uvOffset,offsetDepth) - fixed3(uv,depth);
            //    samplerDir.z = -samplerDir.z;
            //    return (1-(clamp(distance(normal,normalize(samplerDir)),0,1.414)/2))*(1.0/(1.0+length(samplerDir)*_SamplerFactor));
            //    //return normalize(samplerDir);
            //}

            //fixed4 frag (v2f i) : SV_Target
            //{
            //    fixed result = 0;

            //    fixed depth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,i.uv)));
            //    fixed3 normal = normalize(DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv)));

            //    fixed4 rand1 = tex2D(_Noise, i.uv);
            //    rand1 = rand1 - fixed4(0.5,0.5,0.5,0.5);
            //    fixed4 rand2 = tex2D(_Noise, i.uv+0.1);
            //    rand2 = rand2 - fixed4(0.5,0.5,0.5,0.5);

            //    fixed2 uvOffset = fixed2(rand1.x,rand2.x) *_SamplerRange*(1-depth)*_Contrast;
            //    result += getOcclusion(i.uv,uvOffset,normal,depth);

            //    uvOffset = fixed2(rand1.y,rand2.y)*_SamplerRange*(1-depth)*_Contrast;
            //    result += getOcclusion(i.uv,uvOffset,normal,depth);

            //    uvOffset = fixed2(rand1.z,rand2.z)*_SamplerRange*(1-depth)*_Contrast;
            //    result += getOcclusion(i.uv,uvOffset,normal,depth);

            //    uvOffset = fixed2(rand1.w,rand2.w)*_SamplerRange*(1-depth)*_Contrast;
            //    result += getOcclusion(i.uv,uvOffset,normal,depth);

            //    rand1 = tex2D(_Noise, i.uv+0.2);
            //    rand1 = rand1 - fixed4(0.5,0.5,0.5,0.5);
            //    rand2 = tex2D(_Noise, i.uv+0.3);
            //    rand2 = rand2 - fixed4(0.5,0.5,0.5,0.5);

            //    uvOffset = fixed2(rand1.x,rand2.x) *_SamplerRange*(1-depth)*_Contrast;
            //    result += getOcclusion(i.uv,uvOffset,normal,depth);

            //    uvOffset = fixed2(rand1.y,rand2.y) *_SamplerRange*(1-depth)*_Contrast;
            //    result += getOcclusion(i.uv,uvOffset,normal,depth);

            //    uvOffset = fixed2(rand1.z,rand2.z) *_SamplerRange*(1-depth)*_Contrast;
            //    result += getOcclusion(i.uv,uvOffset,normal,depth);

            //    uvOffset = fixed2(rand1.w,rand2.w) *_SamplerRange*(1-depth)*_Contrast;
            //    result += getOcclusion(i.uv,uvOffset,normal,depth);

            //    result = result/8;
            //    result = 1-result;
            //    //result = clamp(result - (1-result)*_Contrast,0,1);

            //    return fixed4(result,result,result,1);
            //}
            ENDCG
        }
    }
}
