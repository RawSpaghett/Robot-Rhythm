#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class MaxScoreCalculator
{

   [MenuItem("Tools/Score/Get Max Score")]
    public static void GetMaxScore()
    {
        var rhythm = Object.FindFirstObjectByType<RhythmManager>();
        if (rhythm == null || rhythm.beatMap == null)
        {
            Debug.LogWarning("Open a level with a RhythmManager and beat map first.");
            return;
        }
        // Use the same capped multiplier ramp as gameplay and the perfect-run record check.
        float tempScore = LevelRecords.MaximumScore(rhythm.beatMap.Obstacles.Count);
        Debug.Log($"A perfect score including multi is {tempScore.ToString()}");
    }
}
#endif
