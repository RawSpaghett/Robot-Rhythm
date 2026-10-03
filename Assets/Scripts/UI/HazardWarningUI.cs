using RobotRhythm.UI;
using UnityEngine;

public class HazardWarningUI : MonoBehaviour
{
    [Header("Hazard and Speaker")]
    [SerializeField] private ObstacleBase obstacle;
    [SerializeField] private ObstacleSpawner obstacleSpawner;
    [SerializeField] private RhythmManager rhythmManager;
    [SerializeField] private Transform speaker;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Vector3 worldOffset = new Vector3(0.6f, 2f, 0f);
    [SerializeField] private SpriteRenderer speakerVisual;
    [SerializeField] private Vector2 headOffset = new Vector2(100f, 42f);

    [Header("Bubble")]
    [SerializeField] private RectTransform bubble;
    [SerializeField] private GameObject attention;
    [SerializeField] private GameObject direction;
    [SerializeField, Min(0.1f)] private float advanceWarning = 1.1f;
    [SerializeField, Min(0f)] private float attentionDuration = 0.25f;
    [Header("Entrance")]
    [SerializeField, Min(0f)] private float entranceDistance = 6f;
    [SerializeField, Min(.01f)] private float entranceDuration = .14f;
    private UiController preferences;

    public bool IsVisible => bubble != null && bubble.gameObject.activeSelf;
    public SpriteRenderer SpeakerVisual => speakerVisual;
    public Camera WorldCamera => worldCamera;
    public void SetPreferences(UiController value) => preferences = value;

    private void LateUpdate()
    {
        if (obstacleSpawner != null)
            obstacle = obstacleSpawner.GetNextObstacle();
        double currentTime = rhythmManager != null ? rhythmManager.SongTime : Time.timeAsDouble;
        double remaining = obstacle != null ? obstacle.TargetTime - currentTime : double.MaxValue;
        bool visible = obstacle != null && obstacle.IsActive && !obstacle.IsResolved &&
            remaining <= advanceWarning && Time.timeScale > 0f && speaker != null && worldCamera != null;
        bubble.gameObject.SetActive(visible);
        if (!visible)
            return;

        Vector3 position = speaker.position + worldOffset;
        if (speakerVisual != null)
        {
            Bounds bounds = speakerVisual.bounds;
            position = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        }
        Vector3 screen = worldCamera.WorldToScreenPoint(position);
        if (screen.z <= 0f)
        {
            bubble.gameObject.SetActive(false);
            return;
        }
        bool alert = remaining > advanceWarning - attentionDuration;
        attention.SetActive(alert);
        direction.SetActive(!alert);
        // The existing obstacle classes decide which action clears the hazard.
        direction.transform.localRotation = Quaternion.Euler(0f, 0f, obstacle is DuckObstacle ? -90f : 90f);
        var parent = (RectTransform)bubble.parent;
        Canvas canvas = bubble.GetComponentInParent<Canvas>().rootCanvas;
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, uiCamera, out var local);
        if (speakerVisual != null)
            local += headOffset;
        float rise = preferences != null && preferences.ReducedMotion ? 0f :
            -entranceDistance * (1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((float)(advanceWarning - remaining) / entranceDuration)));
        float halfWidth = bubble.rect.width * 0.5f;
        float halfHeight = bubble.rect.height * 0.5f;
        bubble.anchoredPosition = new Vector2(
            Mathf.Clamp(local.x, parent.rect.xMin + halfWidth, parent.rect.xMax - halfWidth),
            Mathf.Clamp(local.y + rise, parent.rect.yMin + halfHeight, parent.rect.yMax - halfHeight));
    }

    private void OnDisable()
    {
        if (bubble != null)
            bubble.gameObject.SetActive(false);
    }
}
