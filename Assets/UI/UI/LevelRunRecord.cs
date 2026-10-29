using UnityEngine;

// Observes the finished run. It does not award points or change the combo.
public class LevelRunRecord : MonoBehaviour
{
    public string levelId;
    private int observed;
    private bool everyObstaclePerfect = true;
    private LevelRecords.UpdateResult savedResult;

    void OnEnable() { ObstacleBase.OnObstacleResolved += Observe; }
    void OnDisable() { ObstacleBase.OnObstacleResolved -= Observe; }

    void Observe(ObstacleBase obstacle, ObstacleResult result, float accuracy)
    {
        if (obstacle == null || obstacle.gameObject.scene != gameObject.scene || result == ObstacleResult.None) return;
        observed++;
        if (result != ObstacleResult.Success || accuracy < 1f) everyObstaclePerfect = false;
    }

    public LevelRecords.UpdateResult Finish(ScoreManager score)
    {
        if (savedResult != null) return savedResult;
        if (string.IsNullOrEmpty(levelId)) return null;
        RhythmManager rhythm = null;
        ComboTracker combo = null;
        foreach (var root in gameObject.scene.GetRootGameObjects())
        {
            if (rhythm == null) rhythm = root.GetComponentInChildren<RhythmManager>();
            if (combo == null) combo = root.GetComponentInChildren<ComboTracker>();
        }
        int total = LevelRecords.CountScoredObstacles(rhythm != null ? rhythm.beatMap : null);
        savedResult = LevelRecords.SaveRun(levelId, score.score, score.AverageAccuracy,
            score.ResolvedObstacles, total, everyObstaclePerfect && observed == total,
            combo != null && combo.isActiveAndEnabled);
        return savedResult;
    }
}
