using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RoadblockObstacle : ObstacleBase
{
    public override ObstacleInputType RequiredInput => ObstacleInputType.LongJump;
    protected override bool AcceptsInput(
        ObstacleInputType inputType,
        float charge)
    {
        // A pothole can be cleared with either jump type.
        return inputType == ObstacleInputType.LongJump;
    }
}