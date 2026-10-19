using System;
using UnityEngine;

public enum RobotTimingVisual { Default, Perfect, EarlyLate, Failed }

// Each action keeps its existing frames as the blue/default sequence.
// Assign already-colored artwork here; no SpriteRenderer tint is applied.
[Serializable]
public class RobotAnimationVariations
{
    [Tooltip("Green: successful input meeting the obstacle's perfect threshold.")]
    [SerializeField] private Sprite[] perfectFrames = new Sprite[0];
    [Tooltip("Yellow: successful input inside the window but below perfect accuracy.")]
    [SerializeField] private Sprite[] earlyLateFrames = new Sprite[0];
    [Tooltip("Red: wrong input or failed timing. Also used for sprite-only missed feedback.")]
    [SerializeField] private Sprite[] failedFrames = new Sprite[0];

    public Sprite[] Select(RobotTimingVisual visual, Sprite[] defaultFrames)
    {
        Sprite[] selected = null;
        switch (visual)
        {
            case RobotTimingVisual.Perfect: selected = perfectFrames; break;
            case RobotTimingVisual.EarlyLate: selected = earlyLateFrames; break;
            case RobotTimingVisual.Failed: selected = failedFrames; break;
        }
        return IsValid(selected) ? selected : defaultFrames;
    }

    public static bool IsValid(Sprite[] frames)
    {
        if (frames == null || frames.Length == 0) return false;
        foreach (Sprite frame in frames)
            if (frame == null) return false;
        return true;
    }
}
