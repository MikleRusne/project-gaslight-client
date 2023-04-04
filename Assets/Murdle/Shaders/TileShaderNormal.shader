Shader "Unlit/TileShaderNormal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PlayerTurn ("PlayerTurn", Float) = 0.0
        _PlayerPath ("PlayerPath", Float) = 0.0
        _DefaultColor ("Default Color", Color) = (1, 0.95, 0.84, 1)
        _PlayerTurnColor ("Player Turn Color", Color) = (0.0,1.0,0.0, 1)
        _PlayerPathColor ("Player Path Color", Color) = (0.0,0.0,1.0,1)
        
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            vector _DefaultColor;
            vector _PlayerTurnColor;
            vector _PlayerPathColor;
            float _PlayerTurn;
            float _PlayerPath;
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col1 = lerp(_DefaultColor, _PlayerTurnColor, _PlayerTurn);
                fixed4 col2 = lerp(col1, _PlayerPathColor, _PlayerPath);
                return col2;
            }
            ENDCG
        }
    }
}
