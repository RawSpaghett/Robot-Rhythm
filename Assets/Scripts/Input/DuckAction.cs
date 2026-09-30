using UnityEngine;
using UnityEngine.Events;

// Recognizes an input attempt; actual gameplay belongs to the receiving systems.
// GestureButtonInput calls Matches during feedback and again on release.
// It calls Execute only on release, for the first matching enabled action.
[DisallowMultipleComponent]
public class DuckAction : ButtonActionBase
{
    [Header("Duck Request")]
    // FUTURE INSPECTOR CONNECTION:
    // Add the receiving component to this event in the Inspector and select its
    // public, parameterless request method. An empty event is safe during testing.
    // Usually the receiver should judge rhythm timing before applying a gameplay
    // effect; directly wiring a movement effect would bypass that judgment.
    [SerializeField] private UnityEvent onDuckRequested = new UnityEvent();

    [Header("Debug")]
    // Logs recognition only. This does not mean the attempt hit the beat window.
    [SerializeField] private bool logRequests;

    // ALTERNATIVE CODE CONNECTION:
    // A receiver can subscribe with OnDuckRequested.AddListener(itsMethod)
    // in OnEnable and RemoveListener(itsMethod) in OnDisable.
    // Use either Inspector wiring or a code subscription for the same callback,
    // otherwise that callback could run twice.
    public UnityEvent OnDuckRequested => onDuckRequested;

    public override bool Matches(GestureData gesture)
    {
        // Recognition only: do not play animations, change speed, or invoke events
        // here because GestureButtonInput also calls this while the button is held.
        // IsHold uses the existing minimum hold duration. Direction uses the
        // existing swipe-distance and direction-dominance settings.
        return gesture.IsHold && gesture.Direction == SwipeDirection.Down;
    }

    public override void Execute(GestureData gesture)
    {
        // Defensive check if another script ever calls Execute directly.
        if (!Matches(gesture)) return;

        if (logRequests)
            Debug.Log("Duck requested on release.", this);

        // FUTURE DUCK / DODGE CONNECTION:
        // Connect a listener that requests the robot's duck animation and reports
        // the dodge attempt to the obstacle/rhythm system.
        // A duck-specific obstacle can integrate with the existing ObstacleBase
        // once its API is available. Evaluate success using the beat window;
        // the animation or a collider overlap should not decide timing accuracy.
        // Keep animation playback and success/failure judgment separate so an
        // early/late attempt can animate while still receiving a failed judgment.
        // Exact listener methods will be chosen after reviewing ObstacleBase.
        
        // THIS IS THE HANDOFF TO FUTURE FUNCTIONALITY:
        // Connected listeners run immediately at this point on release.
        // The rhythm listener should evaluate its authoritative beat clock here,
        // before delaying work for animations or other effects.
        // HeldDuration is gesture length, not a timestamp on the music timeline.
        // No changes to this invocation are needed for parameterless listeners.
        onDuckRequested.Invoke();
    }
}
