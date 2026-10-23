using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AccelerateObstacle : ObstacleBase
{
    public override ObstacleInputType RequiredInput =>
        ObstacleInputType.Accelerate;

    protected override bool AcceptsInput(
        ObstacleInputType inputType,
        float charge)
    {
        return inputType == ObstacleInputType.Accelerate;
    }
}