using UnityEngine;

public class PlayerActionController : MonoBehaviour
{
    [Header("Level")]
    [SerializeField] private ObstacleSpawner obstacleSpawner;
    [SerializeField] private RhythmManager rhythmManager;

    public void RegisterJumpAttempt() => SubmitInput(ObstacleInputType.Jump);
    public void RegisterLongJumpAttempt(float charge) => SubmitInput(ObstacleInputType.LongJump, charge);
    public void RegisterBrakeAttempt() => SubmitInput(ObstacleInputType.Brake);
    public void RegisterAccelerateAttempt() => SubmitInput(ObstacleInputType.Accelerate);
    public void RegisterDuckAttempt() => SubmitInput(ObstacleInputType.Dodge);

    private void SubmitInput(ObstacleInputType inputType, float charge = 0f)
    {
        if (!isActiveAndEnabled || Time.timeScale <= 0f || AudioListener.pause ||
            rhythmManager == null || !rhythmManager.isPlaying || obstacleSpawner == null)
            return;

        // Capture the music time on release, before playing any feedback.
        double inputTime = rhythmManager.SongTime;
        ObstacleBase obstacle = obstacleSpawner.FindInputTarget(inputTime);
        if (obstacle != null)
            obstacle.EvaluateInput(inputType, inputTime, charge);
    }
}
