using System;
using UnityEngine;

public enum RobotActionType
{
    Jump,
    LongJump,
    Duck,
    Brake,
    Accelerate
}

// Plain C# base class. ActionManager supplies references and calls Tick.
// These objects are owned by individual robots; no static/shared action state.
[Serializable]
public abstract class ActionBase
{
    [NonSerialized] protected Transform Visual;
    [NonSerialized] protected SpriteRenderer Renderer;
    [NonSerialized] protected UnityEngine.Object LogContext;

    public bool IsRunning { get; protected set; }

    // Actions do not accept buffered input until they expose a duration.
    public virtual float RemainingTime => IsRunning ? float.PositiveInfinity : 0f;

    public void Initialize(Transform visual, SpriteRenderer renderer,
        UnityEngine.Object logContext)
    {
        Visual = visual;
        Renderer = renderer;
        LogContext = logContext;
        IsRunning = false;
    }

    // Return false if this action cannot start (e.g. unassigned duck frames).
    public abstract bool TryBegin(RobotActionType type, float charge);
    public abstract void Tick(float deltaTime);

    // Restore the pose and clear runtime state. Safe to call when idle.
    public abstract void Cancel();
}
