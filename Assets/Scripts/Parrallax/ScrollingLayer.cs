using UnityEngine;

/// <summary>
/// Scrolls a layer sideways, road needs to be one so it matches with obstacles
/// </summary>

public class ScrollingLayer : MonoBehaviour
{
    // 1 matches obstcles
    public float speedMultiplier = 1f;

    // width of the art in world units
    public float layerWidth = 20f;

    [SerializeField] private RhythmManager rhythmManager;
    [SerializeField] private ObstacleSpawner obstacleSpawner;

    private float startX;

    private void Awake()
    {
        startX = transform.position.x;
    }

    private void Update()
    {
        if (rhythmManager == null || !rhythmManager.isPlaying)
        {
            return;
        }

        float distance = rhythmManager.songPositionInBeats * obstacleSpawner.distancePerBeat * speedMultiplier;

        // Repeat wraps back to 0 after layerWidth
        float offset = Mathf.Repeat(distance, layerWidth);

        Vector3 position = transform.position;
        position.x = startX - offset;
        transform.position = position;
    }
}