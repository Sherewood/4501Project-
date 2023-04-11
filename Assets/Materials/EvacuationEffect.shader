Shader "Custom/EvacuationEffect"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _EffectTime ("EffectTime", float) = 0.0
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
        fixed4 _Color;

        float _EffectTime;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        //note: the selection ring takes some inspiration from the UI > Unit_Selection asset in the RTS_Scifi_game_assets folder,
        //however it doesn't use any code from outside sources, and the aforementioned asset was done using a particle system + texture afaik (haven't looked too far into it)
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;

            float2 pos = IN.uv_MainTex;

            //center the position
            pos -= float2(0.5,0.5);
            float r = sqrt(pos.x*pos.x + pos.y*pos.y);

            float diff = abs(r-0.45);

            float timeEffect = (sin(3.14*_EffectTime - 3.14/2) + 1)/2;

            //ring
            if(diff < 0.05){
                o.Albedo = _Color;
                o.Alpha = smoothstep(0.05,0.0,diff)*timeEffect;
            }
            //X (its a plus but will show up as a X ingame)
            else if ((abs(pos.x) < 0.02 && abs(pos.y) < 0.3) || (abs(pos.y) < 0.02 && abs(pos.x) < 0.3)){
                o.Albedo = _Color;
                o.Alpha = timeEffect;
            }
            //discard otherwise
            else {
                o.Alpha = 0.0;
            }

            //do the discard here to account for alpha being low
            if(o.Alpha < 0.2){
                discard;
            }
        }
        ENDCG
    }
    FallBack "Diffuse"
}
