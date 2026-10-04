using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class GestureButtonInput : MonoBehaviour, IPointerDownHandler,
    IPointerUpHandler, IDragHandler, IInitializePotentialDragHandler
{
    [Header("Gesture Settings")]
    [SerializeField, Min(0.01f)] private float minimumHoldDuration = 0.25f;
    [SerializeField, Min(0f)] private float tapMovementTolerance = 25f;
    [SerializeField, Min(1f)] private float minimumSwipeDistance = 60f;
    [Tooltip("How strongly one axis must dominate to recognize a direction.")]
    [SerializeField, Min(1f)] private float directionDominance = 1.2f;

    [Header("Actions - First Matching Enabled Action Wins")]
    [SerializeField] private ButtonActionBase[] actions = new ButtonActionBase[0];

    [Header("Feedback")]
    [SerializeField] private SwipeDirectionIndicator directionIndicator;
    [SerializeField] private ChargeIndicator chargeIndicator;
    [SerializeField] private GesturePadFeedback padFeedback;

    private Button button;
    private RectTransform buttonRect;
    private bool trackingPointer;
    private int activePointerId;
    private Vector2 pressPosition;
    private Vector2 currentPosition;
    private float maximumTravel;
    private bool swipeThresholdReached;
    private double pressTime;
    private SwipeDirection trackedDirection = SwipeDirection.None;
    private double directionStartTime;

    public event System.Action<GestureData> GesturePerformed;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonRect = GetComponent<RectTransform>();
        CancelGesture();
    }

    private void Update()
    {
        if (!trackingPointer) return;
        if (!CanAcceptInput()) { CancelGesture(); return; }
        UpdateFeedback();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (trackingPointer || !CanAcceptInput() ||
            eventData.button != PointerEventData.InputButton.Left) return;

        if (!TryGetPosition(eventData, out Vector2 position)) return;
        trackingPointer = true;
        activePointerId = eventData.pointerId;
        pressPosition = currentPosition = position;
        maximumTravel = 0f;
        swipeThresholdReached = false;
        pressTime = Time.unscaledTimeAsDouble;
        trackedDirection = SwipeDirection.None;
        directionStartTime = pressTime;
        HideFeedback();
        if (padFeedback != null)
            padFeedback.Begin(position - buttonRect.rect.min);
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!trackingPointer || eventData.pointerId != activePointerId) return;
        if (!CanAcceptInput()) { CancelGesture(); return; }
        if (!RecordPosition(eventData)) { CancelGesture(); return; }
        UpdateFeedback();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!trackingPointer || eventData.pointerId != activePointerId ||
            eventData.button != PointerEventData.InputButton.Left) return;
        if (!CanAcceptInput()) { CancelGesture(); return; }
        if (!RecordPosition(eventData)) { CancelGesture(); return; }

        bool releasedInside = RectTransformUtility.RectangleContainsScreenPoint(
            buttonRect, eventData.position, eventData.pressEventCamera);
        GestureData gesture = BuildGesture(releasedInside);
        ButtonActionBase action = FindAction(gesture);
        float charge = 0f;
        bool hasCharge = action != null && action.TryGetCharge(gesture, out charge);

        // Reset before external events run. No fallback to a second action.
        CancelGesture();
        if (action != null && action.isActiveAndEnabled)
        {
            action.Execute(gesture);
            if (padFeedback != null && Time.timeScale > 0f)
                padFeedback.Release(gesture, hasCharge, charge);
            GesturePerformed?.Invoke(gesture);
        }
    }

    private bool CanAcceptInput()
    {
        return isActiveAndEnabled && button != null && button.IsActive() &&
            button.IsInteractable() && Time.timeScale > 0f;
    }

    private bool TryGetPosition(PointerEventData eventData, out Vector2 position)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            buttonRect, eventData.position, eventData.pressEventCamera,
            out position);
    }

    private bool RecordPosition(PointerEventData eventData)
    {
        if (!TryGetPosition(eventData, out Vector2 position)) return false;
        currentPosition = position;
        float travel = Vector2.Distance(pressPosition, currentPosition);
        maximumTravel = Mathf.Max(maximumTravel, travel);
        if (travel >= minimumSwipeDistance) swipeThresholdReached = true;
        return true;
    }

    private SwipeDirection GetDirection()
    {
        Vector2 delta = currentPosition - pressPosition;
        float x = Mathf.Abs(delta.x);
        float y = Mathf.Abs(delta.y);
        if (y >= minimumSwipeDistance && y >= x * directionDominance)
            return delta.y > 0f ? SwipeDirection.Up : SwipeDirection.Down;
        if (x >= minimumSwipeDistance && x >= y * directionDominance)
            return delta.x > 0f ? SwipeDirection.Right : SwipeDirection.Left;
        return SwipeDirection.None;
    }

    private GestureData BuildGesture(bool releasedInside = false)
    {
        double now = Time.unscaledTimeAsDouble;
        SwipeDirection direction = GetDirection();

        // Only time continuously spent in the current direction counts.
        // Returning to Up after another direction starts a fresh charge.
        if (direction != trackedDirection)
        {
            trackedDirection = direction;
            directionStartTime = now;
        }

        double directionDuration = direction == SwipeDirection.None
            ? 0d : now - directionStartTime;
        double duration = now - pressTime;
        bool isTap = duration < minimumHoldDuration &&
            maximumTravel <= tapMovementTolerance &&
            !swipeThresholdReached && releasedInside;
        return new GestureData(duration, minimumHoldDuration, direction, isTap,
            directionDuration);
    }

    private ButtonActionBase FindAction(GestureData gesture)
    {
        if (actions == null) return null;
        foreach (ButtonActionBase action in actions)
            if (action != null && action.isActiveAndEnabled && action.Matches(gesture))
                return action;
        return null;
    }

    private void UpdateFeedback()
    {
        GestureData gesture = BuildGesture();
        if (directionIndicator != null)
            directionIndicator.ShowDirection(
                gesture.IsHold ? gesture.Direction : SwipeDirection.None);

        ButtonActionBase action = FindAction(gesture);
        float charge = 0f;
        bool hasCharge = action != null && action.TryGetCharge(gesture, out charge);
        if (chargeIndicator != null)
        {
            if (hasCharge) chargeIndicator.Show(charge);
            else chargeIndicator.Hide();
        }
        if (padFeedback != null)
            padFeedback.Show(gesture, currentPosition - buttonRect.rect.min, hasCharge, charge);
    }

    private void HideFeedback()
    {
        if (directionIndicator != null) directionIndicator.Hide();
        if (chargeIndicator != null) chargeIndicator.Hide();
        if (padFeedback != null) padFeedback.ResetFeedback();
    }

    private void CancelGesture()
    {
        trackingPointer = false;
        trackedDirection = SwipeDirection.None;
        directionStartTime = 0d;
        maximumTravel = 0f;
        swipeThresholdReached = false;
        HideFeedback();
    }

    private void OnDisable() => CancelGesture();
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) CancelGesture();
    }
    private void OnApplicationPause(bool paused)
    {
        if (paused) CancelGesture();
    }
}
