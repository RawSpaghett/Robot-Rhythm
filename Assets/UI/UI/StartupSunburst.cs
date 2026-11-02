using UnityEngine;
using UnityEngine.UI;

// Draws the cover outside an expanding sunburst opening.
[RequireComponent(typeof(CanvasRenderer))]
public class StartupSunburst : MaskableGraphic
{
    public float Progress;
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect rect = rectTransform.rect;
        const int segments = 144;
        float outside = rect.size.magnitude;
        for(int i=0;i<=segments;i++)
        {
            float angle = i*Mathf.PI*2/segments;
            Vector2 direction = new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
            float inside = outside*.65f*Progress*(1+.16f*Mathf.Cos(angle*12));
            mesh.AddVert(rect.center+direction*inside,Color.black,Vector2.zero);
            mesh.AddVert(rect.center+direction*outside,Color.black,Vector2.zero);
            if(i>0)
            {
                int end=i*2;
                mesh.AddTriangle(end-2,end-1,end);
                mesh.AddTriangle(end,end-1,end+1);
            }
        }
    }
}
