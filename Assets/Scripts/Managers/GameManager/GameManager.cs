using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Owns scene flow and pause state. UIManager presents the current state.
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
    private StateBase stateBeforeOptions;
    private Scene menuScene;
    private Scene gameplayScene;
    private bool holdsPause;
    private float timeScaleBeforePause;
    private bool audioPausedBeforePause;
    private bool pauseAfterLoading;

    public event Action StateChanged;
    public StateBase CurrentState => stateMachine?.currentState;
    public Scene GameplayScene => gameplayScene;
    public bool HasActiveGame => gameplayScene.IsValid() && gameplayScene.isLoaded;
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
        menuScene = gameObject.scene;
        DontDestroyOnLoad(gameObject);
        gameStart = new GAMESTART();
        gameEnd = new GAMEEND();
        scoreBoard = new SCOREBOARD();
        pause = new PAUSE();
        options = new OPTIONS();
        stateMachine = new StateMachineBase();
        stateMachine.Intialize(gameStart);
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

    public void OpenOptions()
    {
        if (IsLoading || CurrentState == options)
            return;
        // Closing settings during a run returns to Pause, never straight into play.
        stateBeforeOptions = HasActiveGame && CurrentState == gameStart ? pause : CurrentState;
        ChangeState(options);
    }

    public void CloseOptions()
    {
        if (!IsLoading && CurrentState == options)
            ChangeState(stateBeforeOptions ?? gameStart);
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

    private void UpdatePause()
    {
        SetPaused(IsLoading || IsUITransitioning || (HasActiveGame && CurrentState != gameStart));
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
        SetPaused(true);
        StateChanged?.Invoke();
        if (HasActiveGame)
        {
            if (menuScene.IsValid() && menuScene.isLoaded)
                SceneManager.SetActiveScene(menuScene);
            yield return SceneManager.UnloadSceneAsync(gameplayScene);
            gameplayScene = default;
        }

        if (loadGame)
        {
            AsyncOperation loading = null;
            try
            {
                loading = SceneManager.LoadSceneAsync(gameplayScenePath, LoadSceneMode.Additive);
            }
            catch (Exception error)
            {
                Debug.LogException(error, this);
            }
            if (loading != null)
            {
                yield return loading;
                gameplayScene = SceneManager.GetSceneByPath(gameplayScenePath);
            }
            if (HasActiveGame)
                SceneManager.SetActiveScene(gameplayScene);
            else
                LastError = "Couldn't open the level. Please try again.";
        }

        IsLoading = false;
        ChangeState(HasActiveGame && pauseAfterLoading ? pause : gameStart);
    }

    private void ChangeState(StateBase next)
    {
        if (CurrentState != next)
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
