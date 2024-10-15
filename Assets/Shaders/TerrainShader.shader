Shader "Custom/TerrainShader"
{
    Properties
    {
        _MainTex ("Terrain Texture", 2D) = "white" {}  // Expose the terrain texture
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

            // Properties
            sampler2D _MainTex;  // The texture (your terrain sprite)

            // Vertex input structure
            struct appdata_t
            {
                float4 vertex : POSITION;  // Vertex position
                float2 uv : TEXCOORD0;     // UV coordinates for texture mapping
            };

            // Vertex output structure
            struct v2f
            {
                float2 uv : TEXCOORD0;     // UV coordinates passed to the fragment shader
                float4 vertex : SV_POSITION;  // Screen-space position
            };

            // Vertex shader
            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);  // Transform vertex position to screen space
                o.uv = v.uv;  // Pass UV coordinates directly to the fragment shader
                return o;
            }

            // Fragment (pixel) shader
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the texture using UV coordinates
                fixed4 col = tex2D(_MainTex, i.uv);

                // Return the sampled color from the texture
                return col;
            }
            ENDCG
        }
    }
}
