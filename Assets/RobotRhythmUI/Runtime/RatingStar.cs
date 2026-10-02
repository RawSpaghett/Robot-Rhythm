using UnityEngine;
using UnityEngine.UI;

namespace RobotRhythm.UI
{
    public sealed class RatingStar : MaskableGraphic
    {
        [Header("Star Finish")]
        [SerializeField] private Color insetColor = new Color32(15, 34, 44, 255);
        [SerializeField] private Color insetShade = new Color32(5, 16, 24, 255);
        [SerializeField] private Color insetRim = new Color32(74, 104, 118, 255);
        [SerializeField] private Color goldHighlight = new Color32(255, 225, 151, 255);
        [SerializeField] private Color goldShade = new Color32(167, 105, 37, 255);
        [SerializeField, Range(.05f, .3f)] private float bevelWidth = .18f;

        [Header("Pop Animation")]
        [SerializeField, Min(.01f)] private float jumpDuration = .55f;
        [SerializeField, Range(0f, .5f)] private float jumpHeight = .30f;
        [SerializeField, Range(0f, .5f)] private float jumpScale = .30f;
        private float jumpTime;
        private bool jumping;
        public float FillAmount { get; private set; }
        public bool Earned => FillAmount > 0f;
        public bool IsJumping => jumping;

        public void SetEarned(bool earned, bool animate)
        {
            SetFill(earned ? 1f : 0f, animate);
        }

        public void SetFill(float amount, bool animate)
        {
            amount = Mathf.Round(Mathf.Clamp01(amount) * 2f) * .5f;
            if (FillAmount == amount)
            {
                if (!animate && jumping) Settle();
                return;
            }
            jumping = amount > FillAmount && animate && isActiveAndEnabled;
            FillAmount = amount;
            jumpTime = 0f;
            SetVerticesDirty();
        }

        private void Update()
        {
            if (!jumping) return;
            jumpTime += Time.unscaledDeltaTime;
            if (jumpTime >= jumpDuration) jumping = false;
            SetVerticesDirty();
        }

        private void Settle()
        {
            jumping = false;
            jumpTime = jumpDuration;
            SetVerticesDirty();
        }

        protected override void OnDisable()
        {
            jumping = false;
            base.OnDisable();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            var rect = rectTransform.rect;
            float size = Mathf.Min(rect.width, rect.height);
            float feather = CutPanel.FeatherWidth(this);
            Vector2[] socket = Points(rect.center, size * .47f);
            mesh.Clear();
            DrawBevel(mesh, socket, rect.center, insetColor, insetShade, insetRim, feather, float.PositiveInfinity);
            if (!Earned) return;

            float progress = Mathf.Clamp01(jumpTime / jumpDuration);
            float lift = jumping ? Mathf.Sin(progress * Mathf.PI) : 0f;
            // A short compression at landing makes the gold face feel seated in its rim.
            float landing = jumping && progress > .72f ? Mathf.Sin((progress - .72f) / .28f * Mathf.PI) * .035f : 0f;
            Vector2 center = rect.center + Vector2.up * size * (jumpHeight * lift + .025f);
            float radius = size * .44f * (1f + jumpScale * lift - landing);
            Vector2[] shadow = Points(center + Vector2.down * size * (.04f + .045f * lift), radius);
            float clip = FillAmount < 1f ? center.x : float.PositiveInfinity;
            DrawFace(mesh, shadow, insetShade, feather, clip);
            Color face = Color.Lerp(color, goldHighlight, lift * .35f);
            DrawBevel(mesh, Points(center, radius), center, face, goldHighlight, goldShade, feather, clip);
            if (jumping && progress < .7f)
            {
                Color flash = goldHighlight;
                flash.a *= Mathf.Sin(progress / .7f * Mathf.PI);
                for (int i = 0; i < 3; i++)
                {
                    float angle = (45f + i * 45f) * Mathf.Deg2Rad;
                    Vector2 ray = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    Vector2 cross = new Vector2(-ray.y, ray.x);
                    Vector2 tip = center + ray * size * (.52f + progress * .16f);
                    Vector2[] spark = { tip + ray * size * .07f, tip + cross * size * .02f,
                        tip - ray * size * .07f, tip - cross * size * .02f };
                    CutPanel.DrawPolygon(mesh, spark, flash, feather, false);
                }
            }
        }

        private void DrawBevel(VertexHelper mesh, Vector2[] edge, Vector2 center, Color face, Color light, Color dark, float feather, float clip)
        {
            DrawFace(mesh, edge, face, feather, clip);
            var inner = new Vector2[edge.Length];
            for (int i = 0; i < edge.Length; i++)
                inner[i] = Vector2.Lerp(edge[i], center, bevelWidth);
            for (int i = 0; i < edge.Length; i++)
            {
                int next = (i + 1) % edge.Length;
                Vector2 tangent = (edge[next] - edge[i]).normalized;
                Vector2 normal = new Vector2(-tangent.y, tangent.x);
                float lighting = Mathf.Clamp01(.5f + Vector2.Dot(normal, new Vector2(-.6f, .8f)) * .5f);
                Color shade = Color.Lerp(dark, light, lighting);
                DrawFace(mesh, new[] { edge[i], edge[next], inner[next], inner[i] }, shade, 0f, clip);
            }
            DrawFace(mesh, inner, face, feather * .5f, clip);
        }

        private static void DrawFace(VertexHelper mesh, Vector2[] points, Color tint, float feather, float clip)
        {
            if (!float.IsPositiveInfinity(clip))
            {
                var left = new System.Collections.Generic.List<Vector2>();
                for (int i = 0; i < points.Length; i++)
                {
                    Vector2 a = points[i];
                    Vector2 b = points[(i + 1) % points.Length];
                    bool inside = a.x <= clip;
                    if (inside) left.Add(a);
                    if (inside != (b.x <= clip))
                        left.Add(Vector2.Lerp(a, b, (clip - a.x) / (b.x - a.x)));
                }
                points = left.ToArray();
            }
            if (points.Length >= 3) CutPanel.DrawPolygon(mesh, points, tint, feather, false);
        }

        private static Vector2[] Points(Vector2 center, float radius)
        {
            var points = new Vector2[10];
            for (int i = 0; i < points.Length; i++)
            {
                float angle = (90f - i * 36f) * Mathf.Deg2Rad;
                float r = i % 2 == 0 ? radius : radius * .46f;
                points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
            }
            return points;
        }
    }
}
