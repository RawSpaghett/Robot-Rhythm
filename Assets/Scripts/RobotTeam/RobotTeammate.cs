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
        actionManager = GetComponent<ActionManager>();
    }

    private void OnDisable()
    {
        ObstacleBase.OnPerfectTiming -= PrepareQueue;
    }

    void Update()
    {
        if (actionQueue.Count != 0)
        {
            if (RhythmManager.Instance.SongTime >= actionQueue.Peek().dspTime)
            {
                ObstacleData nextAction = actionQueue.Dequeue();
                AutomateAction(nextAction);
            }
        }
    }
    
    

    private void PrepareQueue(ObstacleInputType Type, double DspTime)
    {
        //Debug.Log("Robot team PrepareQueue!");
        actionQueue.Enqueue(new ObstacleData { dspTime = DspTime + offset, type = Type }); //add to queue
    }
    
    private void AutomateAction(ObstacleData type)
    {
        
        switch(type.type)
        {
            case ObstacleInputType.Dodge:
                //Debug.Log($"Robot team Dodge!");
                actionManager.TryDuck();
                return;
            case ObstacleInputType.Jump:
                //Debug.Log($"Robot team Jump!");
                actionManager.TryJump();
                return;
        }
    }
    
}
