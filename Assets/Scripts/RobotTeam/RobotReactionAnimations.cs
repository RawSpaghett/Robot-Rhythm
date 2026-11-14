using System;
using UnityEngine;

// Serialized inside each ActionManager. The manager is the only renderer writer.
// These are cosmetic responses to the PLAYER'S result, not robot gameplay actions.
[Serializable]
public class RobotReactionAnimations
{
    [SerializeField] private bool playReactions = true;
    [SerializeField, Min(0.01f)] private float duration = 0.45f;
    [SerializeField] private SpriteSheetSequence perfectSheet = new SpriteSheetSequence();
    [SerializeField] private SpriteSheetSequence earlyLateSheet = new SpriteSheetSequence();
    [SerializeField] private SpriteSheetSequence failedSheet = new SpriteSheetSequence();

    [NonSerialized] private SpriteRenderer renderer;
    [NonSerialized] private Sprite previousSprite;
    [NonSerialized] private Sprite[] activeFrames;
    [NonSerialized] private float timer;
    [NonSerialized] private float activeDuration;
    [NonSerialized] private int currentFrame;
    public bool IsPlaying { get; private set; }

    public void Initialize(SpriteRenderer target)
    {
        renderer = target;
    }

    public bool TryPlay(RobotTimingVisual visual)
    {
        SpriteSheetSequence sheet = visual == RobotTimingVisual.Perfect ? perfectSheet
            : visual == RobotTimingVisual.EarlyLate ? earlyLateSheet
            : visual == RobotTimingVisual.Failed ? failedSheet : null;
        if (!playReactions || renderer == null || sheet == null || !sheet.IsValid)
            return false; // Optional/unassigned reactions do not interrupt running.

        Stop();
        previousSprite = renderer.sprite;
        activeFrames = (Sprite[])sheet.Frames.Clone();
        activeDuration = Mathf.Max(0.01f, duration);
        timer = 0f;
        currentFrame = 0;
        IsPlaying = true;
        renderer.sprite = activeFrames[0];
        return true;
    }

    public void Tick(float deltaTime)
    {
        if (!IsPlaying) return;
        if (!playReactions || renderer == null) { Stop(); return; }
        timer += Mathf.Max(0f, deltaTime);
        if (timer >= activeDuration) { Stop(); return; }
        int frame = Mathf.Min(Mathf.FloorToInt(timer / activeDuration * activeFrames.Length),
            activeFrames.Length - 1);
        if (frame == currentFrame) return;
        currentFrame = frame;
        renderer.sprite = activeFrames[frame];
    }

    public void Stop()
    {
        if (IsPlaying && renderer != null) renderer.sprite = previousSprite;
        IsPlaying = false;
        activeFrames = null;
        previousSprite = null;
        timer = 0f;
    }
}
