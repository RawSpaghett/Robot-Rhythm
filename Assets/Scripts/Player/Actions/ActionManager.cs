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

    private readonly Dictionary<RobotActionType, ActionBase> actions =
        new Dictionary<RobotActionType, ActionBase>();

    private ActionBase activeAction;
    private bool initialized;

    public bool IsBusy => activeAction != null && activeAction.IsRunning;

    private void Awake()
    {
        InitializeActions();
    }

    private void InitializeActions()
    {
        if (initialized) return;

        if (robotVisual == null) robotVisual = transform;
        if (robotRenderer == null)
            robotRenderer = robotVisual.GetComponent<SpriteRenderer>();

        if (jump == null) jump = new RobotJump();
        if (duck == null) duck = new RobotDuck();

        jump.Initialize(robotVisual, robotRenderer, this);
        duck.Initialize(robotVisual, robotRenderer, this);

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
        return true;
    }

    private void Update()
    {
        if (activeAction == null || Time.timeScale <= 0f)
            return;

        activeAction.Tick(Time.deltaTime);

        if (!activeAction.IsRunning)
            activeAction = null;
    }

    public void CancelCurrentAction()
    {
        activeAction?.Cancel();
        activeAction = null;
    }

    private void OnDisable()
    {
        CancelCurrentAction();
    }

}
