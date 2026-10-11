using System;

// This pairs the type with the beat it lands on so the BeatMap can make the level
// Serializable so unity can save this inside of the BeatMap asset, without this the list would show up empty
[Serializable]
public class ObstaclePlacement
{
    public ObstacleType Type;
    public float Beat;
}
