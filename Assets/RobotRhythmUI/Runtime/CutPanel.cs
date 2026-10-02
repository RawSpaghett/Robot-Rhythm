using UnityEngine;
using UnityEngine.UI;

namespace RobotRhythm.UI
{
    public sealed class CutPanel : MaskableGraphic
    {
        public float cut = 16;
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var r = rectTransform.rect;
            float c = Mathf.Min(cut, Mathf.Min(r.width, r.height) * .4f);
            Vector2[] points =
            {
                new Vector2(r.xMin + c, r.yMax),
                new Vector2(r.xMax, r.yMax),
                new Vector2(r.xMax, r.yMin + c),
                new Vector2(r.xMax - c, r.yMin),
                new Vector2(r.xMin, r.yMin),
                new Vector2(r.xMin, r.yMax - c)
            };
            DrawPolygon(mesh, points, color, FeatherWidth(this));
        }

        internal static float FeatherWidth(MaskableGraphic graphic)
        {
            var root = graphic.canvas != null ? graphic.canvas.rootCanvas : null;
            if (root == null) return 1f;
            float rootScale = Mathf.Max(0.000001f, Mathf.Abs(root.transform.lossyScale.x));
            float pixelsPerUnit = Mathf.Abs(graphic.rectTransform.lossyScale.x) / rootScale * root.scaleFactor;
            return 1f / Mathf.Max(0.01f, pixelsPerUnit);
        }

        internal static void DrawPolygon(VertexHelper mesh, Vector2[] points, Color tint, float feather, bool clear = true)
        {
            if (clear) mesh.Clear();
            var contour = new System.Collections.Generic.List<Vector2>();
            foreach (var point in points)
                if (contour.Count == 0 || (point - contour[contour.Count - 1]).sqrMagnitude > 0.000001f)
                    contour.Add(point);
            if (contour.Count > 1 && (contour[0] - contour[contour.Count - 1]).sqrMagnitude < 0.000001f)
                contour.RemoveAt(contour.Count - 1);
            if (contour.Count < 3) return;
            points = contour.ToArray();
            int start = mesh.currentVertCount;
            Vector2 center = Vector2.zero;
            float area = 0f;
            for (int i = 0; i < points.Length; i++)
            {
                center += points[i];
                var next = points[(i + 1) % points.Length];
                area += points[i].x * next.y - next.x * points[i].y;
            }
            center /= points.Length;
            mesh.AddVert(center, tint, Vector2.zero);
            foreach (var point in points)
                mesh.AddVert(point, tint, Vector2.zero);
            // Ear clipping keeps concave tails and arrow shoulders inside the silhouette.
            var remaining = new System.Collections.Generic.List<int>();
            for (int i = 0; i < points.Length; i++) remaining.Add(i);
            float winding = area < 0f ? -1f : 1f;
            while (remaining.Count > 2)
            {
                bool clipped = false;
                for (int i = 0; i < remaining.Count; i++)
                {
                    int a = remaining[(i + remaining.Count - 1) % remaining.Count];
                    int b = remaining[i];
                    int c = remaining[(i + 1) % remaining.Count];
                    if (Cross(points[b] - points[a], points[c] - points[b]) * winding <= 0.00001f) continue;
                    bool contains = false;
                    foreach (int p in remaining)
                    {
                        if (p == a || p == b || p == c) continue;
                        if (Cross(points[b] - points[a], points[p] - points[a]) * winding >= 0f &&
                            Cross(points[c] - points[b], points[p] - points[b]) * winding >= 0f &&
                            Cross(points[a] - points[c], points[p] - points[c]) * winding >= 0f)
                        { contains = true; break; }
                    }
                    if (contains) continue;
                    mesh.AddTriangle(start + a + 1, start + b + 1, start + c + 1);
                    remaining.RemoveAt(i);
                    clipped = true;
                    break;
                }
                if (!clipped) break;
            }

            Color transparent = new Color(tint.r, tint.g, tint.b, 0f);
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 incoming = (points[i] - points[(i + points.Length - 1) % points.Length]).normalized;
                Vector2 outgoing = (points[(i + 1) % points.Length] - points[i]).normalized;
                float sign = area < 0f ? 1f : -1f;
                Vector2 a = new Vector2(-incoming.y, incoming.x) * sign;
                Vector2 b = new Vector2(-outgoing.y, outgoing.x) * sign;
                Vector2 bisector = (a + b).normalized;
                float distance = feather / Mathf.Max(0.25f, Vector2.Dot(bisector, b));
                mesh.AddVert(points[i] + bisector * distance, transparent, Vector2.zero);
            }
            for (int i = 0; i < points.Length; i++)
            {
                int next = (i + 1) % points.Length;
                mesh.AddTriangle(start + i + 1, start + i + 1 + points.Length, start + next + 1 + points.Length);
                mesh.AddTriangle(start + i + 1, start + next + 1 + points.Length, start + next + 1);
            }
        }

        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
    }
}
