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
            float4 _SunScreenPos;
            float4 _SunColor;
            float _Density;
            float _Weight;
            float _Decay;
            float _Exposure;

            #define NUM_SAMPLES 64

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 originalColor = tex2D(_MainTex, i.uv);

                float2 textCoo = i.uv;
                float2 toSun = (textCoo - _SunScreenPos.xy);

                // Ograniczamy rozciąganie wektora, gdy słońce jest daleko poza ekranem,
                // dzięki czemu smugi mają stałą, równoległą długość niezależnie od obrotu kamery
                float dist = length(toSun);
                if (dist > 1.2)
                {
                    toSun = (toSun / dist) * 1.2;
                }

                float2 deltaTextCoord = toSun * ((1.0 / float(NUM_SAMPLES)) * _Density);

                float illuminationDecay = 1.0;
                fixed4 rays = fixed4(0, 0, 0, 0);

                for (int s = 0; s < NUM_SAMPLES; s++)
                {
                    textCoo -= deltaTextCoord;

                    // Jeśli próbka wychodzi poza ekran, nie generujemy fałszywego światła
                    if (textCoo.x < 0.0 || textCoo.x > 1.0 || textCoo.y < 0.0 || textCoo.y > 1.0)
                        break;

                    float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, textCoo));
                    fixed4 sampleCol = (depth > 0.98) ? _SunColor : fixed4(0, 0, 0, 0);

                    sampleCol *= illuminationDecay * _Weight;
                    rays += sampleCol;
                    illuminationDecay *= _Decay;
                }

                return originalColor + (rays * _Exposure);
            }
            ENDCG
        }
    }
    FallBack Off
}