using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Closely spaced round offsets fill the gaps left by the four-corner UI outline.
[RequireComponent(typeof(Text))]
public sealed class RoundedTextOutline : BaseMeshEffect
{
    public Color strokeColor = new Color32(12, 32, 42, 255);
    [Min(0)] public float width = 3f;

    public override void ModifyMesh(VertexHelper mesh)
    {
        if (!IsActive() || width <= 0 || mesh.currentVertCount == 0) return;
        var face = new List<UIVertex>();
        mesh.GetUIVertexStream(face);
        const int segments = 24;
        var vertices = new List<UIVertex>(face.Count * (segments * 2 + 1));
        for (int ring = 1; ring <= 2; ring++)
        {
            float radius = width * ring * .5f;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2 / segments;
                var offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                foreach (var original in face)
                {
                    var vertex = original;
                    vertex.position += offset;
                    Color tint = strokeColor;
                    tint.a *= original.color.a / 255f;
                    vertex.color = tint;
                    vertices.Add(vertex);
                }
            }
        }
        vertices.AddRange(face);
        mesh.Clear();
        mesh.AddUIVertexTriangleStream(vertices);
    }
}
