using UnityEngine;

//Singleton that handles Pause, Resume, End game, Start game
//Justin
public class GameManager: MonoBehaviour//GameManager.INSTANCE.ChangeState(state);
{
    #region Singleton Logic
    private static GameManager instance; //singleton

    private GameManager()
    {}
    public static GameManager Instance //intialize Singleton
    {
        get {
            if(instance==null) {
                instance = new GameManager();
            }
            return instance;
        }
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
        stateMachine = new StateMachineBase();
        gameStart = new GAMESTART();
        stateMachine.Intialize(gameStart);
    }
   
}
