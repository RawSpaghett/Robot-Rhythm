using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;

public class MaxScoreCalculator
{

   [MenuItem("Tools/Score/Get Max Score")]
    public static void GetMaxScore()
    {
        float tempScore = 0;
        float tempMulti = ComboTracker.multiplierBase;
        float tempObstacles = 0;
        int obstacles = Object.FindFirstObjectByType<RhythmManager>().beatMap.Obstacles.Count;
        while(ComboTracker.maxMultiplier > tempMulti)
        {
            tempMulti += ComboTracker.multiplierStep;
            tempScore += ScoreManager.PointsPerPerfectHazard * tempMulti;
            tempObstacles++;
        }
        tempScore += ScoreManager.PointsPerPerfectHazard * (obstacles - tempObstacles) * tempMulti;
        Debug.Log($"A perfect score including multi is {tempScore.ToString()}");
    }
}
