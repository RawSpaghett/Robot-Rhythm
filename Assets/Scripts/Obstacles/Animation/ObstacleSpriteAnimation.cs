using UnityEngine;

// Optional prefab component. Does not affect obstacle judgment or movement.
[DisallowMultipleComponent]
public class ObstacleSpriteAnimation : MonoBehaviour
{
    [SerializeField] private bool playAnimation = true;
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private SpriteSheetSequence animationSheet = new SpriteSheetSequence();
    [SerializeField, Min(0.01f)] private float framesPerSecond = 12f;

    private Sprite originalSprite;
    private Sprite[] activeFrames;
    private float frameProgress;
    private int currentFrame;
    private bool isPlaying;

    private void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!playAnimation)
        {
            StopAnimation();
            return;
        }
        if (Time.timeScale <= 0f || AudioListener.pause) return;
        if (targetRenderer == null) { StopAnimation(); return; }

        if (!isPlaying)
        {
            if (animationSheet == null || !animationSheet.IsValid) return;
            // Deliberately capture here, not Awake/OnEnable: the spawner sets
            // level-specific art immediately AFTER Instantiate returns.
            originalSprite = targetRenderer.sprite;
            activeFrames = (Sprite[])animationSheet.Frames.Clone();
            frameProgress = 0f;
            currentFrame = 0;
            isPlaying = true;
            targetRenderer.sprite = activeFrames[0];
            return;
        }

        frameProgress = (frameProgress + Mathf.Max(0f, Time.deltaTime) *
            Mathf.Max(0.01f, framesPerSecond)) % activeFrames.Length;
        int frame = Mathf.FloorToInt(frameProgress);
        if (frame == currentFrame) return;
        currentFrame = frame;
        targetRenderer.sprite = activeFrames[frame];
    }

    // Called by the spawner after it assigns this stage's static fallback sprite.
    // Changes only the spawned instance, never the shared prefab or sheet asset.
    public void ConfigureForStage(SpriteRenderer renderer, SpriteSheetSequence sheet,
        float fps, bool useAnimation)
    {
        StopAnimation();
        targetRenderer = renderer;
        animationSheet = sheet;
        framesPerSecond = Mathf.Max(0.01f, fps);
        // An invalid stage sheet must not fall back to another stage's prefab loop.
        playAnimation = useAnimation && sheet != null && sheet.IsValid;
    }

    // Optional runtime API; the Inspector toggle is enough for prefab setup.
    public void SetAnimationEnabled(bool value)
    {
        playAnimation = value;
        if (!value) StopAnimation();
    }

    private void StopAnimation()
    {
        if (isPlaying && targetRenderer != null) targetRenderer.sprite = originalSprite;
        isPlaying = false;
        activeFrames = null;
        originalSprite = null;
        frameProgress = 0f;
    }

    private void OnDisable()
    {
        StopAnimation();
    }
}
