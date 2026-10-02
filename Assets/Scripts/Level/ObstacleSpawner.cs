using System.Collections.Generic;
using UnityEngine;

// Goes through a beatmap's obstacle list and spawns them at the right time
public class ObstacleSpawner : MonoBehaviour
{
    // Copy of the beat map list
    private List<ObstaclePlacement> obstacles;

    // gets the next obstacle up in the list 
    private int nextIndex;

    void Start()
    {
        BeatMap beatMap = RhythmManager.Instance.beatMap;

        obstacles = new List<ObstaclePlacement>(beatMap.Obstacles);
        obstacles.Sort(CompareByBeat);
    }

    void Update()
    {
        if (!RhythmManager.Instance.isPlaying)
        {
            return;
        }

        float currentBeat = RhythmManager.Instance.songPositionInBeats;

        while (nextIndex < obstacles.Count && obstacles[nextIndex].Beat <= currentBeat)
        {
            Spawn(obstacles[nextIndex]);
            nextIndex++;
        }
    }

    private void Spawn(ObstaclePlacement placement)
    {
        Debug.Log(placement.Type.DisplayName + " at beat " + placement.Beat);
    }

    private static int CompareByBeat(ObstaclePlacement a, ObstaclePlacement b)
    {
        return a.Beat.CompareTo(b.Beat);
    }
}
