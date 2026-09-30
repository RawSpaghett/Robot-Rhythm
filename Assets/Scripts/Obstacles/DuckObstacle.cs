using UnityEngine;

// Timing-based duck obstacle. No collider check is required for its judgment.
// Prepare, EvaluateInput, and CheckForMiss are inherited from ObstacleBase.
[RequireComponent(typeof(SpriteRenderer))]
public class DuckObstacle : ObstacleBase
{
    protected override bool AcceptsInput(
        ObstacleInputType inputType,
        float charge)
    {
        // DuckAction represents the downward gesture. Dodge is the existing
        // gameplay input name used by ObstacleBase for that action.
        // Charge is unused: ducking only needs the correct action and timing.
        return inputType == ObstacleInputType.Dodge;
    }

    // FUTURE ANIMATION CONNECTION:
    // Connect DuckAction's request event to an animation controller separately.
    // The animation can play for an attempt even when its timing is incorrect.
    // Do not use animation completion or collider overlap to determine success.
    
    // FUTURE RESULT CONNECTION:
    // A result listener can use the inherited On Accuracy Evaluated event.
    // That listener should inspect WasCleared / LastResult for success/failure.
    // Accuracy alone is insufficient: an accepted window boundary scores zero.
    
    // FUTURE LEVEL CONNECTION:
    // Replace the test controller with the level's rhythm/input router.
    // It must Prepare the target time, route Dodge attempts to EvaluateInput,
    // and call CheckForMiss using the same clock as the supplied target time.
}
