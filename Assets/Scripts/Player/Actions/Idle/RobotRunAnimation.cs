using System;
using UnityEngine;

// Background sprite loop owned by ActionManager, not a separate MonoBehaviour.
// It has no Update of its own, so it cannot overwrite an active action's sprites.
[Serializable]
public class RobotRunAnimation
{
    [Tooltip("Running sprites in playback order. Loops while no action is active.")]
    [SerializeField] private Sprite[] runFrames = new Sprite[0];

    [SerializeField, Min(0.01f)] private float framesPerSecond = 12f;

    [NonSerialized] private SpriteRenderer renderer;
    [NonSerialized] private UnityEngine.Object logContext;
    [NonSerialized] private Sprite originalSprite;
    [NonSerialized] private Sprite[] activeFrames;
    [NonSerialized] private float frameProgress;
    [NonSerialized] private int currentFrame;
    [NonSerialized] private bool isPlaying;
    [NonSerialized] private bool hasWrittenSprite;

    public void Initialize(SpriteRenderer robotRenderer, UnityEngine.Object context)
    {
        renderer = robotRenderer;
        logContext = context;
        originalSprite = renderer != null ? renderer.sprite : null;
        activeFrames = null;
        isPlaying = false;
        hasWrittenSprite = false;
    }

    // Called when the robot becomes idle. Restart from the first running frame.
    public void Play()
    {
        isPlaying = false;
        activeFrames = null;

        // Optional until the art is available.
        if (runFrames == null || runFrames.Length == 0)
            return;

        if (renderer == null)
        {
            Debug.LogWarning(
                "RobotRunAnimation needs Robot Renderer assigned on ActionManager.",
                logContext);
            return;
        }

        for (int i = 0; i < runFrames.Length; i++)
        {
            if (runFrames[i] == null)
            {
                Debug.LogWarning(
                    $"RobotRunAnimation: Run Frames element {i} is unassigned.",
                    logContext);
                return;
            }
        }

        activeFrames = (Sprite[])runFrames.Clone();
        frameProgress = 0f;
        currentFrame = 0;
        isPlaying = true;
        renderer.sprite = activeFrames[0];
        hasWrittenSprite = true;
    }

    public void Tick(float deltaTime)
    {
        if (!isPlaying || renderer == null)
            return;

        frameProgress = (frameProgress +
            Mathf.Max(0f, deltaTime) * Mathf.Max(0.01f, framesPerSecond))
            % activeFrames.Length;

        int frame = Mathf.FloorToInt(frameProgress);
        if (frame != currentFrame)
        {
            currentFrame = frame;
            renderer.sprite = activeFrames[frame];
        }
    }

    // Suspend without restoring a sprite: the new action already owns the renderer.
    public void Suspend()
    {
        isPlaying = false;
    }

    // Called after the active action is canceled when the manager is disabled.
    public void Stop()
    {
        isPlaying = false;
        activeFrames = null;
        frameProgress = 0f;
        currentFrame = 0;

        if (hasWrittenSprite && renderer != null)
            renderer.sprite = originalSprite;

        hasWrittenSprite = false;
    }
}
