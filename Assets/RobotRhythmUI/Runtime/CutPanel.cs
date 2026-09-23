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
            mesh.AddVert(r.center, color, Vector2.zero);
            foreach (var point in points)
                mesh.AddVert(point, color, Vector2.zero);
            for (int i = 0; i < points.Length; i++)
                mesh.AddTriangle(0, i + 1, (i + 1) % points.Length + 1);
        }
    }
}
