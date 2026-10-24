using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Counts cleared obstacles in a row and the multiplier that is earned from the combo then puts both into the ui
/// Listens for obstacles resolving so nothing has to call into it
/// </summary>
public class ComboTracker : MonoBehaviour
{
    private const float multiplierStep = 0.1f;
    private const float maxMultiplier = 2f;

    public int Count {  get; private set; }
    public float Multiplier { get; private set; } = 1f;
    public event UnityAction<float, float> OnScoreReady;

    private void OnEnable()
    {
        ObstacleBase.OnObstacleResolved += HandleResolved;
    }

    private void OnDisable()
    {
        ObstacleBase.OnObstacleResolved -= HandleResolved;
    }

    private void HandleResolved(ObstacleBase obstacle, ObstacleResult result, float accuracy)
    {
        if (obstacle == null || obstacle.gameObject.scene != gameObject.scene || result == ObstacleResult.None)
        {
            return;
        }

        // i tried getting the ui working in one of the scenes but couldnt so this will do for now
        ComboDisplay display = null;
        if (GameManager.Instance != null && GameManager.Instance.UI != null)
        {
            display = GameManager.Instance.UI.comboDisplay;
        }

        if (result == ObstacleResult.Success)
        {
            Count++;
            Multiplier = Mathf.Min(maxMultiplier, Multiplier + multiplierStep);

            Debug.Log("combo " + Count + ", x" + Multiplier);
            if (display != null) display.SetCombo(Count, Multiplier);
            // Send accuracy and the updated multiplier to ScoreManager.cs for this hit.
            OnScoreReady?.Invoke(accuracy, Multiplier);
            return;
        }

        // Failed obstacles still count toward accuracy, but do not get a combo bonus.
        OnScoreReady?.Invoke(accuracy, 1f);
        if (Count == 0)
        {
            return;
        }

        Debug.Log("combo broke at " + Count);

        Count = 0;
        Multiplier = 1f;

        if (display != null) display.ClearCombo();
    }
}
