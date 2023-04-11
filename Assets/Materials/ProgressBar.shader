Shader "Custom/ProgressBar"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _EmptyColor ("EmptyColor", Color) = (0.4,0,0,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _ProgressPct ("ProgressPercentage", Range(0,1)) = 1.0
        _BorderSize ("BorderSize", Range(0,0.1)) = 0.02
        _Height ("Height", Range(0,1)) = 1.0
        _Width ("Width", Range(0,1)) = 1.0
        _InnerWidth("InnerWidth", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

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
        fixed4 _EmptyColor;

        float _ProgressPct;
        float _BorderSize;

        float _Height;
        float _Width;
        float _InnerWidth;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

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

            //set boundaries
            float heightLb = 0.5 - _Height/2;
            float heightUb = 0.5 + _Height/2;
            float widthLb = 0.5 - _Width/2;
            float widthUb = 0.5 + _Width/2;
            float innerWidthLb = 0.5 - (_Width*_InnerWidth) / 2;
            float innerWidthUb = 0.5 + (_Width*_InnerWidth) / 2;
            //main difference from regular health bar: function to determine border upper bound that produces triangular edges
            //recalcuate height boundary if outside inner width

            //left side
            if(pos.x >= widthLb-_BorderSize && pos.x <= innerWidthLb){
                heightLb = heightLb + (heightUb - heightLb)*(innerWidthLb-pos.x)/(innerWidthLb-widthLb); 
            }

            //right side
            if(pos.x >= innerWidthUb && pos.x <= widthUb+_BorderSize){
                heightLb = heightLb + (heightUb - heightLb)*(pos.x-innerWidthUb)/(widthUb-innerWidthUb); 
            }


            //border
            if(((((abs(pos.x - widthLb) <= _BorderSize) || (abs(pos.x - widthUb) <= _BorderSize)) && (pos.y >= heightLb && pos.y <= heightUb))) 
            || ((((abs(pos.y - heightLb) <= _BorderSize) || (abs(pos.y - heightUb) <= _BorderSize)) && (pos.x >= widthLb && pos.x <= widthUb)))){
                o.Albedo = float3(0,0,0);
            }
            //inside health bar
            else if (pos.x >= widthLb && pos.x <= widthUb && pos.y >= heightLb && pos.y <= heightUb){
                //filled region
                if (pos.x - widthLb <= _ProgressPct * _Width){
                    o.Albedo = _Color;
                }
                //empty region
                else {
                    o.Albedo = _EmptyColor;
                }
            }
            //discard outside health bar
            else {
                discard;
            }
            
        }
        ENDCG
    }
    FallBack "Diffuse"
}
