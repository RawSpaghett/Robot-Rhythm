// A snapshot of one gesture. Only GestureButtonInput reads pointer events.
public readonly struct GestureData
{
    public double HeldDuration { get; }
    // Time continuously spent in Direction, measured from recognition.
    public double DirectionHeldDuration { get; }
    public float MinimumHoldDuration { get; }
    public SwipeDirection Direction { get; }
    public bool IsHold => HeldDuration >= MinimumHoldDuration;
    public bool IsTap { get; }

    public GestureData(double heldDuration, float minimumHoldDuration,
        SwipeDirection direction, bool isTap, double directionHeldDuration = 0d)
    {
        HeldDuration = heldDuration;
        DirectionHeldDuration = directionHeldDuration;
        MinimumHoldDuration = minimumHoldDuration;
        Direction = direction;
        IsTap = isTap;
    }
}
