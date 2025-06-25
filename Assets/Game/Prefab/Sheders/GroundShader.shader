Shader "Custom/GroundShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1) // Просто білий колір
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+2" } // Ваші теги цілком нормальні
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

            fixed4 _Color; // Наша змінна кольору

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color; // Повертаємо просто колір
            }
            ENDCG
        }
    }
}