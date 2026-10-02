using RobotRhythm.UI;
using UnityEngine;

public class HazardWarningUI : MonoBehaviour
{
    [Header("Hazard and Speaker")]
    [SerializeField] private ObstacleBase obstacle;
    [SerializeField] private Transform speaker;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Vector3 worldOffset = new Vector3(0.6f, 2f, 0f);

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
    public void SetPreferences(UiController value) => preferences = value;

    private void LateUpdate()
    {
        double remaining = obstacle != null ? obstacle.TargetTime - Time.timeAsDouble : double.MaxValue;
        bool visible = obstacle != null && obstacle.IsActive && !obstacle.IsResolved &&
            remaining <= advanceWarning && Time.timeScale > 0f && speaker != null && worldCamera != null;
        bubble.gameObject.SetActive(visible);
        if (!visible)
            return;

        Vector3 screen = worldCamera.WorldToScreenPoint(speaker.position + worldOffset);
        if (screen.z <= 0f)
        {
            bubble.gameObject.SetActive(false);
            return;
        }
        bool alert = remaining > advanceWarning - attentionDuration;
        attention.SetActive(alert);
        direction.SetActive(!alert);
        var parent = (RectTransform)bubble.parent;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, null, out var local);
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
