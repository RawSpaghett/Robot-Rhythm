using UnityEngine;
using UnityEngine.UI;

namespace RobotRhythm.UI
{
    public sealed class DirectionArrow : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var r = rectTransform.rect;
            Vector2[] points =
            {
                new Vector2(0, .36f),
                new Vector2(.52f, .36f),
                new Vector2(.52f, 0),
                new Vector2(1, .5f),
                new Vector2(.52f, 1),
                new Vector2(.52f, .64f),
                new Vector2(0, .64f)
            };
            for (int i = 0; i < points.Length; i++)
                points[i] = new Vector2(r.xMin + points[i].x * r.width, r.yMin + points[i].y * r.height);
            CutPanel.DrawPolygon(mesh, points, color, CutPanel.FeatherWidth(this));
        }
    }
}
