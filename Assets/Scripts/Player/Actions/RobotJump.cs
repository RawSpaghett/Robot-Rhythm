using System;
using UnityEngine;

// Serialized inside ActionManager. Do not attach this class to a GameObject.
[Serializable]
public class RobotJump : ActionBase
{
    [Header("Normal Jump Settings")]
    [SerializeField, Min(0f)] private float jumpHeight = 2f;
    [SerializeField, Min(0.01f)] private float jumpDuration = 0.5f;

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
            Cancel();
    }

    public override void Cancel()
    {
        if (IsRunning && Visual != null)
            Visual.localPosition = restingLocalPosition;

        jumpTimer = 0f;
        IsRunning = false;
    }
}
