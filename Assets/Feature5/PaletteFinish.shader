Shader "RobotRhythm/Feature5/PaletteFinish"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _CleanTex ("Clean source", 2D) = "white" {}
        _Regions ("Body, light metal, dark metal, hubs", 2D) = "black" {}
        _Accents ("Flag ears and square", 2D) = "black" {}
        _BodyColor ("Body", Color) = (0.97647,0.92941,0.81961,1)
        _AccentColor ("Accent", Color) = (0.83137,0.21961,0.20392,1)
        _FaceMask ("Face and resting expressions", 2D) = "black" {}
        _FaceColor ("Face panel", Color) = (.067,.2,.243,1)
        _EyeColor ("Resting expression", Color) = (0,.537,1,1)
        _MetalValue ("Arm and hand brightness", Range(0,1)) = 1
        _WheelColor ("Wheel circles", Color) = (.5,.5,.5,1)
        _WheelOverride ("Use wheel color", Range(0,1)) = 0
        _Iridescence ("Rainbow sheen", Range(0,1)) = 0
        _SheenTime ("Preview time (-1 uses game time)", Float) = -1
        _SheenScale ("Sheen coordinate scale", Float) = 1
        _Strength ("Strength", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="False" }
        Cull Off Lighting Off ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local __ UNITY_UI_CLIP_RECT
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            float4 _ClipRect;
            sampler2D _CleanTex, _MainTex, _Regions, _Accents, _FaceMask;
            float4 _CleanTex_TexelSize;
            float4 _BodyColor, _AccentColor, _FaceColor, _EyeColor, _WheelColor;
            float _MetalValue, _WheelOverride;
            float _Strength, _Iridescence, _SheenTime, _SheenScale;
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; float2 local:TEXCOORD1; };
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color; o.local=v.vertex.xy;
                return o;
            }
            float4 frag(v2f i):SV_Target
            {
                float4 source=tex2D(_CleanTex,i.uv);
                // Close isolated transparent pinholes without changing the source artwork.
                float2 step = _CleanTex_TexelSize.xy;
                float2 left = i.uv-float2(step.x,0), right = i.uv+float2(step.x,0);
                float2 down = i.uv-float2(0,step.y), up = i.uv+float2(0,step.y);
                float repair = 0;
                if (source.a < .95)
                {
                    float4 a=tex2D(_CleanTex,left), b=tex2D(_CleanTex,right);
                    float4 c=tex2D(_CleanTex,down), d=tex2D(_CleanTex,up);
                    if (a.a+b.a+c.a+d.a > 3.7)
                    {
                        source=(a+b+c+d)*.25;
                        repair=1;
                    }
                }
                source.a=smoothstep(.05,.95,source.a);
                float3 body=_BodyColor.rgb;
                float3 accentColor=_AccentColor.rgb;
                float3 face=_FaceColor.rgb;
                float3 eye=_EyeColor.rgb;
                float3 wheel=_WheelColor.rgb;
                #ifndef UNITY_COLORSPACE_GAMMA
                source.rgb=LinearToGammaSpace(source.rgb);
                body=LinearToGammaSpace(body);
                accentColor=LinearToGammaSpace(accentColor);
                face=LinearToGammaSpace(face);
                eye=LinearToGammaSpace(eye);
                wheel=LinearToGammaSpace(wheel);
                #endif
                float4 regions=tex2D(_Regions,i.uv);
                float accent=tex2D(_Accents,i.uv).r;
                if (repair > 0)
                {
                    regions=(tex2D(_Regions,left)+tex2D(_Regions,right)+tex2D(_Regions,down)+tex2D(_Regions,up))*.25;
                    accent=(tex2D(_Accents,left).r+tex2D(_Accents,right).r+tex2D(_Accents,down).r+tex2D(_Accents,up).r)*.25;
                }
                float brightness=max(dot(body,float3(.2126,.7152,.0722)),.001);
                float3 metalTint=lerp(float3(1,1,1),body/brightness,.35);
                float time=_SheenTime<0?_Time.y:_SheenTime;
                // Use robot-sized coordinates so the menu and gameplay have the same soft shimmer.
                float2 sheen = i.local * _SheenScale;
                float phase=sheen.x*1.25+sheen.y*.9-time*.18;
                phase+=.16*sin(sheen.y*7-time*.8)+.1*sin(sheen.x*9+time*.65);
                float band=.5+.5*sin(phase*6.283185);
                float3 pearl=.53+.47*cos(6.283185*(phase+float3(0,.333,.667)));
                float shine=pow(.5+.5*sin((phase+.12)*6.283185),6);
                float rainbow=smoothstep(.15,.8,band)*.8;
                float3 finish=lerp(body,pearl,_Iridescence*rainbow);
                finish=lerp(finish,float3(1,.91,.56),shine*_Iridescence*.28);
                float3 lightMetal=lerp((153.0/255.0)*metalTint,finish,_Iridescence*.95)*_MetalValue;
                float3 darkMetal=lerp((101.0/255.0)*metalTint,finish*.88,_Iridescence*.95)*_MetalValue;
                float3 hubs=lerp(lerp(finish*.55,wheel,_WheelOverride),finish,_Iridescence);
                accentColor=lerp(accentColor,lerp(pearl,float3(1,.91,.56),shine*.25),_Iridescence*.65);                // Replace only each region's share of a blended pixel. Keep the outline's share.
                float3 change=regions.r*(finish-float3(249,237,209)/255.0);
                change+=regions.g*(lightMetal-153.0/255.0);
                change+=regions.b*(darkMetal-101.0/255.0);
                change+=regions.a*(hubs-67.0/255.0);
                change+=accent*(accentColor-float3(212,56,52)/255.0);
                float4 faceMask=tex2D(_FaceMask,i.uv);
                if (repair > 0) faceMask=(tex2D(_FaceMask,left)+tex2D(_FaceMask,right)+tex2D(_FaceMask,down)+tex2D(_FaceMask,up))*.25;
                change+=faceMask.r*(face-float3(17,51,62)/255.0);
                change+=faceMask.g*(eye-float3(0,137,255)/255.0);
                change+=faceMask.b*(eye*.83-float3(21,116,212)/255.0);
                change+=faceMask.a*(eye*.52-float3(36,54,133)/255.0);
                source.rgb=saturate(source.rgb+change*_Strength);
                // Flat interiors use the exact palette color, avoiding compression speckles.
                if (_Strength > .99)
                {
                    if (regions.r > .98) source.rgb=finish;
                    if (regions.g > .98) source.rgb=lightMetal;
                    if (regions.b > .98) source.rgb=darkMetal;
                    if (regions.a > .98) source.rgb=hubs;
                    if (accent > .98) source.rgb=accentColor;
                    if (faceMask.r > .98) source.rgb=face;
                    if (faceMask.g > .98) source.rgb=eye;
                    if (faceMask.b > .98) source.rgb=eye*.83;
                    if (faceMask.a > .98) source.rgb=eye*.52;
                }
                #ifndef UNITY_COLORSPACE_GAMMA
                source.rgb=GammaToLinearSpace(source.rgb);
                #endif
                #ifdef UNITY_UI_CLIP_RECT
                source.a*=UnityGet2DClipping(i.local,_ClipRect);
                #endif
                source*=i.color;
                source.rgb*=source.a;
                return source;
            }
            ENDCG
        }
    }
}



