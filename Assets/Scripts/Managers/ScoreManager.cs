using UnityEngine;

//receives actions from obstacles that are saved here
//Jadon

public class ScoreManager: MonoBehaviour
{
    public const float PointsPerPerfectHazard = 100f;
    public float score{get; private set;}
    public int ResolvedObstacles { get; private set; }
    public float AverageAccuracy => ResolvedObstacles > 0 ? Mathf.Clamp01(totalAccuracy / ResolvedObstacles) : 0f;
    private float totalAccuracy;
    private ComboTracker comboTracker;
    
    void Awake()
    {
        ResetScore();
    }

    void OnEnable()
    {
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            comboTracker = root.GetComponentInChildren<ComboTracker>();
            if (comboTracker != null)
            {
                comboTracker.OnScoreReady += CompileScore;
                break;
            }
        }
        ObstacleBase.OnObstacleResolved += OnObstacleResolved;
    }

    void OnDisable()
    {
        if (comboTracker != null)
        {
            comboTracker.OnScoreReady -= CompileScore;
        }
        comboTracker = null;
        ObstacleBase.OnObstacleResolved -= OnObstacleResolved;
    }

    private void OnObstacleResolved(ObstacleBase obstacle, ObstacleResult result, float accuracy)
    {
        if (obstacle == null || obstacle.gameObject.scene != gameObject.scene || result == ObstacleResult.None)
            return;

        // ComboTracker.cs sends accuracy and its updated multiplier to CompileScore() below.
        // Only score directly from ObstacleBase.cs when this level has no active combo tracker.
        if (comboTracker != null && comboTracker.isActiveAndEnabled)
        {
            return;
        }
        CompileScore(accuracy, 1f);
    }

    private void CompileScore(float accuracy, float multiplier)
    {
        score += accuracy * PointsPerPerfectHazard * multiplier;
        // Keep bonus points out of accuracy so they do not inflate the star rating.
        totalAccuracy += accuracy;
        ResolvedObstacles++;
    }

    private void ResetScore()
    {
        score = 0;
        totalAccuracy = 0;
        ResolvedObstacles = 0;
    }
}
