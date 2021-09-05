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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

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
                clip(col.a == 0 ? -1 : 1);

                float2 pixelPos = i.screenPos.xy / i.screenPos.w * _ScreenParams.xy;
                float threshold = thresholdMatrix[pixelPos.x % 4][pixelPos.y % 4] / 17;
                clip(col.a - threshold);

                // Darken horizontal faces
                float hor = 1 - abs(i.worldNormal.x);
                float ver = 1 - abs(i.worldNormal.y);

                col *= lerp(1, 0.8, hor);
                col *= lerp(1,0.6,ver);

                return col;
            }
            ENDCG
        }
    }
}
