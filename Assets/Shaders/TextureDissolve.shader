Shader "Custom/TextureDissolve"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NoiseTex ("Noise texture", 2D) = "white" {}
        _Disolve ("Disolve", Range(0,1) ) = 0
        _EdgeWidth ("EdgeWidth", Range(0,1)) = 0.3
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline"}
        LOD 200

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
            #pragma vertex vert  
            #pragma fragment frag
            #include "UnityCG.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
   
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
 
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float _Disolve;
            float _EdgeWidth;

            fixed4 frag(v2f i) : SV_Target
            {
                float4 noiseTex = tex2D(_NoiseTex, i.uv);
                float max = _Disolve*(1+_EdgeWidth);
                float step = smoothstep(max - _EdgeWidth, max, noiseTex);
                if(step < 0.1) discard;
                float4 col = tex2D(_MainTex, i.uv);
                // float4 col = float4(step, step, step, step);
                col.a = step;
                return col;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}
