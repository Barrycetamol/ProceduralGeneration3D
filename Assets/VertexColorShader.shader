Shader "Custom/VertexColorShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {} // This allows you to add a texture later if needed
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float4 color : COLOR; // Vertex color input
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float4 color : COLOR; // Vertex color passed to fragment
            };

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); // Convert vertex position to clip space
                o.color = v.color; // Pass vertex color to fragment shader
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                return i.color; // Use the vertex color as the fragment color
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}