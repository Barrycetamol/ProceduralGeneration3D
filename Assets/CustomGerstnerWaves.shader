Shader "Custom/GerstnerWaves"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.0, 0.5, 1.0, 1.0)
        _WaveCount ("Wave Count", Float) = 3
        _WaveDirections ("Wave Directions", Vector) = (1.0, 0.5, 0.2, 0.0)
        _WaveFrequencies ("Wave Frequencies", Vector) = (0.8, 0.5, 0.0, 0.0)
        _WaveAmplitudes ("Wave Amplitudes", Vector) = (0.2, 0.15, 0.1, 0.0)
        _WaveSpeeds ("Wave Speeds", Vector) = (0.5, 0.6, 0.4, 0.0)
        _WaveLength ("Wave Length", Float) = 20.0
        _WindDirection ("Wind Direction", Vector) = (1.0, 0.5, 0.0, 0.0)
        _WindStrength ("Wind Strength", Float) = 0.2

        // Color banding properties
        _ColorBands ("Color Bands", Color)[] = {}
        _Thresholds ("Thresholds", Float)[] = {}
        _BandCount ("Band Count", Float) = 4
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            float _WaveCount;
            float4 _WaveDirections;
            float4 _WaveFrequencies;
            float4 _WaveAmplitudes;
            float4 _WaveSpeeds;
            float _WaveLength;
            float4 _WindDirection;
            float _WindStrength;

            float4 _BaseColor;
            float4 _ColorBands[10]; // Max 10 bands for simplicity
            float _Thresholds[10];
            float _BandCount;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));

                // Gerstner wave calculations
                float3 position = o.worldPos;
                float y = 0.0;

                float2 windDir = normalize(_WindDirection.xy) * _WindStrength;

                for (int i = 0; i < 4; i++) // Up to 4 waves (can be adjusted)
                {
                    float2 waveDir = normalize(float2(_WaveDirections[i], _WaveDirections[(i + 1) % 4]));
                    waveDir += windDir;
                    waveDir = normalize(waveDir);

                    float amplitude = _WaveAmplitudes[i];
                    float frequency = _WaveFrequencies[i];
                    float speed = _WaveSpeeds[i];

                    float k = 2.0 * UNITY_PI / _WaveLength;
                    float w = frequency * k;

                    float phase = w * _Time.y * speed;
                    float wave = sin(dot(waveDir, position.xz) * k + phase) * amplitude;

                    y += wave;
                }

                position.y += y;
                o.pos = UnityObjectToClipPos(float4(position, 1.0));

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Color banding based on height
                float height = i.worldPos.y;
                fixed4 color = _BaseColor;

                for (int j = 0; j < _BandCount; j++)
                {
                    if (height <= _Thresholds[j])
                    {
                        color = _ColorBands[j];
                        break;
                    }
                }

                return color;
            }

            ENDCG
        }
    }
}
