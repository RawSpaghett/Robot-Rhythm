using UnityEngine;

// Add once for the PLAYER. NPC ActionManagers do not subscribe to player results.
// Owns input-to-obstacle forwarding and chooses the presentation, not score rules.
public class PlayerTimingFeedback : MonoBehaviour
{
    [SerializeField] private ActionManager actionManager;
    [SerializeField] private ObstacleSpawner obstacleSpawner;
    [SerializeField] private RhythmManager rhythmManager;

    private ObstacleBase evaluatingObstacle;
    private RobotTimingVisual evaluatedVisual;

    private void OnEnable() { ObstacleBase.OnObstacleResolved += OnObstacleResolved; }
    private void OnDisable()
    {
        ObstacleBase.OnObstacleResolved -= OnObstacleResolved;
        if (actionManager != null) actionManager.ClearBufferedAction();
        evaluatingObstacle = null;
    }

    public void Perform(RobotActionType action, float charge = 0f)
    {
        if (!isActiveAndEnabled || actionManager == null || !actionManager.isActiveAndEnabled ||
            Time.timeScale <= 0f || AudioListener.pause) return;

        // Busy input must be accepted by the buffer before it can judge anything.
        // Perform is called again when the robot is free, using fresh SongTime.
        if (actionManager.IsBusy)
        {
            actionManager.TryBufferAction(action, charge, Perform);
            return;
        }

        RobotTimingVisual visual = RobotTimingVisual.Default;
        RhythmManager rhythm = rhythmManager != null ? rhythmManager : RhythmManager.Instance;
        if (obstacleSpawner != null && rhythm != null && rhythm.isPlaying &&
            TryMapInput(action, out ObstacleInputType input))
        {
            double inputTime = rhythm.SongTime;
            ObstacleBase target = obstacleSpawner.FindInputTarget(inputTime);
            if (target != null)
            {
                evaluatingObstacle = target;
                evaluatedVisual = RobotTimingVisual.Default;
                try
                {
                    // Synchronous result event supplies the visual before the action starts.
                    target.EvaluateInput(input, inputTime, charge);
                    visual = evaluatedVisual;
                }
                finally { evaluatingObstacle = null; }
            }
        }

        // Immediate and buffered actions are both judged just before playback.
        // A buffered request has not resolved an obstacle before reaching here.
        actionManager.TryPerformWithFeedback(action, charge, visual);
    }

    private void OnObstacleResolved(ObstacleBase obstacle, ObstacleResult result, float accuracy)
    {
        if (obstacle == null) return;
        if (obstacle == evaluatingObstacle)
        {
            evaluatedVisual = result == ObstacleResult.Success
                ? (obstacle.WasPerfect ? RobotTimingVisual.Perfect : RobotTimingVisual.EarlyLate)
                : RobotTimingVisual.Failed;
            return;
        }

        // Only no-input misses from this level spawner affect this player.
        if (result != ObstacleResult.Missed || actionManager == null || obstacleSpawner == null ||
            !obstacle.transform.IsChildOf(obstacleSpawner.transform)) return;

        switch (obstacle.RequiredInput)
        {
            case ObstacleInputType.Jump: actionManager.ShowMissedObstacle(RobotActionType.Jump); break;
            case ObstacleInputType.LongJump: actionManager.ShowMissedObstacle(RobotActionType.LongJump); break;
            case ObstacleInputType.Dodge: actionManager.ShowMissedObstacle(RobotActionType.Duck); break;
            case ObstacleInputType.Brake: actionManager.ShowMissedObstacle(RobotActionType.Brake); break;
            case ObstacleInputType.Accelerate: actionManager.ShowMissedObstacle(RobotActionType.Accelerate); break;
        }
    }

    private static bool TryMapInput(RobotActionType action, out ObstacleInputType input)
    {
        switch (action)
        {
            case RobotActionType.Jump: input = ObstacleInputType.Jump; return true;
            case RobotActionType.LongJump: input = ObstacleInputType.LongJump; return true;
            case RobotActionType.Duck: input = ObstacleInputType.Dodge; return true;
            case RobotActionType.Brake: input = ObstacleInputType.Brake; return true;
            case RobotActionType.Accelerate: input = ObstacleInputType.Accelerate; return true;
            default: input = default(ObstacleInputType); return false;
        }
    }
}
