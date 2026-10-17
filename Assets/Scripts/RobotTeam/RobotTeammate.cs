using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;

public class RobotTeammate : MonoBehaviour
{
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
            while (actionQueue.Count > 0 && RhythmManager.Instance.SongTime >= actionQueue.Peek().dspTime)
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
                FXManager.Instance.PlaySFX("duck");
                return;
            case ObstacleInputType.Jump:
                //Debug.Log($"Robot team Jump!");
                actionManager.TryJump();
                FXManager.Instance.PlaySFX("jump");
                return;
            case ObstacleInputType.LongJump:
                actionManager.TryLongJump(7f);
                FXManager.Instance.PlaySFX("jump");
                return;
        }
    }
    
}
