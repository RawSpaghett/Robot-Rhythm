using UnityEngine;

public class BrakeObstacle : ObstacleBase
{
    public override ObstacleInputType RequiredInput =>
        ObstacleInputType.Brake;

    protected override bool AcceptsInput(
        ObstacleInputType inputType,
        float charge)
    {
        return inputType == ObstacleInputType.Brake;
    }
}