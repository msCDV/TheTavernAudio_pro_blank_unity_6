Shader "Custom/StylizedVolumetricBeam"
{
    Properties
    {
        [HDR] _Color ("Kolor Swiatla", Color) = (1.0, 0.82, 0.5, 1.0)
        _Intensity ("Intensywnosc", Range(0.1, 5.0)) = 1.5
        _LengthFalloff ("Zanik na dlugosci", Range(0.2, 4.0)) = 1.2
        _SideFalloff ("Zanik na bokach (Fresnel)", Range(0.2, 4.0)) = 1.5
        _DepthFade ("Miekki styk z geometria", Range(0.1, 5.0)) = 1.2
        _CameraFade ("Miekki styk z kamera", Range(0.1, 3.0)) = 0.8
        _NoiseTex ("Tekstura szumu / smug (opcjonalna)", 2D) = "white" {}
        _ScrollSpeed ("Predkosc ruchu smug", Vector) = (0.01, 0.04, 0, 0)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend One One          // Additive blend (swiatlo dodaje sie do tla)
        Cull Off               // Widoczne rowniez od srodka
        ZWrite Off
        ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _CameraDepthTexture;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            fixed4 _Color;
            float _Intensity;
            float _LengthFalloff;
            float _SideFalloff;
            float _DepthFade;
            float _CameraFade;
            float4 _ScrollSpeed;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float3 worldNormal : NORMAL;
                float3 viewDir : TEXCOORD3;
                float eyeDepth : TEXCOORD4;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.pos);

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                
                COMPUTE_EYEDEPTH(o.eyeDepth);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 1. Zanik wzdluz dlugosci wiazki (UV.y idzie od zrodla do konca)
                float lengthFactor = pow(saturate(1.0 - i.uv.y), _LengthFalloff);

                // 2. Miekki fresnel na bokach stozka (brak twardych krawedzi mesha)
                float normalDot = abs(dot(normalize(i.worldNormal), normalize(i.viewDir)));
                float sideFactor = pow(normalDot, _SideFalloff);

                // 3. Miekki styk ze scianami, podloga i dachem (Soft Depth Fade)
                float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
                float depthDifference = sceneDepth - i.eyeDepth;
                float depthFade = saturate(depthDifference / _DepthFade);

                // 4. Zanik przed sama kamera (aby promiec nie uderzal nagle w obiektyw)
                float cameraFade = saturate((i.eyeDepth - _ProjectionParams.y) / _CameraFade);

                // 5. Animowany szum imitujacy drobinki pylu i powietrze
                float2 noiseUV = i.uv * _NoiseTex_ST.xy + _Time.y * _ScrollSpeed.xy;
                fixed4 noiseCol = tex2D(_NoiseTex, noiseUV);

                float alpha = lengthFactor * sideFactor * depthFade * cameraFade * noiseCol.r;

                return _Color * (_Intensity * alpha);
            }
            ENDCG
        }
    }
    FallBack Off
}