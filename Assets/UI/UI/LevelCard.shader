Shader "RobotRhythm/LevelCard"
{
    Properties
    {
        [PerRendererData] _MainTex ("Level artwork", 2D) = "white" {}
        _PaperTex ("Paper frame", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            struct v2f { float4 vertex:SV_POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            sampler2D _MainTex, _PaperTex;
            float paperInterior(float2 uv)
            {
                float4 paper = tex2D(_PaperTex,uv);
                return smoothstep(.25,.7,dot(paper.rgb,float3(.299,.587,.114))) * paper.a;
            }
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex=UnityObjectToClipPos(v.vertex);
                o.color=v.color;
                o.uv=v.uv;
                return o;
            }
            float4 frag(v2f i):SV_Target
            {
                // Keep the paper silhouette, with a consistent thin ink edge instead of its enlarged shadow.
                float2 paperUV = lerp(float2(.055,.075), float2(.93,.925), i.uv);
                float interior = paperInterior(paperUV);
                float2 edge = fwidth(paperUV) * 3;
                float outline = max(max(paperInterior(paperUV+float2(edge.x,0)),
                    paperInterior(paperUV-float2(edge.x,0))),
                    max(paperInterior(paperUV+float2(0,edge.y)),
                    paperInterior(paperUV-float2(0,edge.y))));
                float4 art = tex2D(_MainTex,i.uv);
                float4 color = float4(lerp(float3(.035,.045,.05),art.rgb,interior),max(interior,outline));
                return color * i.color;
            }
            ENDCG
        }
    }
}
