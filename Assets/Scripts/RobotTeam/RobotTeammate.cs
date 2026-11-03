using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class RobotTeammate : MonoBehaviour
{
    [SerializeField] private string jumpID;
    [SerializeField] private string duckID;
    [SerializeField] private string accelerateID;
    [SerializeField] private string brakeID;

    [SerializeField] private double ChargeOffset = 0.5;
    
    private struct ObstacleData
    {
        public double dspTime;
        public ObstacleInputType type;
    }
    private Queue<ObstacleData> actionQueue = new Queue<ObstacleData>();

    [SerializeField] private double offset;
    [SerializeField] private ActionManager actionManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        ObstacleBase.OnPerfectTiming += PrepareQueue;
        actionQueue.Clear();
        actionManager = GetComponent<ActionManager>();
    }

    private void OnDisable()
    {
        ObstacleBase.OnPerfectTiming -= PrepareQueue;
    }

    private void Start() //for alignment, GEMINI ASSISTED
    {  
        ObstacleSpawner spawner = FindFirstObjectByType<ObstacleSpawner>();

        float distance = transform.position.x - spawner.hitLineX;
        float speed = spawner.distancePerBeat / (float)RhythmManager.Instance.beatMap.SecondsPerBeat;
        offset += -(distance / speed);
    }

    void Update()
    {
        //Gross i know
      while (actionQueue.Count > 0 && 
              ((RhythmManager.Instance.SongTime >= actionQueue.Peek().dspTime) || 
              ((actionQueue.Peek().type == ObstacleInputType.LongJump) && 
               (RhythmManager.Instance.SongTime >= actionQueue.Peek().dspTime - ChargeOffset))))
            {
                ObstacleData nextAction = actionQueue.Dequeue();
                AutomateAction(nextAction);
            }
    }
    
    

    private void PrepareQueue(ObstacleInputType Type, double DspTime)
    {
        actionQueue.Enqueue(new ObstacleData { dspTime = DspTime + offset, type = Type }); //add to queue
    }
    
    private void AutomateAction(ObstacleData type)
    {
    
        switch(type.type)
        {
            case ObstacleInputType.Dodge:
                //Debug.Log($"Robot team Dodge!");
                actionManager.TryDuck();
                FXManager.Instance.PlaySFX(duckID);
                return;
            case ObstacleInputType.Jump:
                //Debug.Log($"Robot team Jump!");
                actionManager.TryJump();
                FXManager.Instance.PlaySFX(jumpID);
                return;
            case ObstacleInputType.LongJump:
                //Debug.Log($"Robot team ChargeJump!");
                StartCoroutine(LongJumpWait(ChargeOffset));
                return;
            case ObstacleInputType.Accelerate:
                //Debug.Log($"Robot team Accelerate!");
                actionManager.TryAccelerate();
                FXManager.Instance.PlaySFX(accelerateID);
                return;
            case ObstacleInputType.Brake:
                //Debug.Log($"Robot team Brake!");
                actionManager.TryBrake();
                FXManager.Instance.PlaySFX(brakeID);
                return;
        }
    }

    private IEnumerator LongJumpWait(double time)
    {
            actionManager.SetLongJumpCharging(true);
            yield return new WaitForSeconds(Convert.ToSingle(time));
            actionManager.TryLongJump(10f);
            FXManager.Instance.PlaySFX(jumpID);
    }
    
}
