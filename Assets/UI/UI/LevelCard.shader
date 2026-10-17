Shader "RobotRhythm/LevelCard"
{
    Properties
    {
        [PerRendererData] _MainTex ("Artwork", 2D) = "white" {}
        _Size ("Card size", Vector) = (540,304,0,0)
        _Radius ("Corner radius", Float) = 22
        _Border ("Border width", Float) = 4
        _BorderColor ("Border color", Color) = (0.025,0.035,0.04,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="False" }
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
            sampler2D _MainTex;
            float4 _Size, _BorderColor;
            float _Radius, _Border;
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
                // Distance to a rounded rectangle, measured in canvas units.
                float2 q=abs((i.uv-.5)*_Size.xy)-(_Size.xy*.5-_Radius);
                float distance=length(max(q,0))+min(max(q.x,q.y),0)-_Radius;
                float edge=max(fwidth(distance),.001);
                float outline=1-smoothstep(-_Border-edge,-_Border+edge,distance);
                float4 color=lerp(_BorderColor,tex2D(_MainTex,i.uv),outline)*i.color;
                color.a*=1-smoothstep(-edge,0,distance);
                return color;
            }
            ENDCG
        }
    }
}
