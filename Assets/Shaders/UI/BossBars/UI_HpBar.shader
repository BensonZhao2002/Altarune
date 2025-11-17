Shader "Anthony/UI/HP Bar"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)

        [Header(Fill)][Space]
        _Fill("Fill", Range(0, 1)) = 1
        _BaseBrightness("Base Brightness", Range(0, 1)) = 0.5

        [Header(Background)][Space]
        _BackgroundColor ("Background Color", Color) = (0.2, 0.2, 0.2, 0.7)
        [Toggle(CLIP_BACKGROUND)] _ClipBackground ("Clip Background?", Float) = 0

        [Header(PulseAcrossX)][Space]
        [Toggle(GLOW_PULSE_OVER_TIME)] _GlowOverTimeX("Glow over time across X?", Int) = 1
        _GlowRamp("Glow Ramp (RGB)", 2D) = "white" {}
        _GlowMultiplier("Glow Multiplier", Float) = 2
        _GlowWidth("Glow Width", Range(0, 0.5)) = 0.05
        _GlowParams("Glow XSpeed (X) XOffset (Y) Time Delay(Z) ", Vector) = (0.2, 0, 0, 0)

        // Stencils
        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255

        _ColorMask("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
    }
    CGINCLUDE
        #include "UnityCG.cginc"
        #include "UnityUI.cginc"
        #include "UIGlow.cginc"

        #pragma multi_compile_instancing // enable instancing
        #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
        #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

        #pragma shader_feature GLOW_PULSE_OVER_TIME
        #pragma shader_feature GLOW_WHEN_LOW
        #pragma shader_feature CLIP_BACKGROUND

        struct MeshData
        {
            float4 vertex : POSITION;
            float4 color : COLOR;
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Interpolators
        {
            float4 vertex : SV_POSITION;
            float4 color : COLOR;
            float2 uv : TEXCOORD0;
            float4 worldPosition : TEXCOORD1;
            float glowPosition : TEXCOORD2;
            float4 screenPos : TEXCOORD3;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        #define TAU 6.2831853078

        // Default UI Params
        sampler2D _MainTex;
        float4 _MainTex_ST;
        fixed4 _Color;
        fixed4 _TextureSampleAdd;
        float4 _ClipRect;

        // Glow Flash
        float4 _LowGlowColor;
        float _LowThreshold;
        float _GlowFrequency;
        float _GlowAmplitude;

        // Fill
        float _Fill;
        float _BaseBrightness;

        // Background
        float4 _BackgroundColor;

        Interpolators vert(MeshData v)
        {
            Interpolators o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

            o.worldPosition = v.vertex; // save world position
            o.vertex = UnityObjectToClipPos(v.vertex); // local to clip space position

            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            o.color = v.color * _Color;

            #ifdef GLOW_PULSE_OVER_TIME
            // Glow position
            o.glowPosition = CalculateGlowPositionX(_MainTex_ST);
            #endif

            o.screenPos = ComputeScreenPos(o.vertex);

            return o;
        }

        fixed4 frag(Interpolators i) : SV_Target
        {
            half4 color = (tex2D(_MainTex, i.uv) + _TextureSampleAdd) * i.color;
            color.rgb = lerp(color.rgb, color.rgb * i.uv.y, 0.5); // vertical gradient

            // Glow across X
            float2 screenUV = i.screenPos.xy / i.screenPos.w;

            #ifdef GLOW_PULSE_OVER_TIME
                color.rgb *= IlluminateSection(screenUV.x, i.glowPosition, _GlowWidth) + _BaseBrightness;
            #endif

            // Glow when fill is low
            #ifdef GLOW_WHEN_LOW
            if (_Fill <= _LowThreshold)
            {
                float flash = (cos(_Time.y * _GlowFrequency) * 0.5 + 0.5) * _GlowAmplitude;
                color.rgb *= flash * 0.5;
                //color.rgb = lerp(color.rgb, _LowGlowColor, flash);
            }
            #endif

            // Unity's default UI Clipping
            #ifdef UNITY_UI_CLIP_RECT
            color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
            #endif

            #ifdef UNITY_UI_ALPHACLIP
            clip(color.a - 0.001);
            #endif

            // Fill clip
            #if CLIP_BACKGROUND
            if (i.uv.x > _Fill)
            {
                discard;
            }
            #endif
            // Otherwise, does background color
            half4 backgroundColor = (tex2D(_MainTex, i.uv) + _TextureSampleAdd) * _BackgroundColor;
            backgroundColor.rgb = lerp(backgroundColor.rgb, backgroundColor.rgb * i.uv.y, 0.6);
            float fillMask = step(i.uv.x, _Fill);
            float backgroundMask = 1 - fillMask;

            color.rgb = (color.rgb * fillMask) + backgroundColor * backgroundMask;
            color.a = saturate(fillMask * color.a + backgroundMask * backgroundColor.a);

            return color;
        }
    
    ENDCG
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest[unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask[_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            ENDCG
            }
        }
}
