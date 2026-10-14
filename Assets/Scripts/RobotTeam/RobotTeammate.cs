using System.Diagnostics;
using UnityEngine;

public class RobotTeammate : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        ObstacleBase.OnPerfectTiming += Animate;
    }

    private void OnDisable()
    {
        
    }

    private void Animate(ObstacleInputType type, float dspTime)
    {
        switch(type)
        {
            case ObstacleInputType.Dodge:
                return;
            case ObstacleInputType.Jump:
                return;
        }
    }

    private void CalculateOffset()
    {
        
    }
}
