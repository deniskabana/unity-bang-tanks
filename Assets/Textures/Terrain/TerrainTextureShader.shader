Shader "Custom/TerrainTextureShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _HeightMap ("Height Map", 2D) = "white" {}
        _HeightIntensity ("Height Intensity", Float) = 1.0
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _HeightMap;
            float _HeightIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 height = tex2D(_HeightMap, i.uv);
                
                // Use the red channel of the height map to modify the alpha
                col.a *= height.r * _HeightIntensity;
                
                // Multiply by vertex color
                col *= i.color;
                
                return col;
            }
            ENDCG
        }
    }
}