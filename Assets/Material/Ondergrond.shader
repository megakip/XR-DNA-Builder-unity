Shader "Unlit/RadialGradient"
{
    Properties
    {
        _Color ("Base Color", Color) = (0.3, 0.3, 0.3, 1.0)
        _CenterIntensity ("Center Intensity", Range(0, 1)) = 0.1
        _EdgeIntensity ("Edge Intensity", Range(0, 1)) = 0.8
        _RadiusMultiplier ("Radius Multiplier", Range(0.1, 3.0)) = 1.0
        _CenterAlpha ("Center Alpha", Range(0, 1)) = 1.0
        _EdgeAlpha ("Edge Alpha", Range(0, 1)) = 0.0
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
        }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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

            fixed4 _Color;
            float _CenterIntensity;
            float _EdgeIntensity;
            float _RadiusMultiplier;
            float _CenterAlpha;
            float _EdgeAlpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Bereken afstand van het midden (0.5, 0.5)
                float2 center = float2(0.5, 0.5);
                float2 uvFromCenter = i.uv - center;
                float distanceFromCenter = length(uvFromCenter) * _RadiusMultiplier;
                
                // Maak een gladde overgang van donker (midden) naar licht (rand)
                float gradient = smoothstep(0.0, 1.0, distanceFromCenter);
                
                // Interpoleer tussen center intensity en edge intensity
                float intensity = lerp(_CenterIntensity, _EdgeIntensity, gradient);
                
                // Interpoleer tussen center alpha en edge alpha voor transparantie
                float alpha = lerp(_CenterAlpha, _EdgeAlpha, gradient);
                
                // Pas de intensiteit toe op de basis kleur
                fixed4 col = _Color * intensity;
                col.a = alpha;
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
