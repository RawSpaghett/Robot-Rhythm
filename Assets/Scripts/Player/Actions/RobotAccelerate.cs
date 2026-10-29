using System;
using UnityEngine;

// Visual action owned by ActionManager. Does not change gameplay movement speed.
[Serializable]
public class RobotAccelerate : ActionBase
{
    [SerializeField, Min(0.01f)] private float animationDuration = 0.5f;
    [Tooltip("Maximum forward offset in local units. Zero disables horizontal feedback.")]
    [SerializeField, Min(0f)] private float movementDistance = 0.3f;
    [SerializeField] private SpriteSheetSequence defaultSheet = new SpriteSheetSequence();
    [SerializeField] private SpriteSheetSequence perfectSheet = new SpriteSheetSequence();
    [SerializeField] private SpriteSheetSequence earlyLateSheet = new SpriteSheetSequence();
    [SerializeField] private SpriteSheetSequence failedSheet = new SpriteSheetSequence();

    [NonSerialized] private Sprite[] activeFrames;
    [NonSerialized] private Sprite previousSprite;
    [NonSerialized] private float timer;
    [NonSerialized] private float duration;
    [NonSerialized] private int currentFrame;
    [NonSerialized] private Vector3 restingLocalPosition;
    [NonSerialized] private float activeMovementDistance;

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

    // ActionManager uses this to open the input buffer near the action's end.
    public override float RemainingTime =>
        IsRunning ? Mathf.Max(0f, duration - timer) : 0f;

    public override bool TryBegin(RobotActionType type, float charge)
    {
        return TryBeginWithFeedback(type, charge, RobotTimingVisual.Default);
    }

    public bool TryBeginWithFeedback(RobotActionType type, float charge, RobotTimingVisual visual)
    {
        if (IsRunning || type != RobotActionType.Accelerate) return false;
        Sprite[] frames = GetAnimationFrames(visual);
        if (Renderer == null || !RobotAnimationVariations.IsValid(frames))
        {
            Debug.LogWarning("Accelerate: assign Robot Renderer and a sliced Default Sheet. " +
                "Use Refresh Frames From Sheet outside Play mode if needed.", LogContext);
            return false;
        }
        activeFrames = (Sprite[])frames.Clone();
        previousSprite = Renderer.sprite;
        // Capture each attempt so a later action returns to its own starting pose.
        if (Visual != null) restingLocalPosition = Visual.localPosition;
        activeMovementDistance = Mathf.Max(0f, movementDistance);
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
        // Use the sprite animation's timer: zero offset at both ends, peak halfway.
        float progress = Mathf.Clamp01(timer / duration);
        float offset = 4f * activeMovementDistance * progress * (1f - progress);
        if (Visual != null)
            Visual.localPosition = restingLocalPosition + Vector3.right * offset;

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
        // Completion, cancellation, and manager disable all restore the visual pose.
        if (IsRunning && Visual != null) Visual.localPosition = restingLocalPosition;
        if (IsRunning && Renderer != null) Renderer.sprite = previousSprite;
        IsRunning = false;
        activeFrames = null;
        previousSprite = null;
        timer = 0f;
        currentFrame = 0;
    }
}
