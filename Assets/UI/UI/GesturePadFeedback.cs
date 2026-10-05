using RobotRhythm.UI;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GesturePadFeedback : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private UiTheme theme;
    [SerializeField] private Graphic border;
    [SerializeField] private DirectionArrow[] directionArrows;
    [SerializeField] private RectTransform centerDot;
    [SerializeField] private GameObject chargeGroup;
    [SerializeField] private Image chargeFill;

    [Header("Touch Trace")]
    [SerializeField] private RectTransform originMark;
    [SerializeField] private RectTransform touchMark;
    [SerializeField] private RectTransform dragLine;
    [SerializeField, Min(1f)] private float lineWidth = 4f;
    [SerializeField, Min(0.05f)] private float releaseDuration = 0.3f;

    [Header("Pad Motion")]
    [SerializeField] private RectTransform visualSurface;
    [SerializeField, Range(1f, 1.8f)] private float selectedArrowScale = 1.45f;
    [SerializeField, Range(0f, 20f)] private float holdTilt = 14f;
    [SerializeField, Min(1f)] private float motionSpeed = 14f;
    [SerializeField, Min(0f)] private float holdOffset = 7f;
    [SerializeField, Range(.8f, 1f)] private float holdScale = .97f;
    private bool holding;
    private Vector2 surfaceOrigin;
    private UiController preferences;
    private Vector2 origin;
    private float releaseTime;
    private bool releasing;
    private Color releaseColor;
    public bool IsPressed { get; private set; }
    public SwipeDirection Direction { get; private set; }
    public float Charge { get; private set; }

    public void SetPreferences(UiController value) => preferences = value;

    private void Awake()
    {
        if (visualSurface != null) surfaceOrigin = visualSurface.anchoredPosition;
        ResetFeedback();
    }

    public void Begin(Vector2 point)
    {
        ResetFeedback();
        IsPressed = true;
        origin = point;
        originMark.gameObject.SetActive(true);
        touchMark.gameObject.SetActive(true);
        originMark.anchoredPosition = point;
        touchMark.anchoredPosition = point;
        border.color = theme.orange;
    }

    public void Show(GestureData gesture, Vector2 point, bool hasCharge, float charge)
    {
        if (!IsPressed)
            return;
        Direction = gesture.Direction;
        holding = IsPressed && gesture.IsHold;
        Charge = hasCharge ? Mathf.Clamp01(charge) : 0f;
        bool directional = Direction != SwipeDirection.None;
        UpdateArrows();
        border.color = gesture.IsHold ? theme.yellow : directional ? theme.teal : theme.orange;
        chargeGroup.SetActive(hasCharge);
        chargeFill.fillAmount = Charge;
        touchMark.anchoredPosition = ClampPoint(point);
        Vector2 delta = touchMark.anchoredPosition - origin;
        dragLine.gameObject.SetActive(delta.sqrMagnitude > 9f);
        dragLine.anchoredPosition = origin;
        dragLine.sizeDelta = new Vector2(delta.magnitude, lineWidth);
        dragLine.localEulerAngles = Vector3.forward * Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
    }

    public void Release(GestureData gesture, bool hasCharge, float charge)
    {
        IsPressed = false;
        releasing = true;
        releaseTime = 0f;
        Direction = gesture.Direction;
        holding = IsPressed && gesture.IsHold;
        Charge = hasCharge ? Mathf.Clamp01(charge) : 0f;
        releaseColor = gesture.IsTap ? theme.orange : theme.teal;
        border.color = releaseColor;
        touchMark.gameObject.SetActive(true);
        UpdateArrows();
        chargeGroup.SetActive(hasCharge);
        chargeFill.fillAmount = Charge;
    }

    private void Update()
    {
        UpdateMotion();
        if (!releasing)
            return;
        releaseTime += Time.unscaledDeltaTime;
        float progress = Mathf.Clamp01(releaseTime / releaseDuration);
        border.color = Color.Lerp(releaseColor, IdleBorder(), progress);
        if (preferences == null || !preferences.ReducedMotion)
        {
            touchMark.localScale = Vector3.one * (1f + progress * 0.3f);
            centerDot.localScale = Vector3.one * (1f + Mathf.Sin(progress * Mathf.PI) * 0.2f);
        }
        if (progress >= 1f)
            ResetFeedback();
    }

    public void ResetFeedback()
    {
        IsPressed = false;
        holding = false;
        releasing = false;
        Direction = SwipeDirection.None;
        Charge = 0f;
        if (theme == null || border == null)
            return;
        border.color = IdleBorder();
        UpdateArrows();
        if (centerDot != null)
            centerDot.localScale = Vector3.one;
        chargeGroup.SetActive(false);
        originMark.gameObject.SetActive(false);
        touchMark.gameObject.SetActive(false);
        touchMark.localScale = Vector3.one;
        dragLine.gameObject.SetActive(false);
    }

    private Vector2 ClampPoint(Vector2 point)
    {
        var rect = ((RectTransform)transform).rect;
        return new Vector2(Mathf.Clamp(point.x, 12f, rect.width - 12f), Mathf.Clamp(point.y, 12f, rect.height - 12f));
    }

    private Color IdleBorder()
    {
        return new Color(theme.cream.r, theme.cream.g, theme.cream.b, 0.35f);
    }

    private void UpdateArrows()
    {
        // The four Inspector slots follow Up, Down, Left, Right.
        for (int i = 0; i < directionArrows.Length; i++)
        {
            bool selected = Direction == (SwipeDirection)(i + 1);
            directionArrows[i].color = selected ? Color.Lerp(theme.teal, theme.yellow, Charge) :
                new Color(theme.cream.r, theme.cream.g, theme.cream.b, 0.25f);
        }
    }

    private void UpdateMotion()
    {
        bool reduced = preferences != null && preferences.ReducedMotion;
        float blend = reduced ? 1f : 1f - Mathf.Exp(-motionSpeed * Time.unscaledDeltaTime);
        for (int i = 0; i < directionArrows.Length; i++)
        {
            bool selected = IsPressed && Direction == (SwipeDirection)(i + 1);
            float scale = selected && !reduced ? selectedArrowScale : 1f;
            var arrow = directionArrows[i].rectTransform;
            arrow.localScale = Vector3.Lerp(arrow.localScale, Vector3.one * scale, blend);
        }
        if (visualSurface == null) return;
        Vector2 direction = Vector2.zero;
        if (holding && !reduced)
        {
            if (Direction == SwipeDirection.Up) direction = Vector2.up;
            else if (Direction == SwipeDirection.Down) direction = Vector2.down;
            else if (Direction == SwipeDirection.Left) direction = Vector2.left;
            else if (Direction == SwipeDirection.Right) direction = Vector2.right;
        }
        visualSurface.localRotation = Quaternion.Slerp(visualSurface.localRotation,
            Quaternion.Euler(direction.y * holdTilt, -direction.x * holdTilt, -direction.x * 2f), blend);
        visualSurface.anchoredPosition = Vector2.Lerp(visualSurface.anchoredPosition, surfaceOrigin + direction * holdOffset, blend);
        visualSurface.localScale = Vector3.Lerp(visualSurface.localScale, Vector3.one * (direction == Vector2.zero ? 1f : holdScale), blend);
    }

    private void OnDisable()
    {
        ResetFeedback();
        foreach (var arrow in directionArrows)
            if (arrow != null) arrow.rectTransform.localScale = Vector3.one;
        if (visualSurface == null) return;
        visualSurface.localRotation = Quaternion.identity;
        visualSurface.localScale = Vector3.one;
        visualSurface.anchoredPosition = surfaceOrigin;
    }
}
