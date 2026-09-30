using UnityEngine;

public class PlayerActionController : MonoBehaviour
{
    /*
PURPOSE
Connects the player's action requests to the rhythm and obstacle systems.
This will replace the input-routing role currently handled by ObstacleTimingTest.

GestureButtonInput already recognizes gestures and executes the matching action
when the player RELEASES the button. The individual action scripts then invoke
their On[Action]Requested events. This controller should receive those requests;
it should not duplicate gesture recognition or read pointer input.

EXPECTED INPUT METHODS
Provide public methods that can be connected through the Inspector:
- RegisterJumpAttempt()
- RegisterLongJumpAttempt(float charge)
- RegisterBrakeAttempt()
- RegisterAccelerateAttempt()
- RegisterDuckAttempt()

On the Gameplay Button, connect each action component's request event to the
corresponding method on this controller. For long jump, use the dynamic float
event option so the actual charge value is passed through.

Replace the ObstacleTimingTest event connections when this controller is ready.
Do not leave both controllers receiving the same attempt in the gameplay scene.

RHYTHM MANAGER CONNECTION
This controller needs access to:
- The current authoritative music/rhythm timestamp.
- Scheduled obstacle or command targets and their target times.
- The current gameplay state, including pause, restart, and song completion.

Capture the rhythm timestamp immediately when a request is received.
Do not wait for an animation, coroutine, or visual effect before recording it.
Gesture hold duration is not a timestamp on the music timeline.

Target times and input timestamps MUST use the same clock and units.
ObstacleTimingTest currently uses Time.timeAsDouble as a temporary test clock.
Production timing should consistently use the clock provided by RhythmManager.

OBSTACLE ROUTING
Determine which active obstacle or command is eligible to receive the attempt.
Do not broadcast the input to every obstacle in the scene.

The current ObstacleBase API provides:
- Prepare(double targetTime)
- EvaluateInput(ObstacleInputType inputType, double inputTime, float charge = 0f)
- CheckForMiss(double currentTime)
- CancelAttempt()

Map the request methods to these existing input types:
- Jump      -> ObstacleInputType.Jump
- LongJump  -> ObstacleInputType.LongJump
- Brake     -> ObstacleInputType.Brake
- Accelerate-> ObstacleInputType.Accelerate
- Duck      -> ObstacleInputType.Dodge

Pass the supplied charge value through for long jumps.
Other actions can use the default charge value of zero.

ObstacleBase already evaluates timing, accepted input, and accuracy.
Avoid duplicating that judgment logic in this controller.
PotholeObstacle accepts Jump or LongJump; DuckObstacle accepts Dodge.

SCHEDULING AND MISSES
Agree on which system owns preparing targets and checking for missed inputs.
Whether that is this controller or a separate scheduler, each obstacle must:
- Be prepared once for its scheduled attempt.
- Receive miss checks even when the player provides no input.
- Stop receiving attempts after it has resolved.
- Be canceled/reset appropriately when the level restarts.

This controller should not copy ObstacleTimingTest's automatic repeating
countdown. Real attempts should follow the level's scheduled rhythm targets.

IMPORTANT CURRENT BEHAVIOR
ObstacleBase resolves an attempt immediately on an early or late input.
An incorrect action inside the timing window also resolves it as WrongInput.
Routing must therefore avoid sending unrelated inputs to distant future targets.

Decide how to handle inputs when there is no eligible target, and how to select
a target when timing windows overlap. Do not silently evaluate multiple targets
for one request unless the game design explicitly requires that behavior.

RESULTS AND GAMEPLAY
ObstacleBase exposes LastResult, LastAccuracy, WasCleared, and WasPerfect.
Use WasCleared / LastResult to distinguish success from failure.
Do not assume zero accuracy means failure: a valid window-boundary input can
succeed with zero accuracy.

Successful brake/acceleration judgments can request effects from the level
movement controller. That controller should own scrolling speed, effect
duration, and restoration. Its API is still pending.

Ducking/jumping presentation should remain separate from timing judgment.
An attempt can play its animation even when it is early, late, or incorrect.
Avoid triggering an animation twice if the action event already calls a visual
or animation component directly.

Ensure score, damage, and other result effects are applied once per resolved
attempt. Agree on which system owns those effects before adding listeners.

LIFECYCLE
Ignore gameplay requests when the level is paused, restarting, or finished.
Clear stale target references during resets and scene changes.
If subscribing to events in code, unsubscribe when disabled.
Do not register the same callback through both code and the Inspector.

INTEGRATION STATUS
Gesture recognition and release-triggered action events are already implemented.
ObstacleBase, PotholeObstacle, and DuckObstacle provide timing-based evaluation.
RhythmManager integration, production target routing, level movement effects,
and final animation connections still need implementation.
*/


}
