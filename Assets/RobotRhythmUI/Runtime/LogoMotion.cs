using UnityEngine;

namespace RobotRhythm.UI
{
    public sealed class LogoMotion : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UiController ui;
        [SerializeField] private RectTransform top;
        [SerializeField] private RectTransform bottom;

        [Header("Entrance")]
        [SerializeField, Min(0.01f)] private float topDuration = 0.75f;
        [SerializeField, Min(0.01f)] private float bottomDuration = 0.78f;
        [SerializeField, Min(0f)] private float bottomDelay = 0.12f;
        [SerializeField] private float topOffset = -72f;
        [SerializeField] private float bottomOffset = 84f;

        [Header("Idle Motion")]
        [SerializeField, Min(0f)] private float idleDelay = 1f;
        [SerializeField, Min(0f)] private float idleDistance = 3f;
        [SerializeField, Min(0.01f)] private float idlePeriod = 4.8f;

        private Vector2 topOrigin;
        private Vector2 bottomOrigin;
        private float started;

        public RectTransform TopLayer => top;

        private void Awake()
        {
            if (ui == null || top == null || bottom == null)
            {
                Debug.LogError("LogoMotion needs the menu and both logo layers.", this);
                enabled = false;
                return;
            }

            topOrigin = top.anchoredPosition;
            bottomOrigin = bottom.anchoredPosition;
        }

        private void OnEnable()
        {
            started = Time.unscaledTime;
        }

        private void LateUpdate()
        {
            Render(Time.unscaledTime - started);
        }

        public void Render(float elapsed)
        {
            if (ui == null || top == null || bottom == null)
                return;

            if (ui.ReducedMotion)
            {
                top.anchoredPosition = topOrigin;
                bottom.anchoredPosition = bottomOrigin;
                return;
            }

            float topProgress = EaseOutCubic(Mathf.Clamp01(elapsed / Mathf.Max(0.01f, topDuration)));
            float bottomProgress = EaseOutCubic(Mathf.Clamp01((elapsed - bottomDelay) / Mathf.Max(0.01f, bottomDuration)));
            float idleTime = Mathf.Max(0f, elapsed - idleDelay);
            float idle = Mathf.Sin(idleTime * Mathf.PI * 2f / Mathf.Max(0.01f, idlePeriod)) * idleDistance * Mathf.SmoothStep(0f, 1f, idleTime);

            top.anchoredPosition = topOrigin + new Vector2(topOffset * (1f - topProgress), idle);
            bottom.anchoredPosition = bottomOrigin + new Vector2(bottomOffset * (1f - bottomProgress), idle);
        }

        private static float EaseOutCubic(float progress)
        {
            return 1f - Mathf.Pow(1f - progress, 3f);
        }
    }
}
