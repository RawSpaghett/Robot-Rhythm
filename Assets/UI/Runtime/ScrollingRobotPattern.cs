using UnityEngine;
using UnityEngine.UI;
using RobotRhythm.UI;

// One seamless lattice, with a shared phase across menu pages.
[RequireComponent(typeof(CanvasRenderer))]
public class ScrollingRobotPattern : MaskableGraphic
{
    public Sprite robot;
    public UiController preferences;
    private static float phase;
    private static int updatedFrame = -1;
    public override Texture mainTexture => robot ? robot.texture : s_WhiteTexture;
    void Update()
    {
        if (updatedFrame != Time.frameCount)
        {
            updatedFrame = Time.frameCount;
            if (!preferences || !preferences.ReducedMotion)
                phase = Mathf.Repeat(phase + Time.unscaledDeltaTime * 17f, 320f);
        }
        SetVerticesDirty();
    }
    private Color EdgeColor(Vector2 point, Rect rect) { float edge=Mathf.Max(Mathf.Abs((point.x-rect.center.x)/(rect.width*.5f)),Mathf.Abs((point.y-rect.center.y)/(rect.height*.5f))); Color tint=color; tint.a*=Mathf.Lerp(1f,.3f,Mathf.SmoothStep(0,1,Mathf.InverseLerp(.5f,1f,edge))); return tint; }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear(); if (!robot) return;
        Rect r=rectTransform.rect;
        Vector4 uv=UnityEngine.Sprites.DataUtility.GetOuterUV(robot);
        float angle=30f*Mathf.Deg2Rad;
        Vector2 xAxis=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
        Vector2 yAxis=new Vector2(-Mathf.Sin(angle),Mathf.Cos(angle));
        float extent=Mathf.Sqrt(r.width*r.width+r.height*r.height)*.5f+440;
        int count=Mathf.CeilToInt(extent/160);
        float height=120f*robot.rect.height/robot.rect.width;
        for(int row=-count;row<=count;row++)for(int col=-count;col<=count;col++)
        {
            Vector2 center=r.center+xAxis*(col*160f+(row%2)*80f+phase)+yAxis*(row*150f);
            Vector2 dx=xAxis*60f,dy=yAxis*(height*.5f);
            int k=vh.currentVertCount;
            vh.AddVert(center-dx-dy,EdgeColor(center-dx-dy,r),new Vector2(uv.x,uv.y));
            vh.AddVert(center-dx+dy,EdgeColor(center-dx+dy,r),new Vector2(uv.x,uv.w));
            vh.AddVert(center+dx+dy,EdgeColor(center+dx+dy,r),new Vector2(uv.z,uv.w));
            vh.AddVert(center+dx-dy,EdgeColor(center+dx-dy,r),new Vector2(uv.z,uv.y));
            vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);
        }
    }
}


