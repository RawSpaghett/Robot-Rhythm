using UnityEngine;

[CreateAssetMenu(fileName = "NewObstacle", menuName = "Robot Rhythm/Obstacle Type")]
public class ObstacleType : ScriptableObject
{
    public string DisplayName = "New Obstacle";
    public float WidthInBeats = 1f; // Everything is going to be meassured in beats so if this were a pothole the jump would need to last this many beats
    public Color TimelineColor = Color.yellow;
}
