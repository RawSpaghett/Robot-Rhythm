Shader "RobotRhythm/FiveStarSheen"
{
    Properties
    {
        [PerRendererData] _MainTex ("Star artwork", 2D) = "white" {}
        _SheenTime ("Sheen time", Float) = 0
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
            sampler2D _MainTex;
            float _SheenTime;
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
                #ifndef UNITY_COLORSPACE_GAMMA
                source.rgb=LinearToGammaSpace(source.rgb);
                #endif
                // Keep the drawn outline and alpha. Only the gold interior receives the sheen.
                float gold=smoothstep(.25,.65,min(source.r,source.g))*(1-smoothstep(.65,.95,source.b));
                float phase=i.uv.x*.75+i.uv.y*.55-_SheenTime*.065;
                phase+=.12*sin(i.uv.y*5+i.uv.x*3+_SheenTime*.2);
                float3 pearl=.68+.32*cos(6.283185*(phase+float3(0,.333,.667)));
                float shine=pow(.5+.5*sin((phase+.12)*12.56637),10);
                float3 finish=lerp(source.rgb,pearl,.48);
                finish=lerp(finish,float3(1,.98,.94),shine*.18);
                source.rgb=lerp(source.rgb,finish,gold);
                #ifndef UNITY_COLORSPACE_GAMMA
                source.rgb=GammaToLinearSpace(source.rgb);
                #endif
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
