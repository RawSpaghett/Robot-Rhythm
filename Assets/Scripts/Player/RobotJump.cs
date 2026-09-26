using UnityEngine;

public class RobotJump : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField, Min(0f)] private float jumpHeight = 2f;
    [SerializeField, Min(0.01f)] private float jumpDuration = 0.5f;

    private Vector3 restingLocalPosition;
    private float jumpTimer;
    private bool isJumping;

    private void Awake()
    {
        // Remember the visual's position relative to its parent.
        restingLocalPosition = transform.localPosition;
    }

    public void TryJump()
    {
        // Prevent additional jumps while airborne or paused.
        if (!isActiveAndEnabled || isJumping || Time.timeScale <= 0f)
            return;

        jumpTimer = 0f;
        isJumping = true;
    }

    private void Update()
    {
        if (!isJumping)
            return;

        jumpTimer += Time.deltaTime;

        // Convert elapsed time into progress between 0 and 1.
        float duration = Mathf.Max(0.01f, jumpDuration);
        float progress = Mathf.Clamp01(jumpTimer / duration);

        // A simple arc: zero at the start/end, maximum halfway through.
        float verticalOffset =
            4f * jumpHeight * progress * (1f - progress);

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
        // Reset cleanly if the visual is disabled during a jump.
        transform.localPosition = restingLocalPosition;
        jumpTimer = 0f;
        isJumping = false;
    }
}