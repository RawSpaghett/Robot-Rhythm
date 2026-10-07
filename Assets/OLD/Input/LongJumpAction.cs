using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class LongJumpAction : ButtonActionBase
{
    [Header("Charge Settings")]
    [Tooltip("Time spent continuously swiping up to reach maximum charge.")]
    [SerializeField, Min(0.01f)] private float fullChargeDuration = 1.25f;

    [Header("Long Jump Request")]
    [SerializeField]
    private UnityEvent<float> onLongJumpRequested =
        new UnityEvent<float>();

    public override bool Matches(GestureData gesture)
    {
        return gesture.IsHold && gesture.Direction == SwipeDirection.Up;
    }

    public override bool TryGetCharge(GestureData gesture, out float charge)
    {
        charge = 0f;
        if (!Matches(gesture))
            return false;

        charge = Mathf.Clamp01(
            (float)gesture.DirectionHeldDuration /
            Mathf.Max(0.01f, fullChargeDuration));
        return true;
    }

    public override void Execute(GestureData gesture)
    {
        if (TryGetCharge(gesture, out float charge))
            onLongJumpRequested.Invoke(charge);
    }
}
