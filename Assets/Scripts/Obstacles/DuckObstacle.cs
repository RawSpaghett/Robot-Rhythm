using UnityEngine;

// Timing-based duck obstacle. No collider check is required for its judgment.
// Prepare, EvaluateInput, and CheckForMiss are inherited from ObstacleBase.
[RequireComponent(typeof(SpriteRenderer))]
public class DuckObstacle : ObstacleBase
{
    public override ObstacleInputType RequiredInput => ObstacleInputType.Dodge;
    protected override bool AcceptsInput(
        ObstacleInputType inputType,
        float charge)
    {
        // DuckAction represents the downward gesture. Dodge is the existing
        // gameplay input name used by ObstacleBase for that action.
        // Charge is unused: ducking only needs the correct action and timing.
        return inputType == ObstacleInputType.Dodge;

    }
}
