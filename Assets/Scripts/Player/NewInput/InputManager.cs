using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.InputSystem;

//https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Touch.html

public class InputManager : MonoBehaviour
{
    private PlayerInput inputSystem;
    private InputAction pressAction;
    private InputAction dragAction;

    private bool drag;
    public event Action OnPressed;
    public event Action<Direction> OnSwipe;

    // Preview signal only. Confirmed actions still use OnPressed/OnSwipe on release.
    public event Action<Direction> OnHeldDirectionChanged;
    public Direction HeldDirection { get; private set; } = Direction.None;

    void Awake()
    {
        inputSystem = GetComponent<PlayerInput>();
        pressAction = inputSystem.actions.FindAction("Press");
        dragAction = inputSystem.actions.FindAction("Drag");
    }
    private bool userHeld;
    private bool previousUserDown;
    private Vector2 userStart;
    private void Update()
    {
        var userInput = Pointer.current;
        if (userInput == null || Time.timeScale <= 0f || AudioListener.pause || !Application.isFocused)
        {
            CancelGesture();
            return;
        }

        bool down = userInput.press.isPressed;
        bool pressed = down && !previousUserDown;
        bool released = !down && previousUserDown;
        previousUserDown = down;

        if (pressed)
        {
            userStart = userInput.position.ReadValue();
            userHeld = !IsOverUI(userStart);
        }

        // Use the same displacement, threshold and direction rules as release.
        if (down && userHeld)
        {
            Vector2 displacement = userInput.position.ReadValue() - userStart;
            SetHeldDirection(displacement.magnitude >= SwipeThreshold
                ? GetDirection(displacement) : Direction.None);
        }
        else SetHeldDirection(Direction.None); // Stop preview BEFORE the release action.

        if (released && userHeld)
        {
            userHeld = false;
            Vector2 displacement = userInput.position.ReadValue() - userStart;
            if (displacement.magnitude < SwipeThreshold)
            {
                OnPressed?.Invoke();
            }
            else PerformSwipe(GetDirection(displacement));
        }
    }
    private float SwipeThreshold => 24f * Mathf.Max(.75f, Screen.height / 720f);

    private void SetHeldDirection(Direction direction)
    {
        if (HeldDirection == direction) return;
        HeldDirection = direction;
        OnHeldDirectionChanged?.Invoke(direction);
    }

    private void CancelGesture()
    {
        userHeld = false;
        // A cancelled hold must not become an action on release or resume mid-hold.
        previousUserDown = Pointer.current != null && Pointer.current.press.isPressed;
        SetHeldDirection(Direction.None);
    }

    private void OnEnable() => CancelGesture();
    private void OnDisable() => CancelGesture();

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) CancelGesture();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) CancelGesture();
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

    private void Drag(InputAction.CallbackContext context)
    {
        Vector2 dragInput = context.ReadValue<Vector2>();
        if (dragInput.magnitude >= SwipeThreshold)
        {
            drag = true;
            Direction direction = GetDirection(dragInput);
            PerformSwipe(direction);
            drag = false;
        }
    }
    private void PerformSwipe(Direction direction)
    {
        OnSwipe?.Invoke(direction);
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



