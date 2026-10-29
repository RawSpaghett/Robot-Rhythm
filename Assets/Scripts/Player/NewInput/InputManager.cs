using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.InputSystem;

//https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Touch.html

public class InputManager : MonoBehaviour
{
    // Assign the player robot's ActionManager
    [SerializeField] private ActionManager actionManager;
    [Tooltip("Optional player timing/animation bridge. Assign it to enable judged variations.")]
    [SerializeField] private PlayerTimingFeedback timingFeedback;

    [SerializeField, Range(0f, 1f)]
    private float longJumpCharge = 1f;
    private PlayerInput inputSystem;
    private InputAction pressAction;
    private InputAction dragAction;

    [SerializeField] private string jumpID;
    [SerializeField] private string chargeJumpID;
    [SerializeField] private string duckID;

    private bool drag;
    public event Action OnPressed;
    public event Action<Direction> OnSwipe;
    
    void Awake()
    {
        if (timingFeedback == null) timingFeedback = GetComponent<PlayerTimingFeedback>();
        inputSystem = GetComponent<PlayerInput>();
        pressAction = inputSystem.actions.FindAction("Press");
        dragAction = inputSystem.actions.FindAction("Drag");
    }

    private void OnEnable()
    {
        pressAction.performed += Jump;
        dragAction.performed += Drag;
    }

    private void OnDisable()
    {
        if (actionManager != null) actionManager.SetLongJumpCharging(false);
        pressAction.performed -= Jump;
        dragAction.performed -= Drag;
    }


    private bool mouseHeld;
    private bool previousMouseDown;
    private Vector2 mouseStart;
    private void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null)
        {
            if (actionManager != null) actionManager.SetLongJumpCharging(false);
            return;
        }
        bool down = mouse.leftButton.isPressed;
        bool pressed = down && !previousMouseDown;
        bool released = !down && previousMouseDown;
        previousMouseDown = down;
        if (Time.timeScale <= 0 || AudioListener.pause)
        {
            mouseHeld = false;
            if (actionManager != null) actionManager.SetLongJumpCharging(false);
            return;
        }
        if (pressed)
        {
            mouseStart = mouse.position.ReadValue();
            mouseHeld = !IsOverUI(mouseStart);
        }

        // Animation only: use the same displacement threshold/direction as release.
        // Begin after swiping up, keep showing it while held, stop if direction changes.
        if (actionManager != null)
        {
            Vector2 heldDisplacement = mouse.position.ReadValue() - mouseStart;
            bool showCharging = down && mouseHeld &&
                heldDisplacement.magnitude >= 24f * Mathf.Max(.75f, Screen.height / 720f) &&
                GetDirection(heldDisplacement) == Direction.Up;
            actionManager.SetLongJumpCharging(showCharging);
        }

        if (released && mouseHeld)
        {
            mouseHeld = false;
            Vector2 displacement = mouse.position.ReadValue() - mouseStart;
            if (displacement.magnitude < 24f * Mathf.Max(.75f, Screen.height / 720f))
            {
                RequestAction(RobotActionType.Jump);
                
                OnPressed?.Invoke();
            }
            else PerformSwipe(GetDirection(displacement));
        }
    }
    private bool IsOverUI(Vector2 position)
    {
        if (EventSystem.current == null) return false;
        // Check this press position instead of the input module's previous pointer state.
        var pointer = new PointerEventData(EventSystem.current) { position = position };
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, hits);
        return hits.Count > 0;
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused)
        {
            mouseHeld = false;
            if (actionManager != null) actionManager.SetLongJumpCharging(false);
        }
    }
    private void Jump(InputAction.CallbackContext context)
    {
        if (context.control.device is Mouse || drag) return;
        // Request a normal jump.
        RequestAction(RobotActionType.Jump);
        if (FXManager.Instance != null) FXManager.Instance.PlaySFX(jumpID);

        OnPressed?.Invoke();
    }

    private void Drag(InputAction.CallbackContext context)
    {
        if (context.control.device is Mouse) return;
        Vector2 dragInput = context.ReadValue<Vector2>();
        if (dragInput.magnitude >= 24f * Mathf.Max(.75f, Screen.height / 720f))
        {
            drag = true;
            Direction direction = GetDirection(dragInput);
            PerformSwipe(direction);
            drag = false;
        }
    }
    private void PerformSwipe(Direction direction)
    {
        // Request the action matching the swipe direction.
        if (actionManager != null)
        {
            switch (direction)
            {
                case Direction.Up:
                    RequestAction(RobotActionType.LongJump, longJumpCharge);
                    if (FXManager.Instance != null) FXManager.Instance.PlaySFX(chargeJumpID);
                    break;

                case Direction.Down:
                    RequestAction(RobotActionType.Duck);
                    if (FXManager.Instance != null) FXManager.Instance.PlaySFX(duckID);
                    break;
                case Direction.Left:
                    RequestAction(RobotActionType.Brake);
                    break;
                case Direction.Right:
                    RequestAction(RobotActionType.Accelerate);
                    break;

            }
        }
        OnSwipe?.Invoke(direction);
    }

    private void RequestAction(RobotActionType action, float charge = 0f)
    {
        if (timingFeedback != null && timingFeedback.isActiveAndEnabled)
            timingFeedback.Perform(action, charge);
        else if (actionManager != null)
            actionManager.TryPerform(action, charge);
    }

    public enum Direction { Up, Down, Left, Right, None }
    private Direction GetDirection(Vector2 vector) //helper function for drag
    {
        if (vector == Vector2.zero)
        {
            Debug.Log("Drag Direction NONE");
            return Direction.None;
        }

        if (Mathf.Abs(vector.x) > Mathf.Abs((vector.y))) //if x is greater than y, left to right
        {
            return vector.x > 0 ? Direction.Right : Direction.Left;
        }
        else //else up and down
        {
            return vector.y > 0 ? Direction.Up : Direction.Down;
        }
    }
}



