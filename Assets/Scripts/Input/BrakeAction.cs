using UnityEngine;
using UnityEngine.Events;

// Recognizes an input attempt; actual gameplay belongs to the receiving systems.
// GestureButtonInput calls Matches during feedback and again on release.
// It calls Execute only on release, for the first matching enabled action.
[DisallowMultipleComponent]
public class BrakeAction : ButtonActionBase
{
    [Header("Brake Request")]
    // FUTURE INSPECTOR CONNECTION:
    // Add the receiving component to this event in the Inspector and select its
    // public, parameterless request method. An empty event is safe during testing.
    // Usually the receiver should judge rhythm timing before applying a gameplay
    // effect; directly wiring a movement effect would bypass that judgment.
    [SerializeField] private UnityEvent onBrakeRequested = new UnityEvent();

    [Header("Debug")]
    // Logs recognition only. This does not mean the attempt hit the beat window.
    [SerializeField] private bool logRequests;

    // ALTERNATIVE CODE CONNECTION:
    // A receiver can subscribe with OnBrakeRequested.AddListener(itsMethod)
    // in OnEnable and RemoveListener(itsMethod) in OnDisable.
    // Use either Inspector wiring or a code subscription for the same callback,
    // otherwise that callback could run twice.
    public UnityEvent OnBrakeRequested => onBrakeRequested;

    public override bool Matches(GestureData gesture)
    {
        // Recognition only: do not play animations, change speed, or invoke events
        // here because GestureButtonInput also calls this while the button is held.
        // IsHold uses the existing minimum hold duration. Direction uses the
        // existing swipe-distance and direction-dominance settings.
        return gesture.IsHold && gesture.Direction == SwipeDirection.Left;
    }

    public override void Execute(GestureData gesture)
    {
        // Defensive check if another script ever calls Execute directly.
        if (!Matches(gesture)) return;

        if (logRequests)
            Debug.Log("Brake requested on release.", this);

        // FUTURE BRAKING CONNECTION:
        // The listener should evaluate this release against the rhythm beat window.
        // After a successful judgment, it can ask the level movement controller
        // to pause background/obstacle scrolling temporarily and then restore it.
        // Let that controller own the brake duration and speed restoration.
        // Do not pause the whole game with Time.timeScale here: the rhythm clock,
        // music, and input must remain available during a brake.
        // A request is only an attempt; this event does not confirm a successful beat.
        
        // THIS IS THE HANDOFF TO FUTURE FUNCTIONALITY:
        // Connected listeners run immediately at this point on release.
        // The rhythm listener should evaluate its authoritative beat clock here,
        // before delaying work for animations or other effects.
        // HeldDuration is gesture length, not a timestamp on the music timeline.
        // No changes to this invocation are needed for parameterless listeners.
        onBrakeRequested.Invoke();
    }

    // No TryGetCharge override is needed: the base implementation returns false,
    // so this action does not display the long-jump charge slider.
}
