using UnityEngine;

[DisallowMultipleComponent]
public class RobotDuck : MonoBehaviour
{
    [Header("Visual Reference")]
    [SerializeField] private SpriteRenderer robotRenderer;

    [Header("Duck Animation")]
    [Tooltip("Animation sprites arranged in playback order.")]
    [SerializeField] private Sprite[] duckFrames = new Sprite[0];

    [Tooltip("Total time to play the entire duck animation once.")]
    [SerializeField, Min(0.01f)]
    private float duckDuration = 0.5f;

    private Sprite spriteBeforeDuck;

    private float duckTimer;
    private float activeDuckDuration;
    private int currentFrame;
    private bool isDucking;

    public bool IsDucking => isDucking;

    private void Awake()
    {
        // If the renderer is on a child, assign it in the Inspector.
        if (robotRenderer == null)
            robotRenderer = GetComponent<SpriteRenderer>();
    }

    // Called by the player's input controller or a team robot's controller.
    public void TryDuck()
    {
        // Prevent overlapping ducks and requests while paused or disabled.
        if (!isActiveAndEnabled || isDucking || Time.timeScale <= 0f)
            return;

        if (!HasValidAnimation())
            return;

        // Remember the sprite displayed before this duck began.
        spriteBeforeDuck = robotRenderer.sprite;

        activeDuckDuration = Mathf.Max(0.01f, duckDuration);
        duckTimer = 0f;
        currentFrame = 0;
        isDucking = true;

        // Display the first animation frame immediately.
        robotRenderer.sprite = duckFrames[currentFrame];
    }

    private void Update()
    {
        if (!isDucking || Time.timeScale <= 0f)
            return;

        if (robotRenderer == null)
        {
            CancelDuck();
            return;
        }

        duckTimer += Time.deltaTime;

        float progress = Mathf.Clamp01(
            duckTimer / activeDuckDuration);

        if (progress >= 1f)
        {
            CancelDuck();
            return;
        }

        // Each frame receives an equal portion of the total duration.
        int frame = Mathf.Min(
            Mathf.FloorToInt(progress * duckFrames.Length),
            duckFrames.Length - 1);

        if (frame != currentFrame)
        {
            currentFrame = frame;
            robotRenderer.sprite = duckFrames[currentFrame];
        }
    }

    // Restores the original sprite after completion or interruption.
    public void CancelDuck()
    {
        if (isDucking && robotRenderer != null)
            robotRenderer.sprite = spriteBeforeDuck;

        duckTimer = 0f;
        currentFrame = 0;
        isDucking = false;
    }

    private bool HasValidAnimation()
    {
        if (robotRenderer == null ||
            duckFrames == null ||
            duckFrames.Length == 0)
        {
            Debug.LogWarning(
                "RobotDuck needs a SpriteRenderer and at least one duck frame.",
                this);

            return false;
        }

        for (int i = 0; i < duckFrames.Length; i++)
        {
            if (duckFrames[i] == null)
            {
                Debug.LogWarning(
                    $"RobotDuck: Duck Frames element {i} is unassigned.",
                    this);

                return false;
            }
        }

        return true;
    }

    private void OnDisable()
    {
        CancelDuck();
    }

    // FUTURE CONNECTION:
    // The new input controller calls TryDuck() on the player's RobotDuck.
    // Team robot controllers call TryDuck() on their own instances.
    
    // This component handles sprite playback only.
    // Rhythm accuracy and obstacle success are evaluated separately.
    
    // Other animation scripts must avoid changing this renderer's sprite
    // while IsDucking is true. Keep duckFrames unchanged during playback.
}