using UnityEngine;
using UnityEngine.Events;

// Describes the gameplay action, rather than the physical gesture.
public enum ObstacleInputType
{
    Jump,
    LongJump,
    Brake,
    Accelerate,
    Dodge
}

public enum ObstacleResult
{
    None,
    Success,
    TooEarly,
    TooLate,
    WrongInput,
    Missed
}

public abstract class ObstacleBase : MonoBehaviour
{
    [Header("Timing Windows")]
    [SerializeField, Min(0.01f)] private float earlyWindow = 0.25f;
    [SerializeField, Min(0.01f)] private float lateWindow = 0.25f;

    [Header("Rating")]
    [SerializeField, Range(0.01f, 1f)]
    private float perfectAccuracyThreshold = 0.90f;

    // Shared result event for every obstacle derived from ObstacleBase.
    // Arguments: obstacle that resolved, outcome, timing accuracy.
    
    // Listeners subscribe with += and unsubscribe with -=.
    // Only ObstacleBase can invoke this event.
    public static event UnityAction<float>
        OnObstacleResolved;

    public double TargetTime { get; private set; }

    public float EarlyWindow => Mathf.Max(0.01f, earlyWindow);
    public float LateWindow => Mathf.Max(0.01f, lateWindow);

    public bool IsActive { get; private set; }
    public bool IsResolved { get; private set; }

    public float LastAccuracy { get; private set; }

    public ObstacleResult LastResult { get; private set; } =
        ObstacleResult.None;

    // Clearance is separate from accuracy because a press exactly
    // on the window boundary is accepted but has zero accuracy.
    public bool WasCleared =>
        IsResolved && LastResult == ObstacleResult.Success;

    public bool WasPerfect =>
        WasCleared && LastAccuracy >= perfectAccuracyThreshold;

    public void Prepare(double targetTime)
    {
        // A test controller or rhythm system supplies the target.
        TargetTime = targetTime;

        IsActive = true;
        IsResolved = false;

        LastAccuracy = 0f;
        LastResult = ObstacleResult.None;
    }

    public void EvaluateInput(
        ObstacleInputType inputType,
        double inputTime,
        float charge = 0f)
    {
        if (!isActiveAndEnabled || !IsActive || IsResolved)
            return;

        if (inputTime < TargetTime - EarlyWindow)
        {
            Resolve(ObstacleResult.TooEarly, 0f);
            return;
        }

        if (inputTime > TargetTime + LateWindow)
        {
            Resolve(ObstacleResult.TooLate, 0f);
            return;
        }

        // Each child obstacle defines its accepted actions.
        if (!AcceptsInput(inputType, Mathf.Clamp01(charge)))
        {
            Resolve(ObstacleResult.WrongInput, 0f);
            return;
        }

        float accuracy = CalculateAccuracy(inputTime);

        Resolve(ObstacleResult.Success, accuracy);
    }

    public void CheckForMiss(double currentTime)
    {
        if (!isActiveAndEnabled || !IsActive || IsResolved)
            return;

        // The obstacle must resolve even if no input occurs.
        if (currentTime > TargetTime + LateWindow)
        {
            Resolve(ObstacleResult.Missed, 0f);
        }
    }

    public float CalculateAccuracy(double inputTime)
    {
        double timingDifference = inputTime - TargetTime;

        double window = timingDifference <= 0.0
            ? EarlyWindow
            : LateWindow;

        double distanceFromTarget =
            System.Math.Abs(timingDifference);

        float accuracy =
            (float)(1.0 - distanceFromTarget / window);

        return Mathf.Clamp01(accuracy);
    }

    public void CancelAttempt()
    {
        // Cancellation does not award points or report a miss.
        IsActive = false;
        IsResolved = false;

        LastAccuracy = 0f;
        LastResult = ObstacleResult.None;
    }

    // Every obstacle subclass must provide its own input rules.
    protected abstract bool AcceptsInput(
        ObstacleInputType inputType,
        float charge);

    private void Resolve(ObstacleResult result, float accuracy)
    {
        // Lock the outcome before notifying other systems.
        IsActive = false;
        IsResolved = true;

        LastResult = result;
        LastAccuracy = Mathf.Clamp01(accuracy);

        // Notify listeners after the obstacle's result has been stored.
        // ?.Invoke safely handles having no listeners.
        OnObstacleResolved?.Invoke(LastAccuracy);
    }

    protected virtual void OnDisable()
    {
        CancelAttempt();
    }
}