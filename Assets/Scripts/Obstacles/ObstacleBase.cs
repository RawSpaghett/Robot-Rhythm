using UnityEngine;
using UnityEngine.Events;

// Gameplay actions, rather than the inputs used to request them.
public enum ObstacleInputType { Jump, LongJump, Brake, Accelerate, Dodge }
public enum ObstacleResult { None, Success, TooEarly, TooLate, WrongInput, Missed }

public abstract class ObstacleBase : MonoBehaviour
{
    [Header("Timing Windows (Seconds)")]
    [SerializeField, Min(0.01f)] private float earlyWindow = 0.25f;
    [SerializeField, Min(0.01f)] private float lateWindow = 0.25f;

    [Header("Rating")]
    [SerializeField, Range(0.01f, 1f)] private float perfectAccuracyThreshold = 0.90f;

    // Unity Action event for sending Obstacle Accuracy & Obstacle Result
    public static event UnityAction<ObstacleBase, ObstacleResult, float> OnObstacleResolved;

    public static event UnityAction<ObstacleInputType,float> OnPerfectTiming; //for robot team

    public double TargetTime { get; private set; }
    public float EarlyWindow => Mathf.Max(0.01f, earlyWindow);
    public float LateWindow => Mathf.Max(0.01f, lateWindow);
    public bool IsActive { get; private set; }
    public bool IsResolved { get; private set; }
    public float LastAccuracy { get; private set; }
    public ObstacleResult LastResult { get; private set; } = ObstacleResult.None;
    public abstract ObstacleInputType RequiredInput { get; }
    

    // A correct input exactly on the window boundary succeeds with 0 accuracy.
    // Use these properties / LastResult when you need success versus failure.
    public bool WasCleared => IsResolved && LastResult == ObstacleResult.Success;
    public bool WasPerfect => WasCleared && LastAccuracy >= perfectAccuracyThreshold;

    public void Prepare(double targetTime)
    {
        // ObstacleSpawner already supplies placement.Beat * SecondsPerBeat.
        // These are seconds relative to beat zero, matching RhythmManager.SongTime.
        // Do not add FirstBeatOffset here: SongTime already subtracts it.
        TargetTime = targetTime;
        IsActive = true;
        IsResolved = false;
        LastAccuracy = 0f;
        LastResult = ObstacleResult.None;
        
    }

    public void EvaluateInput(ObstacleInputType inputType, double inputTime, float charge = 0f)
    {
        // The input/controller forwards one confirmed input to the target chosen
        // by ObstacleSpawner.FindInputTarget. inputTime must be SongTime.
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
        if (!AcceptsInput(inputType, Mathf.Clamp01(charge)))
        {
            Resolve(ObstacleResult.WrongInput, 0f);
            return;
        }

        Resolve(ObstacleResult.Success, CalculateAccuracy(inputTime));
    }

    public void CheckForMiss(double currentTime)
    {
        // The existing spawner calls this every update using SongTime.
        // No separate Update or RhythmManager reference is needed in this class.
        if (!isActiveAndEnabled || !IsActive || IsResolved)
        {
            OnPerfectTiming?.Invoke(RequiredInput, 1.0f);
            return;
        }

        if (currentTime > TargetTime + LateWindow)
            Resolve(ObstacleResult.Missed, 0f);
    }

    public float CalculateAccuracy(double inputTime)
    {
        double difference = inputTime - TargetTime;
        double window = difference <= 0.0 ? EarlyWindow : LateWindow;
        return Mathf.Clamp01((float)(1.0 - System.Math.Abs(difference) / window));
    }

    public void CancelAttempt()
    {
        // Cancellation does not award points or report a miss.
        IsActive = false;
        IsResolved = false;
        LastAccuracy = 0f;
        LastResult = ObstacleResult.None;
    }

    protected abstract bool AcceptsInput(ObstacleInputType inputType, float charge);

    private void Resolve(ObstacleResult result, float accuracy)
    {
        // Store and lock the result before notifying listeners.
        IsActive = false;
        IsResolved = true;
        LastResult = result;
        LastAccuracy = Mathf.Clamp01(accuracy);
        OnObstacleResolved?.Invoke(this, LastResult, LastAccuracy);
    }

    protected virtual void OnDisable()
    {
        CancelAttempt();
    }
}
