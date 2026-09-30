using UnityEngine;

//Singleton that handles Pause, Resume, End game, Start game
//Justin
public class GameManager: MonoBehaviour//GameManager.INSTANCE.ChangeState(state);
{
    #region Singleton Logic
    public static GameManager Instance; //singleton
    private void IntializeInstance()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion
    
    #region States
    private StateMachineBase stateMachine;
    private GAMESTART gameStart;
    private GAMEEND gameEnd;
    private SCOREBOARD scoreBoard;
    private PAUSE pause;
    private OPTIONS options;
    #endregion

    void Awake()
    {
        IntializeInstance();
        stateMachine = new StateMachineBase();
        gameStart = new GAMESTART();
        stateMachine.Intialize(gameStart);
    }
   
}
