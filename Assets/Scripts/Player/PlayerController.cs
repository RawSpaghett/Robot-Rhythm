using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
    }

    private void OnEnable()
    {
        inputManager.OnSwipe += ProcessDirection;
        inputManager.OnPressed += Jump;
    }

    private void OnDisable()
    {
        inputManager.OnSwipe -= ProcessDirection;
        inputManager.OnPressed -= Jump;
    }

    private void Jump()
    {
        Debug.Log("JUMP");
    }

    private void ProcessDirection(InputManager.Direction direction) //calls according swipe functions
    {
        switch(direction)
        {
            case InputManager.Direction.Left:
                Brake();
                return;
            case InputManager.Direction.Right:
                Accelerate();
                return;
            case InputManager.Direction.Up:
                Jump();
                return;
            case InputManager.Direction.Down:
                Duck();
                return;
        }
    }

    private void Duck()
    {
        Debug.Log("DUCk");
    }

    private void Accelerate()
    {
        Debug.Log("ACCELERATE");
    }

    private void Brake()
    {
        Debug.Log("BRAKE");
    }
}
