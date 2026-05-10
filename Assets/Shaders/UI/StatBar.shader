
Shader "IndependentLife/UI/StatBar"
{
    Properties
    {
        [HideInInspector] _MainTex ("Sprite Texture", 2D) = "white" {}

        _FillAmount   ("Fill Amount",      Range(0,1)) = 1.0
        _FillColor    ("Fill Color",       Color)      = (0.22, 0.82, 0.49, 1)
        _BackColor    ("Background Color", Color)      = (0.10, 0.13, 0.18, 1)
        _CornerRadius ("Corner Radius px", Float)      = 12.0
        _GlowWidth    ("Edge Glow px",     Float)      = 6.0
        _RectSize     ("Rect Size",        Vector)     = (200, 28, 0, 0)

        _StencilComp      ("Stencil Comparison", Float) = 8
        _Stencil          ("Stencil ID",         Float) = 0
        _StencilOp        ("Stencil Operation",  Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask",  Float) = 255
        _ColorMask        ("Color Mask",         Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType"      = "Transparent"
            "PreviewType"     = "Plane"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float   _FillAmount;
            float4  _FillColor;
            float4  _BackColor;
            float   _CornerRadius;
            float   _GlowWidth;
            float4  _RectSize;
            float4  _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex   = UnityObjectToClipPos(v.vertex);
                o.uv       = v.texcoord;
                o.worldPos = v.vertex;
                o.color    = v.color;
                return o;
            }

            float sdfRoundRect(float2 p, float2 b, float r)
            {
                float2 q = abs(p) - b + r;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 size = _RectSize.xy;

                float2 px = (i.uv - 0.5) * size;

                float2 half   = size * 0.5;
                float  radius = clamp(_CornerRadius, 0.0, min(half.x, half.y));

                float bgDist = sdfRoundRect(px, half, radius);
                float aa     = max(fwidth(bgDist), 0.001);
                float bgMask = saturate(1.0 - bgDist / aa);

                float fillEdgeX = lerp(-half.x, half.x, _FillAmount);
                float fillClip  = saturate((fillEdgeX - px.x) / aa + 0.5);
                float fillMask  = bgMask * fillClip;

                // Only visible when bar is neither empty nor full
                float glowDist = abs(px.x - fillEdgeX);
                float glow     = saturate(1.0 - glowDist / max(_GlowWidth, 0.1));
                glow = glow * glow * fillMask * 0.6;

                //Top shine on filled region (adds depth)
                float topShine = saturate(1.0 - (px.y + half.y * 0.5) / (half.y * 0.8));
                topShine *= fillMask * 0.20;

                //Inner shadow on empty region
                float emptyDarken = (1.0 - fillClip) * bgMask * 0.12;

                float4 col = _BackColor * bgMask;
                col = lerp(col, _FillColor, fillMask);
                col.rgb += _FillColor.rgb * glow;
                col.rgb += topShine;
                col.rgb -= emptyDarken;
                col.a    = bgMask;

                col   *= i.color;
                col.a *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);
                return col;
            }
            ENDCG
        }
    }
}
