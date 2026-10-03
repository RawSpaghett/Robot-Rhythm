using System.Collections.Generic;
using UnityEngine;

// Goes through a beatmap's obstacle list and spawns them at the right time
public class ObstacleSpawner : MonoBehaviour
{
    // How far an obstacle goes off screen before despawning
    private const float despawnBeats = 2f;
    private const float gizmoHalfHeight = 3f;

    // Where the player is placed/obstacles arrival point
    public float hitLineX = -4f;

    // how many world units the level travels in a beat
    public float distancePerBeat = 3f;

    // How many beats before the hit time that an obstacle spawns
    public float spawnTime = 4f;

    public float groundY;
    // Sorted copy of the beat map list, editor adds in click order so it is sorted to beat order
    private List<ObstaclePlacement> obstacles;

    // gets the next obstacle up in the list 
    private int nextIndex;

    private List<ActiveObstacle> active = new List<ActiveObstacle>();

    // Pairs the object with the beat it is on since the object doesnt know
    private class ActiveObstacle
    {
        public Transform Transform;
        public float TargetBeat;
    }

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

        SpawnNextObstacle(currentBeat);
        MoveObstacles(currentBeat);
    }

    private void SpawnNextObstacle(float currentBeat)
    {
        while (nextIndex < obstacles.Count && obstacles[nextIndex].Beat - spawnTime <= currentBeat)
        {
            Spawn(obstacles[nextIndex]);
            nextIndex++;
        }
    }

    private void Spawn(ObstaclePlacement placement)
    {
        GameObject instance = Instantiate(placement.Type.Prefab);

        ActiveObstacle entry = new ActiveObstacle();
        entry.Transform = instance.transform;
        entry.TargetBeat = placement.Beat;

        active.Add(entry);
    }

    private void MoveObstacles(float currentBeat)
    {
        // Counting down so when an obstacle is removed it doesnt shift the list
        for (int i = active.Count - 1; i >= 0; i--)
        {
            float beatsAway = active[i].TargetBeat - currentBeat;

            active[i].Transform.position = new Vector3(hitLineX + beatsAway * distancePerBeat, groundY, 0f);

            if (beatsAway < -despawnBeats)
            {
                Destroy(active[i].Transform.gameObject);
                active.RemoveAt(i); 
            }
        }
    }

    private static int CompareByBeat(ObstaclePlacement a, ObstaclePlacement b)
    {
        return a.Beat.CompareTo(b.Beat);
    }

    // Draws the hit line so we can line up with the player
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(hitLineX, groundY - gizmoHalfHeight, 0f), new Vector3(hitLineX, groundY + gizmoHalfHeight, 0f));
    }
}
