using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class LoadingBurst : MaskableGraphic
{
    public float Progress;

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if (Progress <= 0) return;
        Rect rect = rectTransform.rect;
        float radius = rect.size.magnitude * .65f * Progress;
        const int segments = 96;
        mesh.AddVert(rect.center, Color.white, Vector2.zero);
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2 / segments;
            float ray = 1 + .16f * Mathf.Cos(angle * 12);
            Vector2 edge = rect.center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius * ray;
            mesh.AddVert(edge, Color.white, Vector2.zero);
            if (i > 0) mesh.AddTriangle(0, i, i + 1);
        }
    }
}
