using UnityEngine;
using UnityEngine.InputSystem;

//https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Touch.html



public class InputManager : MonoBehaviour
{
    private PlayerInput inputSystem;

    private InputAction pressAction;
    private InputAction dragAction;


    void Awake()
    {
        inputSystem = GetComponent<PlayerInput>();
        pressAction = inputSystem.actions.FindAction("Press");
        dragAction = inputSystem.actions.FindAction("Drag");


    }

    private void OnEnable()
    {
        pressAction.performed += Jump;
        pressAction.performed += Drag;

    }

    private void OnDisable()
    {
        pressAction.performed -= Jump;
        pressAction.performed -= Drag;
    }

    private void Update()
    {}

    private void Jump(InputAction.CallbackContext context)
    {
        //Jump and Charge jump
    }

    private void Drag(InputAction.CallbackContext context)
    {
        //determine duck, accelerate or brake
    }

    private void Duck()
    {}

    private void Accelerate()
    {}

    private void Brake()
    {}
}
