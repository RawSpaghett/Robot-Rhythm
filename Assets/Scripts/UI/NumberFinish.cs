using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class NumberFinish : BaseMeshEffect
{
    [Header("Raised Finish")]
    [SerializeField] private Color shadowColor = new Color32(12, 32, 42, 255);
    [SerializeField, Range(0f, 6f)] private float depth = 3f;
    [SerializeField, Range(0f, 3f)] private float bevelWidth = 1.3f;
    [SerializeField, Range(0f, 1f)] private float highlight = .5f;
    [SerializeField, Range(0f, 1f)] private float shade = .35f;

    public void CopyStyleFrom(NumberFinish source)
    {
        shadowColor = source.shadowColor;
        depth = source.depth;
        bevelWidth = source.bevelWidth;
        highlight = source.highlight;
        shade = source.shade;
    }

    public override void ModifyMesh(VertexHelper mesh)
    {
        if (!IsActive() || mesh.currentVertCount == 0) return;
        var face = new List<UIVertex>();
        mesh.GetUIVertexStream(face);
        mesh.Clear();
        var finished = new List<UIVertex>(face.Count * 4);
        Color tint = graphic.color;
        float scale = GetComponent<Text>().fontSize / 64f;

        // Draw the depth and bevel behind the original letters, keeping the text still.
        AddLayer(finished, face, new Vector2(depth, -depth) * scale, shadowColor);
        AddLayer(finished, face, new Vector2(bevelWidth, -bevelWidth) * scale, Color.Lerp(tint, shadowColor, shade));
        AddLayer(finished, face, new Vector2(-bevelWidth, bevelWidth) * scale, Color.Lerp(tint, Color.white, highlight));

        float bottom = float.MaxValue;
        float top = float.MinValue;
        foreach (var vertex in face)
        {
            bottom = Mathf.Min(bottom, vertex.position.y);
            top = Mathf.Max(top, vertex.position.y);
        }
        for (int i = 0; i < face.Count; i++)
        {
            var vertex = face[i];
            float height = Mathf.InverseLerp(bottom, top, vertex.position.y);
            Color finish = Color.Lerp(Color.Lerp(tint, shadowColor, shade * .35f),
                Color.Lerp(tint, Color.white, highlight * .35f), height);
            finish.a = vertex.color.a / 255f;
            vertex.color = finish;
            face[i] = vertex;
        }
        finished.AddRange(face);
        mesh.AddUIVertexTriangleStream(finished);
    }

    private static void AddLayer(List<UIVertex> finished, List<UIVertex> face, Vector2 offset, Color tint)
    {
        var layer = new List<UIVertex>(face.Count);
        foreach (var original in face)
        {
            var vertex = original;
            vertex.position += new Vector3(offset.x, offset.y, 0f);
            Color color = tint;
            color.a *= original.color.a / 255f;
            vertex.color = color;
            layer.Add(vertex);
        }
        finished.AddRange(layer);
    }
}
