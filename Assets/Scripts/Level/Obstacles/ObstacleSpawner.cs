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
    [Header("Timing")]
    [SerializeField] private RhythmManager rhythmManager;
    private bool levelFinished;
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
        public ObstacleBase Obstacle;
    }

    void Start()
    {
        if (rhythmManager == null)
            rhythmManager = RhythmManager.Instance;
        if (rhythmManager == null || rhythmManager.beatMap == null)
        {
            Debug.LogError("ObstacleSpawner needs a rhythm manager with a beatmap.", this);
            enabled = false;
            return;
        }
        BeatMap beatMap = rhythmManager.beatMap;

        obstacles = new List<ObstaclePlacement>(beatMap.Obstacles);
        obstacles.Sort(CompareByBeat);
    }

    void Update()
    {
        if (!rhythmManager.isPlaying || Time.timeScale <= 0f || AudioListener.pause || levelFinished)
        {
            return;
        }

        double currentTime = rhythmManager.SongTime;
        float currentBeat = (float)(currentTime / rhythmManager.beatMap.SecondsPerBeat);

        SpawnNextObstacle(currentBeat);
        MoveObstacles(currentBeat, currentTime);
        CheckLevelFinished(currentTime);
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
        if (placement.Type == null || placement.Type.Prefab == null)
        {
            Debug.LogWarning("A beatmap entry is missing its obstacle prefab.", this);
            return;
        }
        GameObject instance = Instantiate(placement.Type.Prefab, transform);

        ActiveObstacle entry = new ActiveObstacle();
        entry.Transform = instance.transform;
        entry.TargetBeat = placement.Beat;
        entry.Obstacle = instance.GetComponent<ObstacleBase>();
        // Cue pictures have no ObstacleBase, so they don't count as hazards.
        if (entry.Obstacle != null)
            entry.Obstacle.Prepare(placement.Beat * rhythmManager.beatMap.SecondsPerBeat);

        active.Add(entry);
    }

    private void MoveObstacles(float currentBeat, double currentTime)
    {
        // Counting down so when an obstacle is removed it doesnt shift the list
        for (int i = active.Count - 1; i >= 0; i--)
        {
            float beatsAway = active[i].TargetBeat - currentBeat;

            if (active[i].Obstacle != null)
                active[i].Obstacle.CheckForMiss(currentTime);

            active[i].Transform.position = new Vector3(hitLineX + beatsAway * distancePerBeat, groundY, 0f);

            if (beatsAway < -despawnBeats &&
                (active[i].Obstacle == null || active[i].Obstacle.IsResolved))
            {
                Destroy(active[i].Transform.gameObject);
                active.RemoveAt(i); 
            }
        }
    }

    public ObstacleBase FindInputTarget(double inputTime)
    {
        ObstacleBase nearest = null;
        double nearestDistance = double.MaxValue;
        foreach (ActiveObstacle entry in active)
        {
            ObstacleBase obstacle = entry.Obstacle;
            if (obstacle == null || !obstacle.isActiveAndEnabled || !obstacle.IsActive)
                continue;
            obstacle.CheckForMiss(inputTime);
            if (obstacle.IsResolved || inputTime < obstacle.TargetTime - obstacle.EarlyWindow)
                continue;
            double distance = System.Math.Abs(obstacle.TargetTime - inputTime);
            if (distance < nearestDistance)
            {
                nearest = obstacle;
                nearestDistance = distance;
            }
        }
        return nearest;
    }

    public ObstacleBase GetNextObstacle()
    {
        foreach (ActiveObstacle entry in active)
            if (entry.Obstacle != null && entry.Obstacle.isActiveAndEnabled && entry.Obstacle.IsActive)
                return entry.Obstacle;
        return null;
    }

    private void CheckLevelFinished(double currentTime)
    {
        if (nextIndex < obstacles.Count || GetNextObstacle() != null ||
            currentTime + rhythmManager.beatMap.FirstBeatOffset < rhythmManager.beatMap.SongLength)
            return;
        levelFinished = true;
        if (GameManager.Instance != null && GameManager.Instance.GameplayScene == gameObject.scene)
            GameManager.Instance.EndGame();
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
