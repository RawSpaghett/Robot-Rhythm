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
            foreach (var point in points)
                mesh.AddVert(new Vector2(r.xMin + point.x * r.width, r.yMin + point.y * r.height), color, Vector2.zero);
            mesh.AddTriangle(0, 5, 1);
            mesh.AddTriangle(0, 6, 5);
            mesh.AddTriangle(2, 4, 3);
        }
    }
}
