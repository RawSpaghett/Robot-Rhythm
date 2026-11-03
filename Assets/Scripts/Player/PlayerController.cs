using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private ActionManager actionManager;
    
    [Tooltip("Optional player timing/animation bridge. Assign it to enable judged variations.")]
    [SerializeField] private PlayerTimingFeedback timingFeedback;
    
    [SerializeField, Range(0f, 1f)]
    private float longJumpCharge = 1f;
    
    [SerializeField] private string jumpID;
    [SerializeField] private string chargeJumpID;
    [SerializeField] private string duckID;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        if (timingFeedback == null) timingFeedback = GetComponent<PlayerTimingFeedback>();
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
        if (actionManager != null) actionManager.SetLongJumpCharging(false);
    }
    

    private void ProcessDirection(InputManager.Direction direction) //calls according swipe functions
    {
        switch(direction)
        {
            case InputManager.Direction.Up:
                RequestAction(RobotActionType.LongJump, longJumpCharge);
                if (FXManager.Instance != null) FXManager.Instance.PlaySFX(chargeJumpID);
                break;

            case InputManager.Direction.Down:
                RequestAction(RobotActionType.Duck);
                if (FXManager.Instance != null) FXManager.Instance.PlaySFX(duckID);
                break;
            case InputManager.Direction.Left:
                RequestAction(RobotActionType.Brake);
                break;
            case InputManager.Direction.Right:
                RequestAction(RobotActionType.Accelerate);
                break;
        }
    }

    private void Jump()
    {
        RequestAction(RobotActionType.Jump);
    }
    
    private void RequestAction(RobotActionType action, float charge = 0f)
    {
        if (timingFeedback != null && timingFeedback.isActiveAndEnabled)
            timingFeedback.Perform(action, charge);
        else if (actionManager != null)
            actionManager.TryPerform(action, charge);
    }
    
}
