using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class PerfectConfetti : MaskableGraphic
{
    public float Progress { get; set; }
    private Piece[] pieces;
    private readonly Color32[] colors = { new Color32(255,207,72,255), new Color32(255,239,201,255),
        new Color32(238,114,87,255), new Color32(106,164,160,255) };
    private readonly Color ink = new Color32(17,23,15,255);

    class Piece
    {
        public Vector2 origin;
        public float delay, speedX, speedY, gravity, drag, drift, spin, phase, size, life;
        public int shape;
        public Color tint;
    }

    // Separate small bursts keep the paper pieces from moving like one fountain.
    public void Prepare(bool perfect)
    {
        var random = new System.Random();
        pieces = new Piece[perfect ? 86 : 23];
        Vector3[] bursts = perfect ? new[] { new Vector3(.12f,.28f,1.3f), new Vector3(.88f,.34f,1.5f),
            new Vector3(.1f,.52f,1.95f), new Vector3(.91f,.42f,2.3f), new Vector3(.18f,.25f,2.65f) }
            : new[] { new Vector3(.35f,.5f,1f), new Vector3(.64f,.48f,1.2f), new Vector3(.4f,.45f,1.66f) };
        for(int i=0;i<pieces.Length;i++)
        {
            Vector3 burst = bursts[random.Next(bursts.Length)];
            pieces[i] = new Piece {
                origin = new Vector2(burst.x,burst.y), delay = burst.z + (float)random.NextDouble()*.16f,
                speedX = perfect ? (burst.x>.5f?-1:1)*Range(random,-45,195) : Range(random,-100,100),
                speedY = perfect ? Range(random,145,330) : Range(random,75,200),
                gravity=Range(random,48,116), drag=Range(random,.3f,1.4f), drift=Range(random,-22,22),
                spin=Range(random,-5,5), phase=Range(random,0,6.28f), size=Range(random,5,11),
                life=Range(random,2.5f,4.8f), shape=random.Next(5), tint=colors[random.Next(colors.Length)]
            };
        }
    }

    static float Range(System.Random random,float min,float max) { return Mathf.Lerp(min,max,(float)random.NextDouble()); }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if(Progress<=0 || Progress>=1 || pieces==null) return;
        Rect rect = GetPixelAdjustedRect();
        foreach(var piece in pieces)
        {
            float age = Progress*7-piece.delay;
            if(age<0 || age>piece.life) continue;
            float scale=rect.width/790f;
            Vector2 center = new Vector2(rect.xMin+piece.origin.x*rect.width,rect.yMin+piece.origin.y*rect.height);
            center += new Vector2(piece.speedX*(1-Mathf.Exp(-age*piece.drag))/piece.drag + piece.drift*age
                + (Mathf.Sin(age*4+piece.phase)-Mathf.Sin(piece.phase))*14,
                piece.speedY*age-piece.gravity*age*age)*scale;
            float rotation=piece.phase+piece.spin*age+.5f*Mathf.Sin(age*4+piece.phase);
            float flip=.22f+.78f*Mathf.Abs(Mathf.Cos(age*3+piece.phase));
            float alpha=Mathf.Clamp01((piece.life-age)*1.4f)*color.a;
            Color fill=piece.tint; fill.a=alpha;
            Color border=ink; border.a=alpha;
            Vector2[] points;
            if(piece.shape==0)
            {
                // Curling paper strip, drawn as connected outlined segments.
                points=new Vector2[10];
                for(int j=0;j<points.Length;j++) points[j]=new Vector2(Mathf.Sin(j*.8f+piece.phase+age)*piece.size*.7f,j*piece.size*.3f-piece.size*1.5f);
                Transform(points,center,rotation,flip,scale);
                for(int j=1;j<points.Length;j++) Line(mesh,points[j-1],points[j],5*scale,border);
                for(int j=1;j<points.Length;j++) Line(mesh,points[j-1],points[j],2.5f*scale,fill);
            }
            else
            {
                if(piece.shape==1)
                {
                    points=new Vector2[10];
                    for(int j=0;j<10;j++) {
                        float a=j*Mathf.PI/5;
                        float radius=piece.size*(j%2==0?1:.48f)*(1+.12f*Mathf.Sin(j*5+piece.phase));
                        points[j]=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;
                    }
                }
                else points=new[] { new Vector2(-.6f,-1),new Vector2(.5f,-.8f),new Vector2(.8f,.5f),
                    new Vector2(.25f,1.1f),new Vector2(-.7f,.7f) };
                if(piece.shape!=1) for(int j=0;j<points.Length;j++) points[j]*=piece.size;
                Transform(points,center,rotation,flip,scale);
                int first=mesh.currentVertCount;
                mesh.AddVert(center,fill,Vector2.zero);
                foreach(var point in points) mesh.AddVert(point,fill,Vector2.zero);
                for(int j=0;j<points.Length;j++) mesh.AddTriangle(first,first+j+1,first+(j+1)%points.Length+1);
                for(int j=0;j<points.Length;j++) Line(mesh,points[j],points[(j+1)%points.Length],(1.8f+.4f*Mathf.Sin(j+piece.phase))*scale,border);
            }
        }
    }

    static void Transform(Vector2[] points,Vector2 center,float angle,float flip,float scale)
    {
        float sine=Mathf.Sin(angle), cosine=Mathf.Cos(angle);
        for(int i=0;i<points.Length;i++) {
            Vector2 p=points[i]*scale; p.x*=flip;
            points[i]=center+new Vector2(p.x*cosine-p.y*sine,p.x*sine+p.y*cosine);
        }
    }

    static void Line(VertexHelper mesh,Vector2 a,Vector2 b,float width,Color tint)
    {
        Vector2 direction=(b-a).normalized;
        Vector2 side=new Vector2(-direction.y,direction.x)*width*.5f;
        int first=mesh.currentVertCount;
        mesh.AddVert(a-side,tint,Vector2.zero); mesh.AddVert(a+side,tint,Vector2.zero);
        mesh.AddVert(b+side,tint,Vector2.zero); mesh.AddVert(b-side,tint,Vector2.zero);
        mesh.AddTriangle(first,first+1,first+2); mesh.AddTriangle(first,first+2,first+3);
    }
}
