using UnityEngine;

public class RobotJump : MonoBehaviour
{
    [Header("Normal Jump Settings")]
    [SerializeField, Min(0f)] private float jumpHeight = 2f;
    [SerializeField, Min(0.01f)] private float jumpDuration = 0.5f;

    [Header("Long Jump Height")]
    [SerializeField, Min(0f)] private float minimumLongJumpHeight = 2.5f;
    [SerializeField, Min(0f)] private float maximumLongJumpHeight = 3.5f;

    [Header("Long Jump Duration")]
    [SerializeField, Min(0.01f)]
    private float minimumLongJumpDuration = 0.8f;

    [SerializeField, Min(0.01f)]
    private float maximumLongJumpDuration = 1.2f;

    private Vector3 restingLocalPosition;

    private float jumpTimer;
    private float activeJumpHeight;
    private float activeJumpDuration;

    private bool isJumping;

    private void Awake()
    {
        restingLocalPosition = transform.localPosition;
    }

    public void TryJump()
    {
        BeginJump(jumpHeight, jumpDuration);
    }

    public void TryLongJump(float charge)
    {
        charge = Mathf.Clamp01(charge);

        float height = Mathf.Lerp(
            minimumLongJumpHeight,
            Mathf.Max(minimumLongJumpHeight, maximumLongJumpHeight),
            charge);

        float duration = Mathf.Lerp(
            minimumLongJumpDuration,
            Mathf.Max(minimumLongJumpDuration, maximumLongJumpDuration),
            charge);

        BeginJump(height, duration);
    }

    private void BeginJump(float height, float duration)
    {
        // Both jump types obey the same airborne and pause restrictions.
        if (!isActiveAndEnabled || isJumping || Time.timeScale <= 0f)
            return;

        activeJumpHeight = Mathf.Max(0f, height);
        activeJumpDuration = Mathf.Max(0.01f, duration);

        jumpTimer = 0f;
        isJumping = true;
    }

    private void Update()
    {
        if (!isJumping)
            return;

        jumpTimer += Time.deltaTime;

        float progress = Mathf.Clamp01(
            jumpTimer / activeJumpDuration);

        float verticalOffset =
            4f * activeJumpHeight * progress * (1f - progress);

        transform.localPosition =
            restingLocalPosition + Vector3.up * verticalOffset;

        if (progress >= 1f)
        {
            transform.localPosition = restingLocalPosition;
            isJumping = false;
        }
    }

    private void OnDisable()
    {
        transform.localPosition = restingLocalPosition;

        jumpTimer = 0f;
        isJumping = false;
    }
}