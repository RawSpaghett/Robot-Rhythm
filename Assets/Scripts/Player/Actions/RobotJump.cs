using System;
using UnityEngine;

// Serialized inside ActionManager. Do not attach this class to a GameObject.
[Serializable]
public class RobotJump : ActionBase
{
    [Header("Normal Jump Settings")]
    [SerializeField, Min(0f)] private float jumpHeight = 2f;
    [SerializeField, Min(0.01f)] private float jumpDuration = 0.5f;

    [Header("Optional Squat and Landing")]
    [Tooltip("Grounded anticipation after confirmed input. Skipped if its selected sheet is missing.")]
    [SerializeField, Min(0f)] private float squatDuration = 0.06f;
    [Tooltip("Grounded recovery after the airborne phase. Skipped if its selected sheet is missing.")]
    [SerializeField, Min(0f)] private float landingDuration = 0.08f;

    [Header("Normal Jump Squat")]
    [SerializeField] private SpriteSheetSequence jumpSquatSheet = new SpriteSheetSequence();
    [SerializeField] private RobotAnimationVariations jumpSquatVariations = new RobotAnimationVariations();
    [Header("Normal Jump Landing")]
    [SerializeField] private SpriteSheetSequence jumpLandingSheet = new SpriteSheetSequence();
    [SerializeField] private RobotAnimationVariations jumpLandingVariations = new RobotAnimationVariations();
    [Header("Long Jump Squat")]
    [SerializeField] private SpriteSheetSequence longJumpSquatSheet = new SpriteSheetSequence();
    [SerializeField] private RobotAnimationVariations longJumpSquatVariations = new RobotAnimationVariations();
    [Header("Long Jump Landing")]
    [SerializeField] private SpriteSheetSequence longJumpLandingSheet = new SpriteSheetSequence();
    [SerializeField] private RobotAnimationVariations longJumpLandingVariations = new RobotAnimationVariations();

    public enum JumpPhase { Idle, Squat, Airborne, Landing }
    public JumpPhase CurrentPhase { get; private set; }
    [NonSerialized] private float activeSquatDuration;
    [NonSerialized] private float activeLandingDuration;
    [NonSerialized] private Sprite[] squatSequence;
    [NonSerialized] private Sprite[] airborneSequence;
    [NonSerialized] private Sprite[] landingSequence;
    [NonSerialized] private bool hasWrittenSprite;

    [Header("Normal Jump Animation")]
    [SerializeField] private SpriteSheetSequence jumpSheet = new SpriteSheetSequence();
    [SerializeField, HideInInspector] private Sprite[] jumpFrames = new Sprite[0];

    [Header("Long Jump Animation")]
    [SerializeField] private SpriteSheetSequence longJumpSheet = new SpriteSheetSequence();
    [SerializeField, HideInInspector] private Sprite[] longJumpFrames = new Sprite[0];

    [Header("Jump Timing Variations (Default = Jump Sheet)")]
    [SerializeField] private RobotAnimationVariations jumpVariations = new RobotAnimationVariations();
    [Header("Long Jump Timing Variations (Default = Long Jump Sheet)")]
    [SerializeField] private RobotAnimationVariations longJumpVariations = new RobotAnimationVariations();

    public Sprite[] GetAnimationFrames(RobotActionType type, RobotTimingVisual visual)
    {
        Sprite[] normalFrames = jumpSheet != null && jumpSheet.IsValid ? jumpSheet.Frames : jumpFrames;
        Sprite[] chargedFrames = longJumpSheet != null && longJumpSheet.IsValid ? longJumpSheet.Frames : longJumpFrames;
        return type == RobotActionType.LongJump
            ? (longJumpVariations != null ? longJumpVariations.Select(visual, chargedFrames) : chargedFrames)
            : (jumpVariations != null ? jumpVariations.Select(visual, normalFrames) : normalFrames);
    }

    [Header("Long Jump Charging Animation")]
    [SerializeField] private SpriteSheetSequence chargingSheet = new SpriteSheetSequence();
    [SerializeField, HideInInspector] private Sprite[] chargingFrames = new Sprite[0];
    [Tooltip("Seconds to play the charging sequence. Visual only: does not change jump strength or input timing.")]
    [SerializeField, Min(0.01f)] private float chargingAnimationDuration = 1f;

    [NonSerialized] private Sprite[] activeChargingFrames;
    [NonSerialized] private Sprite spriteBeforeCharging;
    [NonSerialized] private float chargingTimer;
    [NonSerialized] private float activeChargingDuration;
    public bool IsChargingAnimationPlaying { get; private set; }

    // A visual preview only. It does not set IsRunning, move the robot or play SFX.
    public bool BeginChargingAnimation()
    {
        if (IsChargingAnimationPlaying) return true;
        Sprite[] frames = chargingSheet != null && chargingSheet.IsValid ? chargingSheet.Frames : chargingFrames;
        if (IsRunning || Renderer == null || frames == null || frames.Length == 0)
            return false;
        for (int i = 0; i < frames.Length; i++)
            if (frames[i] == null) return false;

        activeChargingFrames = (Sprite[])frames.Clone();
        spriteBeforeCharging = Renderer.sprite;
        chargingTimer = 0f;
        activeChargingDuration = Mathf.Max(0.01f, chargingAnimationDuration);
        IsChargingAnimationPlaying = true;
        Renderer.sprite = activeChargingFrames[0];
        return true;
    }

    public void TickChargingAnimation(float deltaTime)
    {
        if (!IsChargingAnimationPlaying || Renderer == null) return;
        chargingTimer = Mathf.Min(activeChargingDuration,
            chargingTimer + Mathf.Max(0f, deltaTime));
        float progress = chargingTimer / activeChargingDuration;
        int frame = Mathf.Min(Mathf.FloorToInt(progress * activeChargingFrames.Length),
            activeChargingFrames.Length - 1);
        Renderer.sprite = activeChargingFrames[frame];
    }

    public void StopChargingAnimation()
    {
        if (IsChargingAnimationPlaying && Renderer != null)
            Renderer.sprite = spriteBeforeCharging;
        IsChargingAnimationPlaying = false;
        activeChargingFrames = null;
        spriteBeforeCharging = null;
        chargingTimer = 0f;
    }

    [Header("Long Jump Height")]
    [SerializeField, Min(0f)] private float minimumLongJumpHeight = 2.5f;
    [SerializeField, Min(0f)] private float maximumLongJumpHeight = 3.5f;

    [Header("Long Jump Duration")]
    [SerializeField, Min(0.01f)] private float minimumLongJumpDuration = 0.8f;
    [SerializeField, Min(0.01f)] private float maximumLongJumpDuration = 1.2f;

    [NonSerialized] private Vector3 restingLocalPosition;
    [NonSerialized] private float jumpTimer;
    [NonSerialized] private float activeJumpHeight;
    [NonSerialized] private float activeJumpDuration;

    [NonSerialized] private Sprite spriteBeforeJump;
    [NonSerialized] private Sprite[] activeFrames;
    [NonSerialized] private int currentFrame;
    [NonSerialized] private bool playingSpriteAnimation;

    // ActionManager uses this to open the input buffer near the action's end.
    public override float RemainingTime =>
        IsRunning ? Mathf.Max(0f, activeSquatDuration + activeJumpDuration + activeLandingDuration - jumpTimer) : 0f;

    public override bool TryBegin(RobotActionType type, float charge)
    {
        return TryBeginWithFeedback(type, charge, RobotTimingVisual.Default);
    }

    public bool TryBeginWithFeedback(RobotActionType type, float charge, RobotTimingVisual visual)
    {
        if (IsRunning || Visual == null ||
            (type != RobotActionType.Jump && type != RobotActionType.LongJump))
            return false;

        if (type == RobotActionType.LongJump)
        {
            charge = Mathf.Clamp01(charge);
            activeJumpHeight = Mathf.Lerp(
                minimumLongJumpHeight,
                Mathf.Max(minimumLongJumpHeight, maximumLongJumpHeight),
                charge);
            activeJumpDuration = Mathf.Lerp(
                minimumLongJumpDuration,
                Mathf.Max(minimumLongJumpDuration, maximumLongJumpDuration),
                charge);
        }
        else
        {
            activeJumpHeight = jumpHeight;
            activeJumpDuration = jumpDuration;
        }

        activeJumpHeight = Mathf.Max(0f, activeJumpHeight);
        activeJumpDuration = Mathf.Max(0.01f, activeJumpDuration);

        // Capture each attempt's starting position so relocation between jumps
        // does not send the robot back to its original scene position.
        restingLocalPosition = Visual.localPosition;
        jumpTimer = 0f;
        IsRunning = true;

        // Existing jump duration still means AIRTIME. Optional grounded phases
        // extend the action, and RemainingTime includes them for input buffering.
        bool longJump = type == RobotActionType.LongJump;
        squatSequence = GetPhaseFrames(
            longJump ? longJumpSquatSheet : jumpSquatSheet,
            longJump ? longJumpSquatVariations : jumpSquatVariations, visual);
        landingSequence = GetPhaseFrames(
            longJump ? longJumpLandingSheet : jumpLandingSheet,
            longJump ? longJumpLandingVariations : jumpLandingVariations, visual);
        airborneSequence = GetAnimationFrames(type, visual);
        if (airborneSequence != null) airborneSequence = (Sprite[])airborneSequence.Clone();
        activeSquatDuration = squatSequence != null ? Mathf.Max(0f, squatDuration) : 0f;
        activeLandingDuration = landingSequence != null ? Mathf.Max(0f, landingDuration) : 0f;
        hasWrittenSprite = false;
        CurrentPhase = JumpPhase.Idle;
        SetPhase(activeSquatDuration > 0f ? JumpPhase.Squat : JumpPhase.Airborne);
        //if (FXManager.Instance != null) FXManager.Instance.PlaySFX("jump");
        return true;
    }

    public override void Tick(float deltaTime)
    {
        if (!IsRunning) return;
        if (Visual == null) { Cancel(); return; }

        jumpTimer += Mathf.Max(0f, deltaTime);
        float airborneEnd = activeSquatDuration + activeJumpDuration;
        float totalDuration = airborneEnd + activeLandingDuration;
        if (jumpTimer >= totalDuration)
        {
            Cancel();
            return;
        }

        float progress;
        if (jumpTimer < activeSquatDuration)
        {
            SetPhase(JumpPhase.Squat);
            Visual.localPosition = restingLocalPosition;
            progress = jumpTimer / activeSquatDuration;
        }
        else if (jumpTimer < airborneEnd)
        {
            SetPhase(JumpPhase.Airborne);
            progress = Mathf.Clamp01((jumpTimer - activeSquatDuration) / activeJumpDuration);
            float verticalOffset = 4f * activeJumpHeight * progress * (1f - progress);
            Visual.localPosition = restingLocalPosition + Vector3.up * verticalOffset;
        }
        else
        {
            SetPhase(JumpPhase.Landing);
            Visual.localPosition = restingLocalPosition;
            progress = (jumpTimer - airborneEnd) / activeLandingDuration;
        }
        // Total elapsed time carries across boundaries even on a slow frame.
        UpdateSpriteAnimation(Mathf.Clamp01(progress));
    }

    private Sprite[] GetPhaseFrames(SpriteSheetSequence sheet,
        RobotAnimationVariations variations, RobotTimingVisual visual)
    {
        Sprite[] frames = sheet != null && sheet.IsValid ? sheet.Frames : null;
        if (variations != null) frames = variations.Select(visual, frames);
        // No invisible anticipation/recovery delay when its artwork is absent.
        return Renderer != null && RobotAnimationVariations.IsValid(frames)
            ? (Sprite[])frames.Clone() : null;
    }

    private void SetPhase(JumpPhase phase)
    {
        if (CurrentPhase == phase) return;
        CurrentPhase = phase;
        BeginSpriteAnimation(phase == JumpPhase.Squat ? squatSequence
            : phase == JumpPhase.Landing ? landingSequence : airborneSequence);
        // Missing airborne art should not leave the squat pose frozen in midair.
        if (!playingSpriteAnimation && hasWrittenSprite && Renderer != null)
            Renderer.sprite = spriteBeforeJump;
    }

    public override void Cancel()
    {
        if (IsRunning && Visual != null)
            Visual.localPosition = restingLocalPosition;

        // Restore only if this attempt actually changed the sprite.
        if (hasWrittenSprite && Renderer != null)
            Renderer.sprite = spriteBeforeJump;

        activeFrames = null;
        spriteBeforeJump = null;
        currentFrame = 0;
        playingSpriteAnimation = false;
        hasWrittenSprite = false;
        squatSequence = null;
        airborneSequence = null;
        landingSequence = null;
        activeSquatDuration = 0f;
        activeLandingDuration = 0f;
        CurrentPhase = JumpPhase.Idle;
        jumpTimer = 0f;
        IsRunning = false;
    }

    private void BeginSpriteAnimation(Sprite[] frames)
    {
        activeFrames = null;
        playingSpriteAnimation = false;
        currentFrame = 0;

        // Animation is optional so jumping still works before art is ready.
        if (frames == null || frames.Length == 0)
            return;

        if (Renderer == null)
        {
            Debug.LogWarning(
                "RobotJump: assign Robot Renderer on ActionManager to play sprites.",
                LogContext);
            return;
        }

        for (int i = 0; i < frames.Length; i++)
        {
            if (frames[i] == null)
            {
                Debug.LogWarning(
                    $"RobotJump: animation frame {i} is unassigned. Playing movement only.",
                    LogContext);
                return;
            }
        }

        // Freeze this attempt's sequence against Inspector edits during play.
        activeFrames = (Sprite[])frames.Clone();
        // Save the pre-action sprite once, not again at every phase transition.
        if (!hasWrittenSprite) spriteBeforeJump = Renderer.sprite;
        hasWrittenSprite = true;
        playingSpriteAnimation = true;
        Renderer.sprite = activeFrames[0];
    }

    private void UpdateSpriteAnimation(float progress)
    {
        if (!playingSpriteAnimation || Renderer == null)
            return;

        // Equal time per frame, using the same progress as the movement arc.
        // Low frame rates may skip frames rather than prolong the jump.
        int frame = Mathf.Min(
            Mathf.FloorToInt(progress * activeFrames.Length),
            activeFrames.Length - 1);

        if (frame != currentFrame)
        {
            currentFrame = frame;
            Renderer.sprite = activeFrames[frame];
        }
    }

}
