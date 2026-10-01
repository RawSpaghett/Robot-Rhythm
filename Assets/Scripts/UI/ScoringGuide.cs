using UnityEngine;
using UnityEngine.UI;
using RobotRhythm.UI;

public class ScoringGuide : MonoBehaviour
{
    [SerializeField] private ScoreDisplay scoring;
    [SerializeField] private Text exampleScore;
    [SerializeField] private Text exampleAccuracy;
    [SerializeField] private Text exampleHits;
    [SerializeField] private RatingStar[] stars;
    [SerializeField] private UiTheme theme;
    private float points;
    private int hits;

    private void OnEnable() => ResetExample();

    // This is a scoring example only. It never sends events to the game.
    public void AddHit(float accuracy)
    {
        points += Mathf.Clamp01(accuracy) * ScoreManager.PointsPerPerfectHazard;
        hits++;
        Refresh();
    }

    public void ResetExample()
    {
        points = 0f;
        hits = 0;
        Refresh();
    }

    private void Refresh()
    {
        float accuracy = hits > 0 ? points / (hits * ScoreManager.PointsPerPerfectHazard) : 0f;
        exampleScore.text = Mathf.RoundToInt(points).ToString();
        exampleAccuracy.text = hits > 0 ? Mathf.RoundToInt(accuracy * 100f) + "%" : "--";
        exampleHits.text = hits + " HAZARDS";
        exampleScore.color = exampleAccuracy.color = scoring.GetAccuracyColor(accuracy);
        float rating = scoring.GetRating(accuracy, hits);
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].color = theme.yellow;
            stars[i].SetFill(Mathf.Clamp01(rating - i), true);
        }
    }
}
