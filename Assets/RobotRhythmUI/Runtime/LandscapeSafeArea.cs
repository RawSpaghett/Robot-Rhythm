using UnityEngine;

namespace RobotRhythm.UI
{
    [RequireComponent(typeof(Canvas))]
    public sealed class LandscapeSafeArea : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private RectTransform frame;
        [SerializeField] private Vector2 referenceSize = new Vector2(1280f, 720f);

        private Canvas canvas;
        private Rect lastArea;
        private Vector2 lastScreen;
        private float lastCanvasScale;

        public RectTransform Frame => frame;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
        }

        public void Apply(Rect safeArea, Vector2 screen)
        {
            if (frame == null || screen.x <= 0f || screen.y <= 0f ||
                referenceSize.x <= 0f || referenceSize.y <= 0f)
                return;

            if (canvas == null)
                canvas = GetComponent<Canvas>();

            float fitScale = Mathf.Min(safeArea.width / referenceSize.x, safeArea.height / referenceSize.y);
            float canvasScale = Mathf.Max(0.001f, canvas.scaleFactor);

            // CanvasScaler handles text density; apply only the remaining safe-area fit here.
            frame.localScale = new Vector3(fitScale / canvasScale, fitScale / canvasScale, 1f);
            frame.anchoredPosition = (safeArea.center - screen * 0.5f) / canvasScale;
            lastCanvasScale = canvasScale;
            lastArea = safeArea;
            lastScreen = screen;
        }

        private void LateUpdate()
        {
            var screen = new Vector2(Screen.width, Screen.height);
            if (lastArea != Screen.safeArea || lastScreen != screen ||
                !Mathf.Approximately(lastCanvasScale, canvas.scaleFactor))
            {
                Apply(Screen.safeArea, screen);
            }
        }
    }
}
