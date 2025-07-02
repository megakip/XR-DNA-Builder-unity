Shader "Custom/GradientSkybox"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (1, 0.5, 0, 1)    // Oranje
        _BottomColor ("Bottom Color", Color) = (0.2, 0.2, 0.2, 1)   // Donkergrijs
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldDir : TEXCOORD0;
            };

            fixed4 _TopColor;
            fixed4 _BottomColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Berekent de wereldrichting (hier geldt: de skybox is een cube, dus de vertexposities geven al de richting)
                o.worldDir = normalize(mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Gebruik de Y-component om tussen de twee kleuren te interpoleren
                float t = saturate(i.worldDir.y * 0.5 + 0.5);
                return lerp(_BottomColor, _TopColor, t);
            }
            ENDCG
        }
    }
    FallBack "RenderFX/Skybox"
}
