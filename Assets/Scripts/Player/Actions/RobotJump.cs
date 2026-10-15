using System;
using UnityEngine;

// Serialized inside ActionManager. Do not attach this class to a GameObject.
[Serializable]
public class RobotJump : ActionBase
{
    [Header("Normal Jump Settings")]
    [SerializeField, Min(0f)] private float jumpHeight = 2f;
    [SerializeField, Min(0.01f)] private float jumpDuration = 0.5f;

    [Header("Normal Jump Animation")]
    [Tooltip("Optional sprites in order: takeoff, airborne, landing.")]
    [SerializeField] private Sprite[] jumpFrames = new Sprite[0];

    [Header("Long Jump Animation")]
    [Tooltip("Optional charged-jump sprites in playback order. Plays during the jump, not while charging.")]
    [SerializeField] private Sprite[] longJumpFrames = new Sprite[0];

    [Header("Long Jump Height")]
    [SerializeField, Min(0f)] private float minimumLongJumpHeight = 2.5f;
    [SerializeField, Min(0f)] private float maximumLongJumpHeight = 3.5f;

    [Header("Long Jump Duration")]
    [SerializeField, Min(0.01f)] private float minimumLongJumpDuration = 0.8f;
    [SerializeField, Min(0.01f)] private float maximumLongJumpDuration = 1.2f;

    [NonSerialized] private Vector3 restingLocalPosition;
    [NonSerialized] private float jumpTimer;
    [NonSerialized] private float activeJumpHeight;
    [NonSerialized] private float activeJumpDuration;

    [NonSerialized] private Sprite spriteBeforeJump;
    [NonSerialized] private Sprite[] activeFrames;
    [NonSerialized] private int currentFrame;
    [NonSerialized] private bool playingSpriteAnimation;

    public override bool TryBegin(RobotActionType type, float charge)
    {
        if (IsRunning || Visual == null ||
            (type != RobotActionType.Jump && type != RobotActionType.LongJump))
            return false;

        if (type == RobotActionType.LongJump)
        {
            charge = Mathf.Clamp01(charge);
            activeJumpHeight = Mathf.Lerp(
                minimumLongJumpHeight,
                Mathf.Max(minimumLongJumpHeight, maximumLongJumpHeight),
                charge);
            activeJumpDuration = Mathf.Lerp(
                minimumLongJumpDuration,
                Mathf.Max(minimumLongJumpDuration, maximumLongJumpDuration),
                charge);
        }
        else
        {
            activeJumpHeight = jumpHeight;
            activeJumpDuration = jumpDuration;
        }

        activeJumpHeight = Mathf.Max(0f, activeJumpHeight);
        activeJumpDuration = Mathf.Max(0.01f, activeJumpDuration);

        // Capture each attempt's starting position so relocation between jumps
        // does not send the robot back to its original scene position.
        restingLocalPosition = Visual.localPosition;
        jumpTimer = 0f;
        IsRunning = true;

        // Each jump type has its own sequence. Charge determines long-jump
        // duration, so its frames automatically stretch across that duration.
        BeginSpriteAnimation(type == RobotActionType.LongJump
            ? longJumpFrames : jumpFrames);
        FXManager.Instance.PlaySFX("jump");
        return true;
    }

    public override void Tick(float deltaTime)
    {
        if (!IsRunning) return;
        if (Visual == null) { Cancel(); return; }

        jumpTimer += Mathf.Max(0f, deltaTime);
        float progress = Mathf.Clamp01(jumpTimer / activeJumpDuration);
        float verticalOffset =
            4f * activeJumpHeight * progress * (1f - progress);

        Visual.localPosition =
            restingLocalPosition + Vector3.up * verticalOffset;

        if (progress >= 1f)
        {
            Cancel();
            return;
        }

        UpdateSpriteAnimation(progress);
    }

    public override void Cancel()
    {
        if (IsRunning && Visual != null)
            Visual.localPosition = restingLocalPosition;

        // Restore only if this attempt actually changed the sprite.
        if (playingSpriteAnimation && Renderer != null)
            Renderer.sprite = spriteBeforeJump;

        activeFrames = null;
        spriteBeforeJump = null;
        currentFrame = 0;
        playingSpriteAnimation = false;
        jumpTimer = 0f;
        IsRunning = false;
    }

    private void BeginSpriteAnimation(Sprite[] frames)
    {
        activeFrames = null;
        playingSpriteAnimation = false;
        currentFrame = 0;

        // Animation is optional so jumping still works before art is ready.
        if (frames == null || frames.Length == 0)
            return;

        if (Renderer == null)
        {
            Debug.LogWarning(
                "RobotJump: assign Robot Renderer on ActionManager to play sprites.",
                LogContext);
            return;
        }

        for (int i = 0; i < frames.Length; i++)
        {
            if (frames[i] == null)
            {
                Debug.LogWarning(
                    $"RobotJump: animation frame {i} is unassigned. Playing movement only.",
                    LogContext);
                return;
            }
        }

        // Freeze this attempt's sequence against Inspector edits during play.
        activeFrames = (Sprite[])frames.Clone();
        spriteBeforeJump = Renderer.sprite;
        playingSpriteAnimation = true;
        Renderer.sprite = activeFrames[0];
    }

    private void UpdateSpriteAnimation(float progress)
    {
        if (!playingSpriteAnimation || Renderer == null)
            return;

        // Equal time per frame, using the same progress as the movement arc.
        // Low frame rates may skip frames rather than prolong the jump.
        int frame = Mathf.Min(
            Mathf.FloorToInt(progress * activeFrames.Length),
            activeFrames.Length - 1);

        if (frame != currentFrame)
        {
            currentFrame = frame;
            Renderer.sprite = activeFrames[frame];
        }
    }

    // ActionManager still starts, ticks, and cancels this action.
    // Pausing the manager pauses both motion and sprite playback.
    // Keep other sprite animation systems from writing this renderer during
    // the jump. This class restores the pre-jump sprite when it ends.
    // No charge-up animation or rhythm judgment is performed here.
}
