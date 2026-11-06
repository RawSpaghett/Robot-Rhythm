using UnityEngine;
using UnityEngine.UI;

namespace RobotRhythm.Feature5
{
    public class SkinPaper : MaskableGraphic
    {
        public Vector2[] outline;
        public float borderWidth = 4;

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (outline == null || outline.Length < 3) return;
            var rect = rectTransform.rect;
            float feather = 1 / Mathf.Max(.01f, canvas == null ? 1 : canvas.scaleFactor);
            var points = new Vector2[outline.Length];
            for (int i = 0; i < points.Length; i++)
                points[i] = rect.center + Vector2.Scale(outline[i], rect.size);
            mesh.AddVert(rect.center, color, Vector2.zero);
            for (int i = 0; i < points.Length; i++)
            {
                var tangent = (points[(i + 1) % points.Length] - points[(i + points.Length - 1) % points.Length]).normalized;
                var inward = new Vector2(-tangent.y, tangent.x);
                var black = new Color(0, 0, 0, color.a);
                mesh.AddVert(points[i] - inward * feather, Color.clear, Vector2.zero);
                mesh.AddVert(points[i], black, Vector2.zero);
                mesh.AddVert(points[i] + inward * borderWidth, black, Vector2.zero);
                mesh.AddVert(points[i] + inward * (borderWidth + feather), color, Vector2.zero);
            }
            for (int i = 0; i < points.Length; i++)
            {
                int a = 1 + i * 4, b = 1 + (i + 1) % points.Length * 4;
                for (int band = 0; band < 3; band++)
                {
                    mesh.AddTriangle(a + band, b + band, a + band + 1);
                    mesh.AddTriangle(a + band + 1, b + band, b + band + 1);
                }
                mesh.AddTriangle(0, a + 3, b + 3);
            }
        }
    }
}
