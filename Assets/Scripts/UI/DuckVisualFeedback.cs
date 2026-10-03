using System.Collections;
using UnityEngine;

// Temporary duck presentation until the real animation is available.
// Attach to the robot and assign a separate visual child, not its gameplay root.
[DisallowMultipleComponent]
public class DuckVisualFeedback : MonoBehaviour
{
    [Header("Visual Target")]
    [SerializeField] private Transform robotVisual;

    [Header("Placeholder Duck")]
    [SerializeField, Min(0.01f)] private float duckDuration = 0.35f;
    [SerializeField, Range(0.1f, 1f)] private float duckHeightMultiplier = 0.5f;
    [Tooltip("Optional local position adjustment while ducking. Tune for your sprite's pivot.")]
    [SerializeField] private Vector3 duckLocalOffset = Vector3.zero;

    private Vector3 standingScale;
    private Vector3 standingPosition;
    private Transform activeVisual;
    private Coroutine duckRoutine;
    private bool isDucking;

    // INSPECTOR CONNECTION:
    // Add this component to DuckAction's On Duck Requested event.
    // Select DuckVisualFeedback.PlayDuck().
    // Keep ObstacleTimingTest.RegisterDuckAttempt() as a separate listener.
    public void PlayDuck()
    {
        if (!isActiveAndEnabled || Time.timeScale <= 0f || robotVisual == null)
            return;

        // Restart from the original pose, never from an already squashed scale.
        StopDuck();

        activeVisual = robotVisual;
        standingScale = activeVisual.localScale;
        standingPosition = activeVisual.localPosition;
        isDucking = true;

        activeVisual.localScale = new Vector3(
            standingScale.x,
            standingScale.y * duckHeightMultiplier,
            standingScale.z);
        activeVisual.localPosition = standingPosition + duckLocalOffset;

        duckRoutine = StartCoroutine(RestoreAfterDuck());
    }

    private IEnumerator RestoreAfterDuck()
    {
        // Scaled time makes the visual pause when the game is paused.
        yield return new WaitForSeconds(Mathf.Max(0.01f, duckDuration));
        RestorePose();
        duckRoutine = null;
    }

    private void StopDuck()
    {
        if (duckRoutine != null)
        {
            StopCoroutine(duckRoutine);
            duckRoutine = null;
        }

        RestorePose();
    }

    private void RestorePose()
    {
        if (isDucking && activeVisual != null)
        {
            activeVisual.localScale = standingScale;
            activeVisual.localPosition = standingPosition;
        }

        isDucking = false;
        activeVisual = null;
    }

    private void OnDisable()
    {
        StopDuck();
    }

    // FUTURE ANIMATION REPLACEMENT:
    // Replace this placeholder with the real animation controller's duck method
    // in DuckAction's event, and remove the PlayDuck listener to avoid double
    // visual effects. Disable this component to restore any active duck pose.
    // Do not have this placeholder and an Animator both control the same
    // transform's scale/position.
    //
    // TIMING AND COLLISION:
    // This visual responds to an attempt even when that attempt is off-beat.
    // It does not evaluate accuracy, grant invulnerability, or resize colliders.
    // Assign a visual child with no gameplay colliders beneath it.
    // DuckObstacle / ObstacleBase remain responsible for the timing judgment.
}
