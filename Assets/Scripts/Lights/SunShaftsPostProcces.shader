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
            float4 _SunScreenPos; // xy = pozycja słońca, z = radialBlend
            float4 _SunRayDir;    // xy = kierunek promieni przy obrocie
            float4 _SunColor;
            float _Density;
            float _Weight;
            float _Decay;
            float _Exposure;

            #define NUM_SAMPLES 64

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 originalColor = tex2D(_MainTex, i.uv);

                // Oryginalny wektor ze starego shadera (gdy patrzysz na słońce)
                float2 radialDir = (i.uv - _SunScreenPos.xy);

                // Stały wektor kierunkowy (gdy odwrócisz się bokiem lub tyłem)
                float2 parallelDir = _SunRayDir.xy * 0.7;

                // Płynne przejście między nimi
                float2 deltaTextCoord = lerp(parallelDir, radialDir, _SunScreenPos.z);
                deltaTextCoord *= (1.0 / float(NUM_SAMPLES)) * _Density;

                float2 textCoo = i.uv;
                float illuminationDecay = 1.0;
                fixed4 rays = fixed4(0, 0, 0, 0);

                // Dokładnie ta sama pętla co w pierwszym shaderze
                for (int s = 0; s < NUM_SAMPLES; s++)
                {
                    textCoo -= deltaTextCoord;

                    // Zabezpieczenie krawędzi ekranu przy obrocie kamery
                    float2 sampleUV = saturate(textCoo);

                    float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampleUV));
                    fixed4 sampleCol = (depth > 0.99) ? _SunColor : fixed4(0, 0, 0, 0);

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