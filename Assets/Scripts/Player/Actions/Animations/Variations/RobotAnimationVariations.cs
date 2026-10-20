using System;
using UnityEngine;

public enum RobotTimingVisual { Default, Perfect, EarlyLate, Failed }

// Each action supplies its own blue/default sequence.
// Assign already-colored artwork here; no SpriteRenderer tint is applied.
[Serializable]
public class RobotAnimationVariations
{
    [SerializeField] private SpriteSheetSequence perfectSheet = new SpriteSheetSequence();
    [SerializeField] private SpriteSheetSequence earlyLateSheet = new SpriteSheetSequence();
    [SerializeField] private SpriteSheetSequence failedSheet = new SpriteSheetSequence();

    // Preserve original serialized field names so existing colored art survives.
    [SerializeField, HideInInspector] private Sprite[] perfectFrames = new Sprite[0];
    [SerializeField, HideInInspector] private Sprite[] earlyLateFrames = new Sprite[0];
    [SerializeField, HideInInspector] private Sprite[] failedFrames = new Sprite[0];

    public Sprite[] Select(RobotTimingVisual visual, Sprite[] defaultFrames)
    {
        Sprite[] selected = null;
        SpriteSheetSequence sheet = null;
        switch (visual)
        {
            case RobotTimingVisual.Perfect: sheet = perfectSheet; selected = perfectFrames; break;
            case RobotTimingVisual.EarlyLate: sheet = earlyLateSheet; selected = earlyLateFrames; break;
            case RobotTimingVisual.Failed: sheet = failedSheet; selected = failedFrames; break;
        }
        if (sheet != null && sheet.IsValid) return sheet.Frames;
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
