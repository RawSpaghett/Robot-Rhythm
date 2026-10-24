using System.Collections.Generic;
using UnityEngine;

// One component per robot. Both player input and NPC logic use this public API.
[DisallowMultipleComponent]
public class ActionManager : MonoBehaviour
{
    [Header("This Robot's Visuals")]
    [Tooltip("Prefer a visual child so jumping does not overwrite root movement.")]
    [SerializeField] private Transform robotVisual;
    [SerializeField] private SpriteRenderer robotRenderer;

    [Header("Action Settings")]
    [SerializeField] private RobotJump jump = new RobotJump();
    [SerializeField] private RobotDuck duck = new RobotDuck();
    [SerializeField] private RobotBrake brake = new RobotBrake();
    [SerializeField] private RobotAccelerate accelerate = new RobotAccelerate();

    [Header("Default Running Animation")]
    [SerializeField]
    private RobotRunAnimation runningAnimation =
        new RobotRunAnimation();

    private readonly Dictionary<RobotActionType, ActionBase> actions =
        new Dictionary<RobotActionType, ActionBase>();

    private ActionBase activeAction;
    private bool initialized;

    [Header("Missed Obstacle Feedback")]
    [Tooltip("Duration of sprite-only red feedback when an obstacle is missed without input.")]
    [SerializeField, Min(0.01f)] private float missedFeedbackDuration = 0.4f;
    private RobotActionType? pendingMiss;
    private Sprite[] missedFrames;
    private Sprite spriteBeforeMiss;
    private float missedTimer;
    private float activeMissedDuration;

    public bool IsBusy => activeAction != null && activeAction.IsRunning;

    private void Awake()
    {
        InitializeActions();
    }

    private void OnEnable()
    {
        InitializeActions();
        if (!IsBusy)
            runningAnimation.Play();
    }

    private void InitializeActions()
    {
        if (initialized) return;

        if (robotVisual == null) robotVisual = transform;
        if (robotRenderer == null)
            robotRenderer = robotVisual.GetComponent<SpriteRenderer>();

        if (jump == null) jump = new RobotJump();
        if (duck == null) duck = new RobotDuck();
        if (brake == null) brake = new RobotBrake();
        if (accelerate == null) accelerate = new RobotAccelerate();
        if (runningAnimation == null)
            runningAnimation = new RobotRunAnimation();

        jump.Initialize(robotVisual, robotRenderer, this);
        duck.Initialize(robotVisual, robotRenderer, this);
        brake.Initialize(robotVisual, robotRenderer, this);
        accelerate.Initialize(robotVisual, robotRenderer, this);
        runningAnimation.Initialize(robotRenderer, this);

        actions.Add(RobotActionType.Jump, jump);
        actions.Add(RobotActionType.LongJump, jump);
        actions.Add(RobotActionType.Duck, duck);
        actions.Add(RobotActionType.Brake, brake);
        actions.Add(RobotActionType.Accelerate, accelerate);
        initialized = true;
    }

    // Void wrappers can also be connected to UnityEvents if needed.
    public void TryJump() => TryPerform(RobotActionType.Jump);
    public void TryLongJump(float charge) =>
        TryPerform(RobotActionType.LongJump, charge);
    public void TryDuck() => TryPerform(RobotActionType.Duck);
    public void TryBrake() => TryPerform(RobotActionType.Brake);
    public void TryAccelerate() => TryPerform(RobotActionType.Accelerate);

    // Cosmetic charging preview; it does not make the robot busy or change input.
    public void SetLongJumpCharging(bool charging)
    {
        if (!charging)
        {
            if (!initialized || !jump.IsChargingAnimationPlaying) return;
            jump.StopChargingAnimation();
            if (!IsBusy && isActiveAndEnabled) runningAnimation.Play();
            return;
        }

        if (!isActiveAndEnabled || Time.timeScale <= 0f || AudioListener.pause || IsBusy || missedFrames != null)
            return;
        InitializeActions();
        if (jump.BeginChargingAnimation()) runningAnimation.Suspend();
    }

    // True means the animation started, NOT that the rhythm judgment succeeded.
    // Busy robots reject requests rather than queueing delayed rhythm actions.
    public bool TryPerform(RobotActionType type, float charge = 0f)
    {
        // NPC callers and existing UnityEvent wrappers keep the default animation.
        return TryPerformWithFeedback(type, charge, RobotTimingVisual.Default);
    }

    public bool TryPerformWithFeedback(RobotActionType type, float charge, RobotTimingVisual visual)
    {
        if (!isActiveAndEnabled || Time.timeScale <= 0f || IsBusy)
            return false;

        // Supports another component calling before this component's Awake.
        InitializeActions();

        if (!actions.TryGetValue(type, out ActionBase action))
            return false;

        // A new player action takes priority over cosmetic missed feedback.
        pendingMiss = null;
        StopMissedFeedback();

        // Restore the pre-charge sprite before the action captures its own pose.
        SetLongJumpCharging(false);

        bool started = action == jump
            ? jump.TryBeginWithFeedback(type, Mathf.Clamp01(charge), visual)
            : action == duck
                ? duck.TryBeginWithFeedback(type, Mathf.Clamp01(charge), visual)
                : action == brake
                    ? brake.TryBeginWithFeedback(type, Mathf.Clamp01(charge), visual)
                    : action == accelerate
                        ? accelerate.TryBeginWithFeedback(type, Mathf.Clamp01(charge), visual)
                        : action.TryBegin(type, Mathf.Clamp01(charge));
        
        if (started && this.gameObject.name == "PlayerRobotVisual") //not optimal solution, decouple inputmanager and action calls and then move this there.
        {
            if (action == jump) FXManager.Instance.PlaySFX("jump");
            else if (action == duck)  FXManager.Instance.PlaySFX("duck");
            else if (action == brake)  FXManager.Instance.PlaySFX("brake");
            else if (action == accelerate)  FXManager.Instance.PlaySFX("accelerate");
        }
        if (!started)
        {
            runningAnimation.Play();
            return false;
        }

        activeAction = action.IsRunning ? action : null;

        if (activeAction != null)
        {
            // TryBegin may already have displayed the action's first frame.
            // Suspend only stops the idle loop; it does not write a sprite.
            runningAnimation.Suspend();
        }
        else
        {
            runningAnimation.Play();
        }

        return true;
    }

    private void Update()
    {
        if (Time.timeScale <= 0f)
            return;

        // Only one animation path updates the renderer in a frame.
        if (activeAction != null)
        {
            activeAction.Tick(Time.deltaTime);

            if (!activeAction.IsRunning)
            {
                activeAction = null;

                // The action restores its pre-action sprite, then this sets the
                // first running frame immediately, before the frame is rendered.
                if (pendingMiss.HasValue)
                {
                    RobotActionType missedType = pendingMiss.Value;
                    pendingMiss = null;
                    BeginMissedFeedback(missedType);
                }
                else runningAnimation.Play();
            }

            return;
        }

        if (missedFrames != null)
        {
            if (AudioListener.pause) return;
            missedTimer += Time.deltaTime;
            if (missedTimer >= activeMissedDuration)
            {
                StopMissedFeedback();
                runningAnimation.Play();
            }
            else if (robotRenderer != null)
            {
                int frame = Mathf.Min(Mathf.FloorToInt(
                    missedTimer / activeMissedDuration * missedFrames.Length), missedFrames.Length - 1);
                robotRenderer.sprite = missedFrames[frame];
            }
            return;
        }

        if (jump.IsChargingAnimationPlaying)
        {
            if (!AudioListener.pause) jump.TickChargingAnimation(Time.deltaTime);
            return;
        }

        runningAnimation.Tick(Time.deltaTime);
    }

    public void CancelCurrentAction()
    {
        bool hadAction = activeAction != null || missedFrames != null || (initialized && jump.IsChargingAnimationPlaying);
        pendingMiss = null;
        StopMissedFeedback();
        if (initialized) jump.StopChargingAnimation();
        activeAction?.Cancel();
        activeAction = null;

        // Repeated cancellation while idle should not restart the loop.
        if (hadAction && initialized && isActiveAndEnabled)
            runningAnimation.Play();
    }

    private void OnDisable()
    {
        pendingMiss = null;
        StopMissedFeedback();
        // Restore action pose first, then stop the running presentation.
        if (initialized) jump.StopChargingAnimation();
        activeAction?.Cancel();
        activeAction = null;
        if (initialized)
            runningAnimation.Stop();
    }

    // A miss with no input must not trigger a real jump/duck or jump SFX.
    // If an action is finishing, show the reaction afterwards rather than interrupting motion.
    public void ShowMissedObstacle(RobotActionType requiredAction)
    {
        if (!isActiveAndEnabled) return;
        InitializeActions();
        if (IsBusy)
        {
            pendingMiss = requiredAction; // Keep only the most recent reaction; never queue gameplay actions.
            return;
        }
        BeginMissedFeedback(requiredAction);
    }

    private void BeginMissedFeedback(RobotActionType type)
    {
        StopMissedFeedback();
        SetLongJumpCharging(false);
        Sprite[] frames = type == RobotActionType.Duck
            ? duck.GetAnimationFrames(RobotTimingVisual.Failed)
            : type == RobotActionType.Brake
                ? brake.GetAnimationFrames(RobotTimingVisual.Failed)
                : type == RobotActionType.Accelerate
                    ? accelerate.GetAnimationFrames(RobotTimingVisual.Failed)
                    : jump.GetAnimationFrames(type, RobotTimingVisual.Failed);
        if (robotRenderer == null || !RobotAnimationVariations.IsValid(frames))
        {
            runningAnimation.Play();
            return;
        }
        spriteBeforeMiss = robotRenderer.sprite;
        missedFrames = (Sprite[])frames.Clone();
        missedTimer = 0f;
        activeMissedDuration = Mathf.Max(0.01f, missedFeedbackDuration);
        runningAnimation.Suspend();
        robotRenderer.sprite = missedFrames[0];
    }

    private void StopMissedFeedback()
    {
        if (missedFrames != null && robotRenderer != null)
            robotRenderer.sprite = spriteBeforeMiss;
        missedFrames = null;
        spriteBeforeMiss = null;
        missedTimer = 0f;
    }

}
