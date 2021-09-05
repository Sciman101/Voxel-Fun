Shader "Unlit/BlockShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                half3 worldNormal : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.screenPos = ComputeScreenPos(o.vertex);

                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 frag(v2f i) : SV_Target
            {
                // Used for dither fade
                const float4x4 thresholdMatrix = {
                    1, 9, 3, 11,
                    13, 5, 15, 7,
                    4, 12, 2, 10,
                    16, 8, 14, 6
                };

                fixed4 col = tex2D(_MainTex, i.uv);

                float2 pixelPos = i.screenPos.xy / i.screenPos.w * _ScreenParams.xy;
                float threshold = thresholdMatrix[pixelPos.x % 4][pixelPos.y % 4] / 17;
                clip(col.a - threshold);

                // Darken horizontal faces
                col *= abs(i.worldNormal.x) > 0.99 ? 0.8 : 1;
                col *= abs(i.worldNormal.z) > 0.99 ? 0.6 : 1;

                return col;
            }
            ENDCG
        }
    }
}
