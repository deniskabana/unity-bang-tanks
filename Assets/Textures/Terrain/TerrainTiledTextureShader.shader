Shader "Custom/TerrainTiledTextureShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}  // Your terrain texture (which needs tiling)
        _HeightMap ("Height Map", 2D) = "white" {}  // Heightmap texture (which doesn't need tiling)
        _HeightIntensity ("Height Intensity", Float) = 1.0
        _TilingX ("Tiling X", Float) = 1.0  // Tiling factor for X axis (for MainTex only)
        _TilingY ("Tiling Y", Float) = 1.0  // Tiling factor for Y axis (for MainTex only)
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;  // Original UV for heightmap (unmodified)
                float2 uvTiled : TEXCOORD1;  // Tiled UV for MainTex
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _HeightMap;
            float _HeightIntensity;
            float _TilingX;  // Tiling factor for X axis (for MainTex)
            float _TilingY;  // Tiling factor for Y axis (for MainTex)

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Original UV coordinates for heightmap (no tiling)
                o.uv = v.uv;

                // Tiled UVs for MainTex (only apply tiling to MainTex)
                o.uvTiled = v.uv * float2(_TilingX, _TilingY);

                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the MainTex using tiled UVs
                fixed4 col = tex2D(_MainTex, i.uvTiled);

                // Sample the HeightMap using the original UVs (no tiling)
                fixed4 height = tex2D(_HeightMap, i.uv);
                
                // Use the red channel of the heightmap to modify alpha
                col.a *= height.r * _HeightIntensity;
                
                // Multiply by vertex color
                col *= i.color;
                
                return col;
            }
            ENDCG
        }
    }
}