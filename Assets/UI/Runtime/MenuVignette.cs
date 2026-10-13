using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(CanvasRenderer))]
public class MenuVignette : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();Rect r=rectTransform.rect;const int steps=32;
        for(int y=0;y<=steps;y++)for(int x=0;x<=steps;x++)
        {
            float u=x/(float)steps,v=y/(float)steps;
            float radius=Mathf.Sqrt(Mathf.Pow((u-.5f)*2,2)+Mathf.Pow((v-.5f)*2,2));
            float a=Mathf.SmoothStep(0,.72f,Mathf.InverseLerp(.35f,1.35f,radius));
            vh.AddVert(new Vector2(r.x+u*r.width,r.y+v*r.height),new Color(0,0,0,a),Vector2.zero);
        }
        for(int y=0;y<steps;y++)for(int x=0;x<steps;x++){int k=y*(steps+1)+x;vh.AddTriangle(k,k+steps+1,k+1);vh.AddTriangle(k+1,k+steps+1,k+steps+2);}
    }
}

