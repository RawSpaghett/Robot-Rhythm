using UnityEngine;

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
            return;
        }

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
