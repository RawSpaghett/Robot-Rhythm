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

    [Header("Overlays")]
    [SerializeField] private GameObject playHud;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private Text errorMessage;

    [SerializeField] private GameObject scoringPanel;

    [Header("Transitions")]
    [SerializeField] private CanvasGroup transitionCover;
    [SerializeField] private GameObject transitionLoadingLabel;
    [SerializeField, Min(0.01f)] private float transitionDuration = 0.32f;

    private readonly List<GameObject> levelPads = new List<GameObject>();
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
        menu.PageRequested += OpenMenuPage;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        SetVolume(menu.UiVolume);
        transitionCover.gameObject.SetActive(false);
        gameManager.BindUI(this);
    }

    private void OpenMenuPage(UiPage page) => Navigate(() => gameManager.OpenMenuPage(page));
    public void OpenScoring() => Navigate(gameManager.OpenScoring);
    public void CloseScoring() => Navigate(gameManager.CloseScoring);
    public void OpenControls() => Navigate(gameManager.OpenControls);
    public void CloseControls() => Navigate(gameManager.CloseControls);

    public void StartGame() => Navigate(gameManager.StartGame);
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
        ShowError();
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
        transitionLoadingLabel.SetActive(gameManager.IsLoading);
        while (gameManager.IsLoading)
            yield return null;
        transitionLoadingLabel.SetActive(false);

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

    public void HideScreens()
    {
        menu.SetVisible(false);
        playHud.SetActive(false);
        pausePanel.SetActive(false);
        loadingPanel.SetActive(false);
        endPanel.SetActive(false);
        if (scoringPanel != null) scoringPanel.SetActive(false);
    }

    // Common scene references stay here; states choose the visible screen.
    public void PrepareScreen(bool showResults)
    {
        HideScreens();
        bool inGame = gameManager.HasActiveGame;
        bool loading = gameManager.IsLoading;
        menuCamera.SetActive(!inGame || loading);
        menuEventSystem.SetActive(!loading);
        foreach (GameObject pad in levelPads)
            if (pad != null) pad.SetActive(inGame && !loading);
        if (scoreDisplay != null && (loading || !inGame)) scoreDisplay.Bind(null);
        if (scoreDisplay != null) scoreDisplay.ShowResults(showResults);
        ShowError();
    }

    public void ShowMenu(UiPage page)
    {
        menu.SetVisible(true);
        menu.ShowPage(page);
    }

    public void ShowGameplay() => playHud.SetActive(true);
    public void ShowPause() => pausePanel.SetActive(true);
    public void ShowLoading() => loadingPanel.SetActive(true);

    public void ShowScoring()
    {
        if (scoringPanel != null) scoringPanel.SetActive(true);
    }

    public void ShowResults()
    {
        endPanel.SetActive(true);
        foreach (GameObject pad in levelPads)
            if (pad != null) pad.SetActive(false);
    }

    private void ShowError()
    {
        errorMessage.text = gameManager.LastError;
        errorPanel.SetActive(!gameManager.IsLoading && !string.IsNullOrEmpty(gameManager.LastError));
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
                warning.SetPreferences(menu);
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
        {
            menu.VolumeChanged -= SetVolume;
            menu.PageRequested -= OpenMenuPage;
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
        AudioListener.volume = previousVolume;
        StopAllCoroutines();
        IsTransitioning = false;
        if (transitionCover != null)
            transitionCover.gameObject.SetActive(false);
    }
}
