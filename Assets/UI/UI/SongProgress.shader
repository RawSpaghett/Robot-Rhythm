Shader "RobotRhythm/SongProgress"
{
    Properties
    {
        [PerRendererData] _MainTex ("Paper artwork", 2D) = "white" {}
        _FillMask ("Inner opening", 2D) = "black" {}
        _Progress ("Progress", Range(0,1)) = 0
        _FillBounds ("Opening bounds", Vector) = (0,1,0,0)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            sampler2D _MainTex, _FillMask;
            float _Progress;
            float4 _FillBounds;
            float4 _ClipRect;
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; float4 local:TEXCOORD1; };
            v2f vert(appdata v)
            {
                v2f o;
                o.local=v.vertex; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color;
                return o;
            }
            float4 frag(v2f i):SV_Target
            {
                float4 source=tex2D(_MainTex,i.uv);
                float opening=tex2D(_FillMask,i.uv).a;
                float position=saturate((i.uv.x-_FillBounds.x)/(_FillBounds.y-_FillBounds.x));
                float filled=step(position,_Progress)*step(.00001,_Progress);
                float3 gold=lerp(float3(1,.62,.06),float3(1,.86,.24),i.uv.y);
                #ifndef UNITY_COLORSPACE_GAMMA
                gold=GammaToLinearSpace(gold);
                #endif
                source.rgb=lerp(source.rgb,gold,opening*filled);
                source*=i.color;
                #ifdef UNITY_UI_CLIP_RECT
                source.a*=UnityGet2DClipping(i.local.xy,_ClipRect);
                #endif
                return source;
            }
            ENDCG
        }
    }
}
