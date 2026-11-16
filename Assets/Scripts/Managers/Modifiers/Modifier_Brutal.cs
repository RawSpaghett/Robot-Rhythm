using UnityEngine;
using System;

public class Modifier_Brutal: ModifierBase
{
    
    public override void Enabled()
    {
        ObstacleSpawner obstacleSpawner = GameObject.Find("ObstacleSpawner").GetComponent<ObstacleSpawner>(); //temp solution
        foreach(ObstacleSpawner.ObstacleSprite obstacle in obstacleSpawner.levelSprites )
        {
            obstacle.Sprite = null; //empties the sprite
        }
    }
}
