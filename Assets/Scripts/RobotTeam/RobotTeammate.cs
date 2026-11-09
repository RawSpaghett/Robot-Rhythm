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

    [Header("Player Result Reactions")]
    [Tooltip("Only react to obstacles spawned by this level's spawner. Auto-found if unassigned.")]
    [SerializeField] private ObstacleSpawner reactionSpawner;
    private bool isPreparingLongJump;
    
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
        ObstacleBase.OnObstacleResolved += ReactToPlayerResult;
        actionQueue.Clear();
        isPreparingLongJump = false;
        actionManager = GetComponent<ActionManager>();
    }

    private void OnDisable()
    {
        ObstacleBase.OnPerfectTiming -= PrepareQueue;
        ObstacleBase.OnObstacleResolved -= ReactToPlayerResult;
        // Do not leave a charging coroutine or reaction alive on a disabled teammate.
        StopAllCoroutines();
        isPreparingLongJump = false;
        if (actionManager != null)
        {
            actionManager.SetLongJumpCharging(false);
            actionManager.CancelReaction();
        }
    }

    private void Start() //for alignment, GEMINI ASSISTED
    {  
        if (reactionSpawner == null) reactionSpawner = FindFirstObjectByType<ObstacleSpawner>();
        ObstacleSpawner spawner = reactionSpawner;

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
    
    

    private void ReactToPlayerResult(ObstacleBase obstacle, ObstacleResult result, float accuracy)
    {
        if (actionManager == null || reactionSpawner == null || obstacle == null ||
            !obstacle.transform.IsChildOf(reactionSpawner.transform) || isPreparingLongJump)
            return;
        if (result == ObstacleResult.None) return;

        // If an action/charge is already due this frame, do not flash a reaction
        // before Update processes it. Future actions can interrupt an active reaction.
        RhythmManager rhythm = RhythmManager.Instance;
        if (rhythm == null || !rhythm.isPlaying) return;
        if (actionQueue.Count > 0)
        {
            ObstacleData next = actionQueue.Peek();
            double dueTime = next.type == ObstacleInputType.LongJump
                ? System.Math.Min(next.dspTime, next.dspTime - ChargeOffset) : next.dspTime;
            if (rhythm.SongTime >= dueTime) return;
        }

        RobotTimingVisual visual = result == ObstacleResult.Success
            ? (obstacle.WasPerfect ? RobotTimingVisual.Perfect : RobotTimingVisual.EarlyLate)
            : RobotTimingVisual.Failed;
        // Busy robots skip this reaction; old reactions are never queued for later.
        actionManager.ShowReaction(visual);
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
            isPreparingLongJump = true;
            actionManager.CancelReaction();
            actionManager.SetLongJumpCharging(true);
            yield return new WaitForSeconds(Convert.ToSingle(time));
            actionManager.TryLongJump(10f);
            isPreparingLongJump = false;
            FXManager.Instance.PlaySFX(jumpID);
    }
    
}
