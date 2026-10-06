using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;

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
        //jump logic
    }

    private void ProcessDirection(InputManager.Direction direction) //calls according swipe functions
    {
        switch(direction)
        {
            case InputManager.Direction.Left:
                return;
            case InputManager.Direction.Right:
                return;
            case InputManager.Direction.Up:
                return;
            case InputManager.Direction.Down:
                return;
        }
    }
    
    private void Duck()
    {}

    private void Accelerate()
    {}

    private void Brake()
    {}
}
