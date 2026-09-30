using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class StateMachineBase
{
    public StateBase currentState {get;set;}
  
    public void Intialize(StateBase intialState)
    {
        Debug.Log("EStateMachine Intialized");
        currentState = intialState;
        currentState.EnterState();
    }

    public void ChangeState(StateBase newState)
    {
        currentState.ExitState();
        currentState = newState;
        Debug.Log($"State changed: {newState}");
        currentState.EnterState();
    }
}