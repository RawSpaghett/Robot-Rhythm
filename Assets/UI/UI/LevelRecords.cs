using System;
using UnityEngine;

// Local records are keyed by the level's scene GUID, not its position in the carousel.
public static class LevelRecords
{
    [Serializable]
    public class Record
    {
        public int version = 1;
        public bool completed;
        public int bestScore;
        public float bestAccuracy;
        public bool perfect;
    }

    public class UpdateResult
    {
        public Record record;
        public bool saved, firstClear, newScore, newAccuracy, perfectRun, firstPerfect;
        public string error;
    }

    public const string KeyPrefix = "rr.level-record.v1.";

    public static float MaximumScore(int obstacles, bool hasCombo = true)
    {
        float score = 0;
        float multiplier = ComboTracker.multiplierBase;
        for (int i = 0; i < obstacles; i++)
        {
            if (hasCombo) multiplier = Mathf.Min(ComboTracker.maxMultiplier, multiplier + ComboTracker.multiplierStep);
            score += ScoreManager.PointsPerPerfectHazard * (hasCombo ? multiplier : 1f);
        }
        return score;
    }

    private static Record Read(string key)
    {
        if (!PlayerPrefs.HasKey(key)) return null;
        try
        {
            var record = JsonUtility.FromJson<Record>(PlayerPrefs.GetString(key));
            if (record == null || record.version != 1 || record.bestScore < 0 ||
                float.IsNaN(record.bestAccuracy) || record.bestAccuracy < 0 || record.bestAccuracy > 1)
                return null;
            return record;
        }
        catch (Exception) { return null; }
    }

    public static Record Load(string levelId, string prefix = KeyPrefix)
    {
        if (string.IsNullOrWhiteSpace(levelId)) return new Record();
        return Read(prefix + levelId) ?? Read(prefix + levelId + ".backup") ?? new Record();
    }

    public static UpdateResult SaveRun(string levelId, float score, float accuracy, int resolved,
        int total, bool everyObstaclePerfect, bool hasCombo, string prefix = KeyPrefix)
    {
        var old = Load(levelId, prefix);
        var result = new UpdateResult { record = old };
        // A quit, an empty chart, or an unfinished run cannot set a completion record.
        if (string.IsNullOrWhiteSpace(levelId) || total <= 0 || resolved != total ||
            float.IsNaN(score) || float.IsInfinity(score) || score < 0 ||
            float.IsNaN(accuracy) || float.IsInfinity(accuracy) || accuracy < 0 || accuracy > 1)
            return result;

        int points = Mathf.RoundToInt(score);
        result.firstClear = !old.completed;
        // The first completed run sets a baseline. Only a later run can beat it.
        result.newScore = old.completed && points > old.bestScore;
        result.newAccuracy = old.completed && accuracy > old.bestAccuracy;
        result.perfectRun = everyObstaclePerfect && accuracy == 1f &&
            Mathf.Abs(score - MaximumScore(total, hasCombo)) < .01f;
        result.firstPerfect = result.perfectRun && !old.perfect;
        var next = new Record {
            completed = true,
            bestScore = Mathf.Max(old.bestScore, points),
            bestAccuracy = Mathf.Max(old.bestAccuracy, accuracy),
            perfect = old.perfect || result.perfectRun
        };
        string key = prefix + levelId;
        try
        {
            PlayerPrefs.SetString(key + ".backup", JsonUtility.ToJson(old));
            PlayerPrefs.SetString(key, JsonUtility.ToJson(next));
            PlayerPrefs.Save(); // Flush before leaving results or closing the application.
            result.record = next;
            result.saved = true;
        }
        catch (Exception exception)
        {
            result.error = "Record could not be saved.";
            Debug.LogWarning(result.error + " " + exception.Message);
        }
        return result;
    }
}
