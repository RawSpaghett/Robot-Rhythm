using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PotholeObstacle : ObstacleBase
{
    protected override bool AcceptsInput(
        ObstacleInputType inputType,
        float charge)
    {
        // A pothole can be cleared with either jump type.
        return inputType == ObstacleInputType.Jump ||
               inputType == ObstacleInputType.LongJump;
    }
}