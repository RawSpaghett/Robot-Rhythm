using System.Globalization;
using RobotRhythm.UI;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    [Header("Score Text")]
    [SerializeField] private Text liveScore;
    [SerializeField] private Text finalScore;
    [SerializeField] private UiController preferences;
    [SerializeField] private RollingNumber scoreRoll;
    [SerializeField] private RollingNumber accuracyRoll;

    [Header("Delivery Rating")]
    [SerializeField] private UiTheme theme;
    [SerializeField] private RatingStar[] liveStars;
    [SerializeField] private RatingStar[] resultStars;
    [SerializeField] private Text accuracyText;
    [SerializeField] private Text deliveryReview;
    [SerializeField] private float[] starThresholds = { 0.5f, 0.65f, 0.8f, 0.9f, 0.98f };

    [SerializeField] private float[] halfStarThresholds = { .25f, .575f, .725f, .85f, .94f };

    [System.Serializable]
    private class AccuracyBand
    {
        [Range(0, 100)] public int minimumPercent;
        public Color color;
    }

    [Header("Number Colors")]
    [Tooltip("Keep these in order from the lowest percentage to the highest.")]
    [SerializeField] private AccuracyBand[] accuracyBands =
    {
        new AccuracyBand { minimumPercent = 0, color = new Color32(255, 45, 45, 255) },
        new AccuracyBand { minimumPercent = 50, color = new Color32(255, 138, 0, 255) },
        new AccuracyBand { minimumPercent = 60, color = new Color32(255, 213, 0, 255) },
        new AccuracyBand { minimumPercent = 70, color = new Color32(69, 214, 90, 255) },
        new AccuracyBand { minimumPercent = 80, color = new Color32(25, 207, 232, 255) },
        new AccuracyBand { minimumPercent = 90, color = new Color32(52, 120, 246, 255) },
        new AccuracyBand { minimumPercent = 95, color = new Color32(168, 85, 247, 255) }
    };

    [System.Serializable]
    private class ScoreBand
    {
        [Min(0)] public int minimumPoints;
        public Color color;
    }

    [Header("Score Colors")]
    [Tooltip("Results score colors use points, independently of accuracy. Keep these in ascending order.")]
    [SerializeField] private ScoreBand[] scoreBands =
    {
        new ScoreBand { minimumPoints = 0, color = new Color32(255, 45, 45, 255) },
        new ScoreBand { minimumPoints = 100, color = new Color32(255, 138, 0, 255) },
        new ScoreBand { minimumPoints = 250, color = new Color32(255, 213, 0, 255) },
        new ScoreBand { minimumPoints = 500, color = new Color32(69, 214, 90, 255) },
        new ScoreBand { minimumPoints = 1000, color = new Color32(25, 207, 232, 255) },
        new ScoreBand { minimumPoints = 1500, color = new Color32(52, 120, 246, 255) },
        new ScoreBand { minimumPoints = 2000, color = new Color32(168, 85, 247, 255) }
    };

    [Header("Animation")]
    [SerializeField, Min(0.01f)] private float resultDuration = 1.6f;

    private ScoreManager source;
    private float lastScore;
    private float resultScore;
    private int lastResolved;
    private float resultRating;
    private int resultAccuracy;
    private bool resultHasAccuracy;
    private float resultTime;
    private bool showingResults;
    private bool animatingResult;

    public void Bind(ScoreManager value)
    {
        if (ReferenceEquals(source, value))
            return;
        source = value;
        lastScore = source != null ? source.score : 0f;
        lastResolved = source != null ? source.ResolvedObstacles : 0;
        showingResults = false;
        animatingResult = false;
        liveScore.text = source != null ? Format(lastScore) : "—";
        finalScore.text = "—";
        if (scoreRoll != null) scoreRoll.Clear();
        if (accuracyRoll != null) accuracyRoll.Clear();
        liveScore.color = theme != null ? theme.ink : Color.white;
        PaintStars(liveStars, GetRating(source != null ? source.AverageAccuracy : 0f, lastResolved));
        PaintStars(resultStars, 0);
        if (accuracyText != null) accuracyText.text = "—";
    }

    public void ShowResults(bool visible)
    {
        if (visible == showingResults)
            return;
        showingResults = visible;
        if (!visible)
        {
            animatingResult = false;
            return;
        }

        if (scoreRoll != null) scoreRoll.Clear();
        if (accuracyRoll != null) accuracyRoll.Clear();
        // Snapshot the total once; later obstacle events cannot alter this result.
        resultScore = source != null ? source.score : 0f;
        float accuracy = source != null ? source.AverageAccuracy : 0f;
        int resolved = source != null ? source.ResolvedObstacles : 0;
        resultRating = GetRating(accuracy, resolved);
        resultAccuracy = Mathf.RoundToInt(accuracy * 100f);
        resultHasAccuracy = resolved > 0;
        if (accuracyText != null)
            accuracyText.text = resolved > 0 ? Mathf.RoundToInt(accuracy * 100f) + "%" : "—";
        if (deliveryReview != null)
            deliveryReview.text = resolved == 0 ? "Ready for another run?" :
                accuracy >= 0.9999f ? "Perfect!" : resultRating >= 4 ? "A great delivery!" :
                resultRating >= 3 ? "Nicely delivered!" : resultRating >= 1 ? "A bumpy delivery." : "Try again!";
        resultTime = 0f;
        animatingResult = source != null;
        bool reducedMotion = preferences != null && preferences.ReducedMotion;
        PaintStars(resultStars, reducedMotion ? resultRating : 0);
        UpdateResultColors();
        finalScore.text = source != null ? Format(reducedMotion ? resultScore : 0f) : "—";
    }

    private void LateUpdate()
    {
        bool reducedMotion = preferences != null && preferences.ReducedMotion;
        if (source != null && !showingResults && (source.score != lastScore || source.ResolvedObstacles != lastResolved))
        {
            lastScore = source.score;
            lastResolved = source.ResolvedObstacles;
            liveScore.text = Format(lastScore);
            liveScore.color = theme != null ? theme.ink : Color.white;
            PaintStars(liveStars, GetRating(source.AverageAccuracy, lastResolved), !reducedMotion);
        }

        if (reducedMotion)
        {
            StopStarMotion(liveStars);
            StopStarMotion(resultStars);
        }
        // Wait for the results panel's entrance to finish before counting up.
        if (!animatingResult || GameManager.Instance == null || GameManager.Instance.IsUITransitioning)
            return;
        resultTime += Time.unscaledDeltaTime;
        float progress = reducedMotion ? 1f : Mathf.Clamp01(resultTime / resultDuration);
        float eased = 1f - Mathf.Pow(1f - progress, 3f);
        finalScore.text = Format(Mathf.Lerp(0f, resultScore, eased));
        if (scoreRoll != null) scoreRoll.SetProgress(Mathf.RoundToInt(resultScore), progress);
        if (accuracyRoll != null && resultHasAccuracy) accuracyRoll.SetProgress(resultAccuracy, progress);
        PaintStars(resultStars, reducedMotion ? resultRating : Mathf.Min(resultRating, Mathf.FloorToInt(progress * 6f)), !reducedMotion);
        if (progress >= 1f)
            animatingResult = false;
    }

    public Color GetAccuracyColor(float accuracy)
    {
        int percent = Mathf.RoundToInt(Mathf.Clamp01(accuracy) * 100f);
        for (int i = accuracyBands.Length - 1; i >= 0; i--)
            if (percent >= accuracyBands[i].minimumPercent)
                return accuracyBands[i].color;
        return theme != null ? theme.ink : Color.white;
    }

    public Color GetScoreColor(float score)
    {
        int points = Mathf.RoundToInt(score);
        for (int i = scoreBands.Length - 1; i >= 0; i--)
            if (points >= scoreBands[i].minimumPoints)
                return scoreBands[i].color;
        return theme != null ? theme.ink : Color.white;
    }

    private void UpdateResultColors()
    {
        finalScore.color = GetScoreColor(resultScore);
        if (accuracyText != null) accuracyText.color = GetAccuracyColor(resultAccuracy / 100f);
    }

    private static string Format(float value)
    {
        return Mathf.RoundToInt(value).ToString("N0", CultureInfo.InvariantCulture);
    }

    public float GetRating(float accuracy, int resolved)
    {
        if (resolved <= 0 || starThresholds == null)
            return 0;
        float rating = 0f;
        for (int i = 0; i < Mathf.Min(5, starThresholds.Length); i++)
        {
            if (accuracy >= Mathf.Clamp01(starThresholds[i])) rating = i + 1f;
            else if (halfStarThresholds != null && i < halfStarThresholds.Length &&
                accuracy >= Mathf.Clamp01(halfStarThresholds[i])) rating = i + .5f;
        }
        return rating;
    }

    private void PaintStars(RatingStar[] stars, float earned, bool animate = false)
    {
        if (stars == null || theme == null) return;
        for (int i = 0; i < stars.Length; i++)
            if (stars[i] != null)
            {
                stars[i].color = i < earned ? theme.yellow : theme.cream;
                stars[i].SetFill(Mathf.Clamp01(earned - i), animate);
            }
    }

    private static void StopStarMotion(RatingStar[] stars)
    {
        if (stars == null) return;
        foreach (var star in stars)
            if (star != null) star.SetFill(star.FillAmount, false);
    }

}
