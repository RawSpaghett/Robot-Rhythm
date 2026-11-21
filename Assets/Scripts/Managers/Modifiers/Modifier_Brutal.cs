using UnityEngine;
using System;

[Serializable]
public class Modifier_Brutal: ModifierBase
{
    
    public override void Enabled()
    {
        ObstacleSpawner obstacleSpawner = GameObject.Find("ObstacleSpawner").GetComponent<ObstacleSpawner>(); //temp solution
        obstacleSpawner.hideObstacles = true;
    }
}
