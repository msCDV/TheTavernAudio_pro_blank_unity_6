Shader "Hidden/SunShaftsPostProcess"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float4 _SunScreenPos; // xy = pozycja słońca, z = widoczność (fade przy odwracaniu)
            float4 _SunColor;
            float _Density;
            float _Weight;
            float _Decay;
            float _Exposure;

            #define NUM_SAMPLES 64

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 originalColor = tex2D(_MainTex, i.uv);

                // Gdy słońce jest za plecami, post-process jest w 100% wyłączony
                if (_SunScreenPos.z <= 0.001 || _Exposure <= 0.001)
                    return originalColor;

                float2 deltaTextCoord = (i.uv - _SunScreenPos.xy);
                deltaTextCoord *= (1.0 / float(NUM_SAMPLES)) * _Density;

                float2 textCoo = i.uv;
                float illuminationDecay = 1.0;
                fixed4 rays = fixed4(0, 0, 0, 0);

                for (int s = 0; s < NUM_SAMPLES; s++)
                {
                    textCoo -= deltaTextCoord;

                    // Zabezpieczenie przed wychodzeniem poza krawędź ekranu
                    if (textCoo.x < 0.0 || textCoo.x > 1.0 || textCoo.y < 0.0 || textCoo.y > 1.0)
                        break;

                    float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, textCoo));

                    // Światło pobieramy wyłącznie z otwartego nieba
                    fixed4 sampleCol = (depth > 0.99) ? _SunColor : fixed4(0, 0, 0, 0);

                    sampleCol *= illuminationDecay * _Weight;
                    rays += sampleCol;
                    illuminationDecay *= _Decay;
                }

                // Miękki tonemapping Reinhard: brak białej łuny, naturalny blask
                fixed4 finalRays = rays * (_Exposure * _SunScreenPos.z);
                finalRays.rgb = finalRays.rgb / (1.0 + finalRays.rgb);

                return originalColor + finalRays;
            }
            ENDCG
        }
    }
    FallBack Off
}