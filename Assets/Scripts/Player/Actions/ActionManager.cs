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

    [Header("Default Running Animation")]
    [SerializeField]
    private RobotRunAnimation runningAnimation =
        new RobotRunAnimation();

    private readonly Dictionary<RobotActionType, ActionBase> actions =
        new Dictionary<RobotActionType, ActionBase>();

    private ActionBase activeAction;
    private bool initialized;

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
        if (runningAnimation == null)
            runningAnimation = new RobotRunAnimation();

        jump.Initialize(robotVisual, robotRenderer, this);
        duck.Initialize(robotVisual, robotRenderer, this);
        runningAnimation.Initialize(robotRenderer, this);

        actions.Add(RobotActionType.Jump, jump);
        actions.Add(RobotActionType.LongJump, jump);
        actions.Add(RobotActionType.Duck, duck);
        initialized = true;
    }

    // Void wrappers can also be connected to UnityEvents if needed.
    public void TryJump() => TryPerform(RobotActionType.Jump);
    public void TryLongJump(float charge) =>
        TryPerform(RobotActionType.LongJump, charge);
    public void TryDuck() => TryPerform(RobotActionType.Duck);

    // True means the animation started, NOT that the rhythm judgment succeeded.
    // Busy robots reject requests rather than queueing delayed rhythm actions.
    public bool TryPerform(RobotActionType type, float charge = 0f)
    {
        if (!isActiveAndEnabled || Time.timeScale <= 0f || IsBusy)
            return false;

        // Supports another component calling before this component's Awake.
        InitializeActions();

        if (!actions.TryGetValue(type, out ActionBase action))
            return false;

        if (!action.TryBegin(type, Mathf.Clamp01(charge)))
            return false;

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
                runningAnimation.Play();
            }

            return;
        }

        runningAnimation.Tick(Time.deltaTime);
    }

    public void CancelCurrentAction()
    {
        bool hadAction = activeAction != null;
        activeAction?.Cancel();
        activeAction = null;

        // Repeated cancellation while idle should not restart the loop.
        if (hadAction && initialized && isActiveAndEnabled)
            runningAnimation.Play();
    }

    private void OnDisable()
    {
        // Restore action pose first, then stop the running presentation.
        activeAction?.Cancel();
        activeAction = null;
        if (initialized)
            runningAnimation.Stop();
    }

}
