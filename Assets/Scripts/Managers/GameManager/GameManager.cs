using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Loads scenes and runs the screen states. Each state decides what UI to show.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scene Flow")]
    [SerializeField] private string gameplayScenePath = "Assets/Scenes/PotholeTimingTest.unity";

    private StateMachineBase stateMachine;
    private GAMESTART gameStart;
    private GAMEEND gameEnd;
    private SCOREBOARD scoreBoard;
    private PAUSE pause;
    private OPTIONS options;
    private MAINMENU mainMenu;
    private LEVELSELECT levelSelect;
    private PACKAGESELECT packageSelect;
    private CONTROLS controls;
    private SCORING scoring;
    private LOADING loading;
    private bool statePausesGameplay;
    public UIManager UI { get; private set; }
    private GameSceneLoader sceneLoader;
    private bool holdsPause;
    private float timeScaleBeforePause;
    private bool audioPausedBeforePause;
    private bool pauseAfterLoading;

    public event Action StateChanged;
    public StateBase CurrentState => stateMachine?.currentState;
    public Scene GameplayScene => sceneLoader.GameplayScene;
    public bool HasActiveGame => sceneLoader != null && sceneLoader.HasActiveGame;
    public bool IsLoading { get; private set; }
    public bool IsUITransitioning { get; private set; }
    public string LastError { get; private set; } = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        sceneLoader = new GameSceneLoader(gameObject.scene);
        DontDestroyOnLoad(gameObject);
        gameStart = new GAMESTART(this);
        gameEnd = new GAMEEND(this);
        scoreBoard = new SCOREBOARD(this);
        pause = new PAUSE(this);
        options = new OPTIONS(this);
        mainMenu = new MAINMENU(this);
        levelSelect = new LEVELSELECT(this);
        packageSelect = new PACKAGESELECT(this);
        controls = new CONTROLS(this);
        scoring = new SCORING(this);
        loading = new LOADING(this);
        stateMachine = new StateMachineBase();
        stateMachine.Intialize(mainMenu);
    }

    public void StartGame()
    {
        if (IsLoading || HasActiveGame)
            return;
        if (CanLoadGame())
            StartCoroutine(ChangeScene(true));
    }

    public void RestartGame()
    {
        if (!IsLoading && HasActiveGame && CanLoadGame())
            StartCoroutine(ChangeScene(true));
    }

    public void ReturnToMenu()
    {
        if (!IsLoading && HasActiveGame)
            StartCoroutine(ChangeScene(false));
    }

    public void PauseGame()
    {
        if (!IsLoading && HasActiveGame && CurrentState == gameStart)
            ChangeState(pause);
    }

    public void ResumeGame()
    {
        if (!IsLoading && HasActiveGame && CurrentState == pause)
            ChangeState(gameStart);
    }

    public void BindUI(UIManager ui)
    {
        UI = ui;
        // The UI may finish starting after GameManager.Awake.
        CurrentState?.EnterState();
    }

    public void OpenMenuPage(RobotRhythm.UI.UiPage page)
    {
        if (IsLoading) return;
        switch (page)
        {
            case RobotRhythm.UI.UiPage.Home:
                if (CurrentState == controls) controls.Close();
                else if (CurrentState == options) options.Close();
                else ChangeState(HasActiveGame ? pause : mainMenu);
                break;
            case RobotRhythm.UI.UiPage.Routes:
                if (!HasActiveGame) ChangeState(levelSelect);
                break;
            case RobotRhythm.UI.UiPage.Placeholder:
                if (!HasActiveGame) ChangeState(packageSelect);
                break;
            case RobotRhythm.UI.UiPage.Controls: OpenControls(); break;
            case RobotRhythm.UI.UiPage.Settings: OpenOptions(); break;
        }
    }

    private UIStateBase MenuReturnState()
    {
        return CurrentState == gameStart ? pause : (UIStateBase)CurrentState;
    }

    public void OpenOptions()
    {
        if (!IsLoading && CurrentState != options) options.Open(MenuReturnState());
    }

    public void CloseOptions()
    {
        if (!IsLoading) options.Close();
    }

    public void OpenControls()
    {
        if (!IsLoading && CurrentState != controls) controls.Open(MenuReturnState());
    }

    public void CloseControls()
    {
        if (!IsLoading) controls.Close();
    }

    public void OpenScoring()
    {
        if (!IsLoading && CurrentState != scoring) scoring.Open(MenuReturnState());
    }

    public void CloseScoring()
    {
        if (!IsLoading) scoring.Close();
    }

    public void EndGame()
    {
        if (!IsLoading && HasActiveGame)
            ChangeState(gameEnd);
    }

    public void ShowScoreboard()
    {
        if (!IsLoading && HasActiveGame && CurrentState == gameEnd)
            ChangeState(scoreBoard);
    }

    public void DismissError()
    {
        LastError = "";
        StateChanged?.Invoke();
    }

    public void SetUITransition(bool active)
    {
        IsUITransitioning = active;
        UpdatePause();
    }

    public void SetStatePause(bool paused)
    {
        statePausesGameplay = paused;
        UpdatePause();
    }

    private void UpdatePause()
    {
        SetPaused(IsLoading || IsUITransitioning || (HasActiveGame && statePausesGameplay));
    }

    private bool CanLoadGame()
    {
        if (!string.IsNullOrEmpty(gameplayScenePath) && Application.CanStreamedLevelBeLoaded(gameplayScenePath))
            return true;
        LastError = "This level isn't available. Please try again later.";
        Debug.LogWarning("Gameplay scene must be enabled in Build Settings: " + gameplayScenePath, this);
        StateChanged?.Invoke();
        return false;
    }

    private IEnumerator ChangeScene(bool loadGame)
    {
        IsLoading = true;
        LastError = "";
        pauseAfterLoading = false;
        ChangeState(loading);
        yield return sceneLoader.ChangeScene(loadGame, gameplayScenePath);
        LastError = sceneLoader.LastError;

        IsLoading = false;
        ChangeState(HasActiveGame ? (pauseAfterLoading ? pause : gameStart) : mainMenu);
    }

    public void ChangeState(UIStateBase next)
    {
        if (next == null || CurrentState == next)
            return;
        stateMachine.ChangeState(next);
        UpdatePause();
        StateChanged?.Invoke();
    }

    private void SetPaused(bool value)
    {
        if (value == holdsPause)
            return;
        holdsPause = value;
        if (value)
        {
            timeScaleBeforePause = Time.timeScale;
            audioPausedBeforePause = AudioListener.pause;
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }
        else
        {
            Time.timeScale = timeScaleBeforePause;
            AudioListener.pause = audioPausedBeforePause;
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            if (IsLoading)
                pauseAfterLoading = true;
            PauseGame();
        }
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused)
        {
            if (IsLoading)
                pauseAfterLoading = true;
            PauseGame();
        }
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;
        SetPaused(false);
        Instance = null;
    }
}
