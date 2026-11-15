Shader "UI/StudioInkReveal"
{
    Properties
    {
        [PerRendererData] _MainTex ("Artwork", 2D) = "white" {}
        _RevealTex ("Ink timing", 2D) = "black" {}
        _Elapsed ("Seconds", Float) = -1
        _StencilComp ("Stencil comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil operation", Float) = 0
        _StencilWriteMask ("Stencil write mask", Float) = 255
        _StencilReadMask ("Stencil read mask", Float) = 255
        _ColorMask ("Color mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR; };
            struct v2f { float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR; };
            sampler2D _MainTex, _RevealTex;
            float _Elapsed;
            v2f vert(appdata v) { v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o; }
            fixed4 frag(v2f i):SV_Target
            {
                float2 bytes=tex2D(_RevealTex,i.uv).rg;
                float arrival=dot(bytes,float2(65280,255))/65535*16;
                fixed4 c=tex2D(_MainTex,i.uv)*i.color;
                c.a*=smoothstep(arrival,arrival+.018,_Elapsed);
                return c;
            }
            ENDCG
        }
    }
}
