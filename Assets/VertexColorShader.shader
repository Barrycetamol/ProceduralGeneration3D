Shader "Custom/VertexColorShader"   // Chatgpt generated shder
{
    Properties
    {
        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
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
                float4 color : COLOR; // Vertex color input
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR; // Pass color to fragment
            };

            float _Glossiness;
            float _Metallic;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color; // Pass vertex color through
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color; // Output vertex color directly
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}