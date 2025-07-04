Shader "Custom/GroundShader"
{
    Properties
    {
        _Color ("Tint Color", Color) = (1,1,1,1) // Колір, який буде тонувати текстуру
        _MainTex ("Texture", 2D) = "white" {} // <<< НОВА ВЛАСТИВІСТЬ: Основна текстура
        _Tiling ("Tiling", Vector) = (1,1,0,0) // <<< НОВА ВЛАСТИВІСТЬ: Для повторення текстури (тайлінгу)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+2" } 
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc" // Містить корисні функції Unity (наприклад, UnityObjectToClipPos)

            struct appdata
            {
                float4 vertex : POSITION; // Позиція вершини в локальному просторі об'єкта
                float2 uv : TEXCOORD0;    // UV-координати вершини (для текстурування)
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;    // UV-координати, що передаються у фрагментний шейдер
                float4 vertex : SV_POSITION; // Позиція вершини в просторі відсікання (для рендерингу)
            };

            // <<< НОВІ ЗМІННІ ШЕЙДЕРА >>>
            sampler2D _MainTex; // Декларація нашої текстури
            float4 _MainTex_ST; // Автоматично генерується Unity для _MainTex, містить тайлінг та офсет (_Tiling)
            // (Деталь: _MainTex_ST = (TilingX, TilingY, OffsetX, OffsetY))
            // ------------------------

            fixed4 _Color; // Наша змінна кольору (для тонування)


            v2f vert (appdata v)
            {
                v2f o;
                // Перетворення позиції вершини з локального простору в простір відсікання
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // <<< ЗМІНА ТУТ: Застосовуємо тайлінг та офсет до UV-координат >>>
                // TRANSFORM_TEX - це макрос Unity, який автоматично застосовує _MainTex_ST до UV
                o.uv = TRANSFORM_TEX(v.uv, _MainTex); 
                // ---------------------------------------------------------------
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // <<< ЗМІНА ТУТ: Беремо колір з текстури та множимо на _Color >>>
                // tex2D - функція для семплювання (читання) кольору з 2D-текстури за заданими UV-координатами
                fixed4 texColor = tex2D(_MainTex, i.uv);
                
                // Перемножуємо колір текстури на колір тонування.
                // Результатом є колір текстури, забарвлений вашим _Color.
                return texColor * _Color; 
                // -----------------------------------------------------------
            }
            ENDCG
        }
    }
}