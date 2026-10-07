using RobotRhythm.UI;
using UnityEngine;
using UnityEngine.UI;

public class BeatPulse : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RhythmManager rhythmManager;
    [SerializeField] private Graphic pulse;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private UiController preferences;

    [Header("Above the Robot")]
    [SerializeField] private SpriteRenderer robotVisual;
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private Vector2 headOffset = new Vector2(0f, 16f);

    [Header("Pulse")]
    [SerializeField, Range(0.05f, 1f)] private float fadeInBeats = 0.55f;
    [SerializeField, Range(0f, 0.5f)] private float sizeChange = 0.12f;
    [SerializeField] private Color restingColor = new Color(0.28f, 0.52f, 0.53f, 0.35f);
    [SerializeField] private Color beatColor = new Color(0.28f, 0.52f, 0.53f, 1f);
    [SerializeField] private Color downbeatColor = new Color(1f, 0.76f, 0.32f, 1f);

    public void Bind(RhythmManager value) => rhythmManager = value;

    public void Follow(SpriteRenderer robot, Camera camera)
    {
        robotVisual = robot;
        gameplayCamera = camera;
    }

    private void LateUpdate()
    {
        bool playing = rhythmManager != null && rhythmManager.isPlaying &&
            rhythmManager.beatMap != null && rhythmManager.SongTime >= 0 &&
            robotVisual != null && gameplayCamera != null;
        group.alpha = playing ? 1f : 0f;
        if (playing)
            FollowHead();
        if (!playing || Time.timeScale <= 0f || AudioListener.pause)
            return;

        double beat = rhythmManager.SongTime / rhythmManager.beatMap.SecondsPerBeat;
        int wholeBeat = (int)System.Math.Floor(beat);
        float phase = (float)(beat - wholeBeat);
        float strength = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(phase / fadeInBeats));
        int beatsPerMeasure = Mathf.Max(1, rhythmManager.beatMap.BeatsPerMeasure);
        Color bright = wholeBeat % beatsPerMeasure == 0 ? downbeatColor : beatColor;
        pulse.color = Color.Lerp(restingColor, bright, strength);
        bool reducedMotion = preferences != null && preferences.ReducedMotion;
        pulse.rectTransform.localScale = Vector3.one * (1f + (reducedMotion ? 0f : strength * sizeChange));
    }

    private void FollowHead()
    {
        // Sprite bounds follow both the jump position and the ducked height.
        Bounds bounds = robotVisual.bounds;
        Vector3 top = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        Vector3 screen = gameplayCamera.WorldToScreenPoint(top);
        var rect = (RectTransform)transform;
        var parent = (RectTransform)rect.parent;
        Canvas canvas = GetComponentInParent<Canvas>().rootCanvas;
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (screen.z <= 0f)
        {
            group.alpha = 0f;
            return;
        }
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, uiCamera, out Vector2 point);
        rect.localPosition = new Vector3(point.x + headOffset.x, point.y + headOffset.y, 0f);
    }
}
