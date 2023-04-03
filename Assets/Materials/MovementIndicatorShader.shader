Shader "Custom/MovementIndicatorShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Alpha("Alpha", Range(0,1)) = 0
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _EffectTime("EffectTime", Range(0,1.1)) = 0

    }
    SubShader
    {
        Tags { 
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 200

        CGPROGRAM

        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        float _Alpha;
        
        float _EffectTime;

        fixed4 _Color;


        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = _Alpha;
            
            float2 pos = IN.uv_MainTex;

            //center the position
            pos -= float2(0.5,0.5);

            float n = _EffectTime/2;
            float length = 0.13 - _EffectTime/10;
            float diffX = abs((abs(pos.x)-n));
            float diffY = abs((abs(pos.y)-n));
            float r = sqrt(pos.x*pos.x + pos.y*pos.y);

            //center
            if(r < 0.085 && r < length){
                o.Albedo = float3(0.0,0.7,0.0);
                o.Alpha = 1.0;
            }
            //brackets traveling outward...
            else if(abs(pos.x) < 0.08 && diffY < length){
                o.Albedo = float3(0.0,0.7,0.0);
                o.Alpha = min(smoothstep(0.02,0.0,abs(pos.x)), smoothstep(length,0,diffY));
            }
            else if (abs(pos.y) < 0.08 && diffX < length){
                o.Albedo = float3(0.0,0.7,0.0);
                o.Alpha = min(smoothstep(0.02,0.0,abs(pos.y)), smoothstep(length,0,diffX));
            }
            else {
                discard;
            }

            //discard pixels that are too transparent
            if(o.Alpha < 0.15){
                discard;
            }

        }
        ENDCG
    }
    FallBack "Diffuse"
}
