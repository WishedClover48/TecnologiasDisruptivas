// IndependentLife/UI/PhoneBackground
// Procedural UGUI dark-glass panel for the phone screen.
// Renders: rounded card + top-to-bottom gradient + subtle glare + thin border ring.
// Requires _RectSize in pixel units (use UIShaderRect.cs).

Shader "IndependentLife/UI/PhoneBackground"
{
    Properties
    {
        // Dummy texture — required by UGUI Image / MaskableGraphic
        [HideInInspector] _MainTex ("Sprite Texture", 2D) = "white" {}

        _TopColor     ("Top Color",        Color)  = (0.09, 0.11, 0.17, 1.0)
        _BottomColor  ("Bottom Color",     Color)  = (0.05, 0.07, 0.11, 1.0)
        _BorderColor  ("Border Color",     Color)  = (0.28, 0.40, 0.60, 0.45)
        _GlareColor   ("Glare Color",      Color)  = (1.00, 1.00, 1.00, 0.07)
        _CornerRadius ("Corner Radius px", Float)  = 28.0
        _BorderWidth  ("Border Width px",  Float)  = 1.5
        _RectSize     ("Rect Size",        Vector) = (300, 600, 0, 0)

        // UGUI masking
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

            float4 _TopColor;
            float4 _BottomColor;
            float4 _BorderColor;
            float4 _GlareColor;
            float  _CornerRadius;
            float  _BorderWidth;
            float4 _RectSize;
            float4 _ClipRect;

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
                float2 px   = (i.uv - 0.5) * size;
                float2 half = size * 0.5;
                float  r    = clamp(_CornerRadius, 0.0, min(half.x, half.y));

                float sdf  = sdfRoundRect(px, half, r);
                float aa   = max(fwidth(sdf), 0.001);
                float mask = saturate(1.0 - sdf / aa);

                // Thin border: pixels just inside the shape edge
                float outerEdge = saturate(1.0 - (sdf + 0.5)  / aa);
                float innerEdge = saturate(1.0 - (sdf + _BorderWidth + 0.5) / aa);
                float border    = saturate(outerEdge - innerEdge);

                // Top-to-bottom gradient (uv.y = 0 is top in Unity Canvas)
                float4 bgColor = lerp(_TopColor, _BottomColor, i.uv.y);

                // Soft elliptical glare in the top-centre area
                float2 glareUV  = (i.uv - float2(0.5, 0.15)) * float2(1.6, 3.0);
                float  glare    = saturate(1.0 - dot(glareUV, glareUV));
                glare           = glare * glare;

                float4 col  = bgColor * mask;
                col.rgb    += _GlareColor.rgb * glare * _GlareColor.a * mask;
                // Additive border (no lerp to keep alpha intact)
                col.rgb    += _BorderColor.rgb * border * _BorderColor.a;
                col.a       = mask * bgColor.a;

                col   *= i.color;
                col.a *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);
                return col;
            }
            ENDCG
        }
    }
}
