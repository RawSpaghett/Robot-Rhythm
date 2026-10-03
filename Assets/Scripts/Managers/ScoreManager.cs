using UnityEngine;

//receives actions from obstacles that are saved here
//Jadon

public class ScoreManager: MonoBehaviour
{
    public const float PointsPerPerfectHazard = 100f;
    public float score{get; private set;}
    public int ResolvedObstacles { get; private set; }
    public float AverageAccuracy => ResolvedObstacles > 0 ? Mathf.Clamp01(score / (ResolvedObstacles * PointsPerPerfectHazard)) : 0f;
    
    void Awake()
    {
        ResetScore();
    }

    void OnEnable()
    {
        ObstacleBase.OnObstacleResolved += CompileScore;
    }

    void OnDisable()
    {
        ObstacleBase.OnObstacleResolved -= CompileScore;
    }

    private void CompileScore(float addedScore)
    {
        score += addedScore * PointsPerPerfectHazard;
        ResolvedObstacles++;
    }

    private void ResetScore()
    {
        score = 0;
        ResolvedObstacles = 0;
    }
}
