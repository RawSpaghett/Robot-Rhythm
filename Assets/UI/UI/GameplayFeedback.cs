using RobotRhythm.UI;
using UnityEngine;
using UnityEngine.UI;

// Presents the level's existing timing results and music clock.
public class GameplayFeedback : MonoBehaviour
{
    [SerializeField] private Text judgment;
    [SerializeField] private Image songFill;
    [SerializeField] private UiController preferences;
    [SerializeField, Min(.1f)] private float feedbackDuration = .8f;

    private float remaining;

    private void OnEnable()
    {
        ObstacleBase.OnObstacleResolved += ShowJudgment;
        ClearJudgment();
    }

    private void OnDisable()
    {
        ObstacleBase.OnObstacleResolved -= ShowJudgment;
        ClearJudgment();
    }

    private void ShowJudgment(ObstacleBase obstacle, ObstacleResult result, float accuracy)
    {
        var game = GameManager.Instance;
        if (judgment == null || obstacle == null || game == null ||
            obstacle.gameObject.scene != game.GameplayScene || !(game.CurrentState is GAMESTART) ||
            Time.timeScale <= 0 || result == ObstacleResult.None) return;

        switch (result)
        {
            case ObstacleResult.Success:
                judgment.text = obstacle.WasPerfect ? "PERFECT!" : "GOOD!";
                judgment.color = obstacle.WasPerfect ? new Color32(255, 218, 73, 255) : new Color32(68, 239, 216, 255);
                break;
            case ObstacleResult.TooEarly:
                judgment.text = "TOO EARLY";
                judgment.color = new Color32(255, 166, 77, 255);
                break;
            case ObstacleResult.TooLate:
                judgment.text = "TOO LATE";
                judgment.color = new Color32(255, 166, 77, 255);
                break;
            default:
                judgment.text = "MISS";
                judgment.color = new Color32(255, 99, 87, 255);
                break;
        }
        remaining = feedbackDuration;
        judgment.canvasRenderer.SetAlpha(1);
    }

    private void Update()
    {
        var rhythm = RhythmManager.Instance;
        if (songFill != null)
            songFill.fillAmount = rhythm != null && rhythm.beatMap != null && rhythm.beatMap.SongLength > 0
                ? Mathf.Clamp01(rhythm.songPosition / rhythm.beatMap.SongLength) : 0;

        if (judgment == null || remaining <= 0) return;
        remaining = Mathf.Max(0, remaining - Time.deltaTime);
        if (remaining <= 0)
        {
            ClearJudgment();
            return;
        }
        float elapsed = feedbackDuration - remaining;
        bool reducedMotion = preferences != null && preferences.ReducedMotion;
        judgment.rectTransform.localScale = Vector3.one * (reducedMotion ? 1 : 1 + .12f * Mathf.Clamp01(1 - elapsed / .15f));
        judgment.canvasRenderer.SetAlpha(Mathf.Clamp01(remaining / .2f));
    }

    private void ClearJudgment()
    {
        remaining = 0;
        if (judgment == null) return;
        judgment.text = "";
        judgment.rectTransform.localScale = Vector3.one;
        judgment.canvasRenderer.SetAlpha(0);
    }
}
