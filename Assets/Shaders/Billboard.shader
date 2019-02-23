Shader "Unlit/Billboard"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Cutoff	("Cutoff", Range(0,1)) = 0.1
		_ScaleX ("Scale X", Float) = 1.0
		_ScaleY ("Scale Y", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Cutout" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

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
			fixed _Cutoff;
			uniform float _ScaleX;
			uniform float _ScaleY;

            v2f vert (appdata v)
            {
				v2f o;

				o.vertex = mul(UNITY_MATRIX_P, 
				mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
				+ float4(v.vertex.x, v.vertex.y, 0.0, 0.0)
				* float4(_ScaleX, _ScaleY, 1.0, 1.0));

				o.uv = v.uv;

				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
				clip(col.a - _Cutoff);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
								
                return col;
            }
            ENDCG
        }
    }
}
