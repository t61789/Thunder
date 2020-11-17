Shader "Unlit/RingShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Radius("Radius",float) = 1
        _Width("Width",float) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType "="Transparent"}
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                fixed3 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            fixed _Radius;
            fixed _Width;

            v2f vert (appdata v)
            {
                v2f o;

                fixed offset = distance(v.vertex,fixed3(0,0,0));
                offset = offset - 1;
                if(offset<0)
                    offset = _Radius - _Width/2;           
                else
                    offset = _Radius + _Width/2;           

                o.vertex = UnityObjectToClipPos(offset * normalize(v.vertex));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
