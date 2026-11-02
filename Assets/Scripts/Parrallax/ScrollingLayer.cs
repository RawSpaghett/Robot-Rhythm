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

    // Move scenery after gameplay, using the same visual clock as obstacles.
    private void LateUpdate()
    {
        if (rhythmManager == null || !rhythmManager.isPlaying || Time.timeScale <= 0f || AudioListener.pause)
        {
            return;
        }

        float distance = (float)(rhythmManager.VisualSongTime / rhythmManager.beatMap.SecondsPerBeat) * obstacleSpawner.distancePerBeat * speedMultiplier;

        // Repeat wraps back to 0 after layerWidth
        float offset = Mathf.Repeat(distance, layerWidth);

        Vector3 position = transform.position;
        position.x = startX - offset;
        transform.position = position;
    }
}