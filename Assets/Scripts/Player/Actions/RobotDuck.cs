using System;
using UnityEngine;

// Serialized inside ActionManager. Plays the sprite sequence once.
[Serializable]
public class RobotDuck : ActionBase
{
    [Header("Duck Animation")]
    [Tooltip("Frames in order: enter duck, ducked pose, stand up.")]
    [SerializeField] private Sprite[] duckFrames = new Sprite[0];

    [Tooltip("Total sequence duration; each frame receives equal time.")]
    [SerializeField, Min(0.01f)] private float duckDuration = 0.5f;

    [NonSerialized] private Sprite spriteBeforeDuck;
    [NonSerialized] private Sprite[] activeFrames;
    [NonSerialized] private float duckTimer;
    [NonSerialized] private float activeDuckDuration;
    [NonSerialized] private int currentFrame;

    public override bool TryBegin(RobotActionType type, float charge)
    {
        if (IsRunning || type != RobotActionType.Duck)
            return false;

        if (Renderer == null || duckFrames == null || duckFrames.Length == 0)
        {
            Debug.LogWarning(
                "RobotDuck needs a renderer and at least one duck frame.",
                LogContext);
            return false;
        }

        for (int i = 0; i < duckFrames.Length; i++)
        {
            if (duckFrames[i] == null)
            {
                Debug.LogWarning(
                    $"RobotDuck: Duck Frames element {i} is unassigned.",
                    LogContext);
                return false;
            }
        }

        // Snapshot the sequence so Inspector edits cannot invalidate its length.
        activeFrames = (Sprite[])duckFrames.Clone();
        spriteBeforeDuck = Renderer.sprite;
        activeDuckDuration = Mathf.Max(0.01f, duckDuration);
        duckTimer = 0f;
        currentFrame = 0;
        IsRunning = true;
        Renderer.sprite = activeFrames[0];
        return true;
    }

    public override void Tick(float deltaTime)
    {
        if (!IsRunning) return;
        if (Renderer == null) { Cancel(); return; }

        duckTimer += Mathf.Max(0f, deltaTime);
        float progress = Mathf.Clamp01(duckTimer / activeDuckDuration);
        if (progress >= 1f) { Cancel(); return; }

        int frame = Mathf.Min(
            Mathf.FloorToInt(progress * activeFrames.Length),
            activeFrames.Length - 1);

        if (frame != currentFrame)
        {
            currentFrame = frame;
            Renderer.sprite = activeFrames[frame];
        }
    }

    public override void Cancel()
    {
        if (IsRunning && Renderer != null)
            Renderer.sprite = spriteBeforeDuck;

        duckTimer = 0f;
        currentFrame = 0;
        activeFrames = null;
        IsRunning = false;
    }
}
