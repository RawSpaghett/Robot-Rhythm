using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Outline the rich-text percentage without thickening the adjacent ACCURACY label.
[RequireComponent(typeof(Text))]
public class AccuracyNumberOutline : BaseMeshEffect
{
    public Color outlineColor = new Color32(23, 49, 60, 255);
    [Range(.1f, 2f)] public float thickness = .8f;

    public override void ModifyMesh(VertexHelper mesh)
    {
        if (!IsActive()) return;
        var original = new List<UIVertex>();
        mesh.GetUIVertexStream(original);
        var outlined = new List<UIVertex>();
        Color32 labelColor = graphic.color;
        for (int direction = 0; direction < 8; direction++)
        {
            float angle = direction * Mathf.PI / 4;
            var offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * thickness;
            foreach (var source in original)
            {
                if (source.color.r == labelColor.r && source.color.g == labelColor.g && source.color.b == labelColor.b) continue;
                var vertex = source;
                vertex.position += offset;
                var color = (Color32)outlineColor;
                color.a = (byte)(color.a * source.color.a / 255);
                vertex.color = color;
                outlined.Add(vertex);
            }
        }
        outlined.AddRange(original);
        mesh.Clear();
        mesh.AddUIVertexTriangleStream(outlined);
    }
}
