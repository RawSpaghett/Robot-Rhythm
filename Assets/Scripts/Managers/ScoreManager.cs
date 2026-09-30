using UnityEngine;

//receives actions from obstacles that are saved here
//Jadon

public class ScoreManager: MonoBehaviour
{
    public float score{get; private set;}
    
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
        score += addedScore;
    }

    private void ResetScore()
    {
        score = 0;
    }
}
