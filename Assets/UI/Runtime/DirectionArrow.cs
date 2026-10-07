using UnityEngine;
using UnityEngine.UI;

namespace RobotRhythm.UI
{
    public sealed class DirectionArrow : MaskableGraphic
    {
        [SerializeField] private bool doubleArrow;
        public void SetDouble(bool value)
        {
            if (doubleArrow == value) return;
            doubleArrow = value;
            SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var r = rectTransform.rect;
            if (doubleArrow)
            {
                for (int arrow = 0; arrow < 2; arrow++)
                {
                    float x = arrow * .48f;
                    Vector2[] chevron = {
                        new Vector2(x,.08f), new Vector2(x+.17f,.08f),
                        new Vector2(x+.48f,.5f), new Vector2(x+.17f,.92f),
                        new Vector2(x,.92f), new Vector2(x+.30f,.5f)
                    };
                    for (int i=0;i<chevron.Length;i++)
                        chevron[i]=new Vector2(r.xMin+chevron[i].x*r.width,r.yMin+chevron[i].y*r.height);
                    CutPanel.DrawPolygon(mesh,chevron,color,CutPanel.FeatherWidth(this),false);
                }
                return;
            }
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
