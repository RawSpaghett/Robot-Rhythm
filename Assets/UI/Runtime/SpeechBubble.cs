using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RobotRhythm.UI
{
    public sealed class SpeechBubble : MaskableGraphic
    {
        [SerializeField] private Color outline = new Color(0.067f, 0.184f, 0.224f, 1f);
        [SerializeField, Min(0f)] private float borderWidth = 3f;
        [SerializeField] private Vector2 shadowOffset = new Vector2(1.5f, -2f);

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            var points = new List<Vector2>();
            Curve(points, new Vector2(.5f,.94f), new Vector2(.77f,.94f), new Vector2(.97f,.80f), new Vector2(.97f,.58f), 24);
            Curve(points, new Vector2(.97f,.58f), new Vector2(.97f,.35f), new Vector2(.78f,.23f), new Vector2(.60f,.23f), 24);
            Curve(points, new Vector2(.60f,.23f), new Vector2(.57f,.14f), new Vector2(.54f,.06f), new Vector2(.5f,.03f), 12);
            Curve(points, new Vector2(.5f,.03f), new Vector2(.46f,.06f), new Vector2(.43f,.14f), new Vector2(.40f,.23f), 12);
            Curve(points, new Vector2(.40f,.23f), new Vector2(.22f,.23f), new Vector2(.03f,.35f), new Vector2(.03f,.58f), 24);
            Curve(points, new Vector2(.03f,.58f), new Vector2(.03f,.80f), new Vector2(.23f,.94f), new Vector2(.5f,.94f), 24);
            var rect = rectTransform.rect;
            var edge = new Vector2[points.Count];
            var fill = new Vector2[points.Count];
            var shadow = new Vector2[points.Count];
            for (int i = 0; i < points.Count; i++)
                edge[i] = new Vector2(rect.xMin + points[i].x * rect.width, rect.yMin + points[i].y * rect.height);
            for (int i = 0; i < edge.Length; i++)
            {
                Vector2 incoming = (edge[i] - edge[(i + edge.Length - 1) % edge.Length]).normalized;
                Vector2 outgoing = (edge[(i + 1) % edge.Length] - edge[i]).normalized;
                Vector2 a = new Vector2(-incoming.y, incoming.x);
                Vector2 b = new Vector2(-outgoing.y, outgoing.x);
                Vector2 normal = (a + b).normalized;
                fill[i] = edge[i] - normal * (borderWidth / Mathf.Max(.5f, Vector2.Dot(normal, b)));
                shadow[i] = edge[i] + shadowOffset;
            }
            float feather = CutPanel.FeatherWidth(this);
            CutPanel.DrawPolygon(mesh, shadow, outline, feather);
            CutPanel.DrawPolygon(mesh, edge, outline, feather, false);
            CutPanel.DrawPolygon(mesh, fill, color, feather, false);
        }

        private static void Curve(List<Vector2> points, Vector2 a, Vector2 b, Vector2 c, Vector2 d, int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                float t = (float)i / steps;
                float u = 1f - t;
                points.Add(u*u*u*a + 3f*u*u*t*b + 3f*u*t*t*c + t*t*t*d);
            }
        }
    }
}
