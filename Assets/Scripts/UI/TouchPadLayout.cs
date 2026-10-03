using RobotRhythm.UI;
using UnityEngine;

public class TouchPadLayout : MonoBehaviour
{
    [SerializeField] private Vector2 edgeOffset = new Vector2(48f, 48f);
    private UiController preferences;

    public void Bind(UiController value)
    {
        if (preferences != null)
            preferences.ControlSideChanged -= Apply;
        preferences = value;
        if (preferences != null)
            preferences.ControlSideChanged += Apply;
        Apply(preferences == null || preferences.ControlsOnRight);
    }

    private void Start()
    {
        Apply(preferences == null || preferences.ControlsOnRight);
    }

    private void Apply(bool right)
    {
        var rect = (RectTransform)transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(right ? 1f : 0f, 0f);
        rect.anchoredPosition = new Vector2(right ? -edgeOffset.x : edgeOffset.x, edgeOffset.y);
    }

    private void OnDestroy()
    {
        if (preferences != null)
            preferences.ControlSideChanged -= Apply;
    }
}
