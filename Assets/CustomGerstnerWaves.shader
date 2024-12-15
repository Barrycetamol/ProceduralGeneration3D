Shader "Custom/GerstnerWaves"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.0, 0.4, 0.7, 1.0) // Soft blue for water
        _WaveCount ("Wave Count", Float) = 3
        _WaveDirections ("Wave Directions", Vector) = (1.0, 0.5, 0.2, 0.0)
        _WaveFrequencies ("Wave Frequencies", Vector) = (0.8, 0.5, 0.0, 0.0)
        _WaveAmplitudes ("Wave Amplitudes", Vector) = (0.2, 0.15, 0.1, 0.0)
        _WaveSpeeds ("Wave Speeds", Vector) = (0.5, 0.6, 0.4, 0.0)
        _WaveLength ("Wave Length", Float) = 20.0
        _WindDirection ("Wind Direction", Vector) = (1.0, 0.5, 0.0, 0.0)
        _WindStrength ("Wind Strength", Float) = 0.2
        _StartingHeight("Starting Height", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" }
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
            float _StartingHeight;
            float _StartTime;

            float4 _BaseColor;

            v2f vert(appdata v)
            {
                v2f o;
                float3 position = v.vertex.xyz;
                float y = 0.0;

                float2 windDir = normalize(_WindDirection.xy) * _WindStrength;

                // Gerstner wave calculations
                for (int i = 0; i < _WaveCount; i++)
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
                o.worldPos = position;
                o.pos = UnityObjectToClipPos(float4(position, 1.0));
                o.normal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal)); // Adjust normals

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Dynamically calculate the height range based on wave amplitudes
                float maxAmplitude = max(max(_WaveAmplitudes.x, _WaveAmplitudes.y), _WaveAmplitudes.z);
                float minHeight = _StartingHeight - maxAmplitude;
                float maxHeight = _StartingHeight + maxAmplitude;

                // Lerp i.worldPos.y between minHeight and maxHeight
                float height = lerp(minHeight, maxHeight, saturate((i.worldPos.y - minHeight) / (maxHeight - minHeight)));

                // Normalize height to a 0-1 range
                float normalizedHeight = saturate((height - minHeight) / (maxHeight - minHeight));

                // Define multiple color bands
                fixed4 band1 = fixed4(0.0, 0.1, 0.5, 0.1); // Deep blue
                fixed4 band2 = fixed4(0.0, 0.3, 0.8, 0.1); // Mid blue
                fixed4 band3 = fixed4(0.5, 0.5, 1.0, 0.1); // Light blue
                fixed4 band4 = fixed4(0.8, 0.8, 1.0, 0.1); // Surface reflection

                // Define thresholds for blending
                float threshold1 = 0.8;
                float threshold2 = 1.0;

                // Determine the correct color band based on normalized height
                fixed4 color;
                if (normalizedHeight < threshold1)
                {
                    // Blend between band1 and band2
                    float t = saturate(normalizedHeight / threshold1);
                    color = lerp(band1, band2, t);
                }
                else if (normalizedHeight < threshold2)
                {
                    // Blend between band2 and band3
                    float t = saturate((normalizedHeight - threshold1) / (threshold2 - threshold1));
                    color = lerp(band2, band3, t);
                }
                else
                {
                    // Blend between band3 and band4
                    float t = saturate((normalizedHeight - threshold2) / (1.0 - threshold2));
                    color = lerp(band3, band4, t);
                }

                return color;
            }

            ENDCG
        }
    }
}
