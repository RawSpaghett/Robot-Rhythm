using System;
using UnityEngine;

// Animation-only action owned by ActionManager. Does not change movement speed.
[Serializable]
public class RobotBrake : ActionBase
{
    [SerializeField, Min(0.01f)] private float animationDuration = 0.5f;
    [SerializeField] private SpriteSheetSequence defaultSheet = new SpriteSheetSequence();
    [SerializeField] private SpriteSheetSequence perfectSheet = new SpriteSheetSequence();
    [SerializeField] private SpriteSheetSequence earlyLateSheet = new SpriteSheetSequence();
    [SerializeField] private SpriteSheetSequence failedSheet = new SpriteSheetSequence();

    [NonSerialized] private Sprite[] activeFrames;
    [NonSerialized] private Sprite previousSprite;
    [NonSerialized] private float timer;
    [NonSerialized] private float duration;
    [NonSerialized] private int currentFrame;

    public Sprite[] GetAnimationFrames(RobotTimingVisual visual)
    {
        SpriteSheetSequence sequence = null;
        switch (visual)
        {
            case RobotTimingVisual.Perfect: sequence = perfectSheet; break;
            case RobotTimingVisual.EarlyLate: sequence = earlyLateSheet; break;
            case RobotTimingVisual.Failed: sequence = failedSheet; break;
        }
        if (sequence != null && sequence.IsValid) return sequence.Frames;
        return defaultSheet != null && defaultSheet.IsValid ? defaultSheet.Frames : null;
    }

    public override bool TryBegin(RobotActionType type, float charge)
    {
        return TryBeginWithFeedback(type, charge, RobotTimingVisual.Default);
    }

    public bool TryBeginWithFeedback(RobotActionType type, float charge, RobotTimingVisual visual)
    {
        if (IsRunning || type != RobotActionType.Brake) return false;
        Sprite[] frames = GetAnimationFrames(visual);
        if (Renderer == null || !RobotAnimationVariations.IsValid(frames))
        {
            Debug.LogWarning("Brake: assign Robot Renderer and a sliced Default Sheet. " +
                "Use Refresh Frames From Sheet outside Play mode if needed.", LogContext);
            return false;
        }
        activeFrames = (Sprite[])frames.Clone();
        previousSprite = Renderer.sprite;
        timer = 0f;
        duration = Mathf.Max(0.01f, animationDuration);
        currentFrame = 0;
        IsRunning = true;
        Renderer.sprite = activeFrames[0];
        return true;
    }

    public override void Tick(float deltaTime)
    {
        if (!IsRunning) return;
        if (Renderer == null) { Cancel(); return; }
        timer += Mathf.Max(0f, deltaTime);
        if (timer >= duration) { Cancel(); return; }
        int frame = Mathf.Min(Mathf.FloorToInt(timer / duration * activeFrames.Length),
            activeFrames.Length - 1);
        if (frame != currentFrame)
        {
            currentFrame = frame;
            Renderer.sprite = activeFrames[frame];
        }
    }

    public override void Cancel()
    {
        if (IsRunning && Renderer != null) Renderer.sprite = previousSprite;
        IsRunning = false;
        activeFrames = null;
        previousSprite = null;
        timer = 0f;
        currentFrame = 0;
    }
}
