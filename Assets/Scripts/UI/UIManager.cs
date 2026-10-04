using System;
using System.Collections;
using System.Collections.Generic;
using RobotRhythm.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Game and Menu")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UiController menu;
    [SerializeField] private GameObject menuCamera;
    [SerializeField] private GameObject menuEventSystem;
    [SerializeField] private ScoreDisplay scoreDisplay;
    [SerializeField] private BeatPulse beatPulse;

    [Header("Overlays")]
    [SerializeField] private GameObject playHud;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private Text errorMessage;

    [SerializeField] private GameObject scoringPanel;
    [SerializeField] private GameObject levelSelectPanel;
    private bool showingLevels;
    private bool showingScoring;

    [Header("Transitions")]
    [SerializeField] private CanvasGroup transitionCover;
    [SerializeField] private GameObject transitionLoadingLabel;
    [SerializeField, Min(0.01f)] private float transitionDuration = 0.32f;

    private readonly List<GameObject> levelPads = new List<GameObject>();
    private bool showingControls;
    private float previousVolume;
    public bool IsTransitioning { get; private set; }

    private void OnEnable()
    {
        previousVolume = AudioListener.volume;
        if (gameManager == null || menu == null || menuCamera == null || menuEventSystem == null ||
            playHud == null || pausePanel == null || loadingPanel == null || endPanel == null ||
            errorPanel == null || errorMessage == null || transitionCover == null ||
            transitionLoadingLabel == null)
        {
            Debug.LogError("UIManager needs its menu and overlay references assigned.", this);
            enabled = false;
            return;
        }
        gameManager.StateChanged += OnStateChanged;
        menu.VolumeChanged += SetVolume;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        SetVolume(menu.UiVolume);
        transitionCover.gameObject.SetActive(false);
        ApplyView();
    }

    public void OpenScoring()
    {
        Navigate(() =>
        {
            if (gameManager.HasActiveGame) gameManager.PauseGame();
            showingScoring = true;
        });
    }

    public void CloseScoring() => Navigate(() => showingScoring = false);

    public void OpenControls()
    {
        Navigate(() => { gameManager.PauseGame(); showingControls = true; });
    }

    public void CloseControls()
    {
        Navigate(() => showingControls = false);
    }

    public void OpenLevels() => Navigate(() => showingLevels = true);
    public void CloseLevels() => Navigate(() => showingLevels = false);
    public void StartGame()
    {
        GetComponent<PracticeIntro>().Show(() => Navigate(() => { showingLevels = false; gameManager.StartGame(); }));
    }
    public void ResumeGame() => Navigate(gameManager.ResumeGame);
    public void RestartGame() => Navigate(gameManager.RestartGame);
    public void ReturnToMenu() => Navigate(gameManager.ReturnToMenu);
    public void OpenSettings() => Navigate(gameManager.OpenOptions);
    public void CloseSettings() => Navigate(gameManager.CloseOptions);
    public void DismissError() => Navigate(gameManager.DismissError);

    public void PauseGame()
    {
        // Freeze immediately on touch; the overlay can then animate using unscaled time.
        if (!IsTransitioning)
            gameManager.PauseGame();
    }

    private void OnStateChanged()
    {
        if (!IsTransitioning)
            StartCoroutine(Transition(null));
    }

    private void Navigate(Action action)
    {
        if (!IsTransitioning && !gameManager.IsLoading)
            StartCoroutine(Transition(action));
    }

    private IEnumerator Transition(Action action)
    {
        IsTransitioning = true;
        gameManager.SetUITransition(true);
        transitionCover.gameObject.SetActive(true);
        transitionCover.blocksRaycasts = true;
        transitionLoadingLabel.SetActive(false);
        float duration = menu.ReducedMotion ? 0.08f : transitionDuration;
        yield return Fade(0f, 1f, duration * 0.4f);

        action?.Invoke();
        ApplyView();
        transitionLoadingLabel.SetActive(gameManager.IsLoading);
        while (gameManager.IsLoading)
            yield return null;
        transitionLoadingLabel.SetActive(false);
        ApplyView();

        yield return Fade(1f, 0f, duration * 0.6f);
        transitionCover.gameObject.SetActive(false);
        IsTransitioning = false;
        gameManager.SetUITransition(false);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float progress = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, progress);
            transitionCover.alpha = Mathf.Lerp(from, to, eased);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        transitionCover.alpha = to;
    }

    private void ApplyView()
    {
        bool loading = gameManager.IsLoading;
        bool inGame = gameManager.HasActiveGame;
        bool options = gameManager.CurrentState is OPTIONS;
        bool paused = gameManager.CurrentState is PAUSE;
        bool ended = gameManager.CurrentState is GAMEEND || gameManager.CurrentState is SCOREBOARD;
        if (scoreDisplay != null)
        {
            if (loading || !inGame)
                scoreDisplay.Bind(null);
            scoreDisplay.ShowResults(inGame && ended && !loading);
        }
        if (loading || options || ended)
            showingControls = false;
        if (loading || ended) showingScoring = false;
        if (loading || inGame) showingLevels = false;
        if (levelSelectPanel != null) levelSelectPanel.SetActive(showingLevels && !loading);
        if (scoringPanel != null) scoringPanel.SetActive(showingScoring && !loading);
        bool showMenu = !showingLevels && !showingScoring && !loading && (options || showingControls || !inGame);
        menu.SetVisible(showMenu);
        if (showMenu)
            menu.ShowPage(options ? UiPage.Settings : showingControls ? UiPage.Controls : UiPage.Home);

        menuCamera.SetActive(!inGame || loading);
        if (beatPulse != null && (!inGame || loading)) beatPulse.Bind(null);
        menuEventSystem.SetActive(!loading);
        playHud.SetActive(inGame && !loading && !paused && !ended && !options && !showingControls && !showingScoring);
        pausePanel.SetActive(inGame && paused && !loading && !showingControls && !showingScoring);
        loadingPanel.SetActive(loading);
        endPanel.SetActive(inGame && ended && !loading);
        foreach (GameObject pad in levelPads)
            if (pad != null) pad.SetActive(inGame && !ended && !loading);
        errorMessage.text = gameManager.LastError;
        errorPanel.SetActive(!loading && !string.IsNullOrEmpty(gameManager.LastError));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!gameManager.IsLoading || mode != LoadSceneMode.Additive)
            return;
        menuCamera.SetActive(false);
        levelPads.Clear();
        // The menu owns touch input while it hosts a level. Keep the level's standalone setup intact.
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            RhythmManager rhythm = root.GetComponentInChildren<RhythmManager>();
            if (beatPulse != null && rhythm != null) beatPulse.Bind(rhythm);
            foreach (EventSystem events in root.GetComponentsInChildren<EventSystem>(true))
            {
                foreach (BaseInputModule module in events.GetComponents<BaseInputModule>())
                    module.enabled = false;
                events.enabled = false;
            }
            foreach (GesturePadFeedback feedback in root.GetComponentsInChildren<GesturePadFeedback>(true))
                feedback.SetPreferences(menu);
            foreach (TouchPadLayout layout in root.GetComponentsInChildren<TouchPadLayout>(true))
            {
                layout.Bind(menu);
                levelPads.Add(layout.gameObject);
            }
            foreach (HazardWarningUI warning in root.GetComponentsInChildren<HazardWarningUI>(true))
            {
                warning.SetPreferences(menu);
                if (beatPulse != null)
                    beatPulse.Follow(warning.SpeakerVisual, warning.WorldCamera);
            }
            if (scoreDisplay != null)
            {
                ScoreManager score = root.GetComponentInChildren<ScoreManager>();
                if (score != null)
                    scoreDisplay.Bind(score);
            }
        }
    }

    private void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.StateChanged -= OnStateChanged;
            gameManager.SetUITransition(false);
        }
        if (menu != null)
            menu.VolumeChanged -= SetVolume;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        AudioListener.volume = previousVolume;
        StopAllCoroutines();
        IsTransitioning = false;
        if (transitionCover != null)
            transitionCover.gameObject.SetActive(false);
    }
}
