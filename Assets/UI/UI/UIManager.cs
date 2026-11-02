using System;
using System.Collections;
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

    [Header("Transitions")]
    [SerializeField] private CanvasGroup transitionCover;
    [SerializeField] private GameObject transitionLoadingLabel;
    [SerializeField, Min(0.01f)] private float transitionDuration = 0.32f;

    [Header("Swipe Trail")]
    [SerializeField] private Color trailColor = new Color(1f, 0.95f, 0.75f, 1f);
    [SerializeField] private Color trailGlowColor = new Color(1f, 0.74f, 0.33f, 0.35f);
    [SerializeField, Range(0f, 16f)] private float trailGlowSize = 7f;
    [SerializeField, Min(1f)] private float trailMarkSize = 25f;
    [SerializeField, Min(1f)] private float trailSpacing = 32f;
    [SerializeField, Min(0.01f)] private float trailFadeDuration = 0.2f;
    private SwipeTrailUI swipeTrail;

    public CanvasGroup[] menuContents;
    public GameObject sharedMenuBackground;
    public ComboDisplay comboDisplay;
    public GameObject countdownPanel;
    public Text countdownText;
    public Text countdownLabel;
    private float previousVolume;
    private Coroutine countdownRoutine;
    private CanvasGroup gameplayInput;
    private bool resultsBackground;
    private GameObject resultsBackdrop;
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
        countdownPanel.SetActive(false);
        gameplayInput = playHud.GetComponent<CanvasGroup>();
        if (gameplayInput == null) gameplayInput = playHud.AddComponent<CanvasGroup>();
        foreach (Graphic graphic in countdownPanel.GetComponentsInChildren<Graphic>(true))
            graphic.raycastTarget = false;
        var trailObject = new GameObject("Swipe Trail", typeof(RectTransform), typeof(CanvasRenderer), typeof(SwipeTrailUI));
        trailObject.transform.SetParent(playHud.transform, false);
        var trailRect = (RectTransform)trailObject.transform;
        trailRect.anchorMin = Vector2.zero;
        trailRect.anchorMax = Vector2.one;
        trailRect.offsetMin = trailRect.offsetMax = Vector2.zero;
        swipeTrail = trailObject.GetComponent<SwipeTrailUI>();
        swipeTrail.color = trailColor;
        swipeTrail.GlowColor = trailGlowColor;
        swipeTrail.GlowSize = trailGlowSize;
        gameManager.BindUI(this);
    }

    private void OpenMenuPage(UiPage page) => Navigate(() => gameManager.OpenMenuPage(page));
    public void OpenScoring() => Navigate(gameManager.OpenScoring);
    public void CloseScoring() => Navigate(gameManager.CloseScoring);
    public void OpenControls() => Navigate(gameManager.OpenControls);
    public void CloseControls() => Navigate(gameManager.CloseControls);

    public void OpenLevels() => OpenMenuPage(UiPage.Routes);
    public void CloseLevels() => OpenMenuPage(UiPage.Home);
    public void StartDesert() { gameManager.SelectLevel("Assets/Scenes/Levels/Desert Level NEW.unity"); StartGame(); }
    public void StartTutorial() { gameManager.SelectLevel("Assets/Scenes/Levels/Tutorial.unity"); StartGame(); }
    public void StartLevel(string scenePath)
    {
        if (IsTransitioning || gameManager.IsLoading) return;
        gameManager.SelectLevel(scenePath);
        StartGame();
    }

    public void StartGame()
    {
        Navigate(gameManager.StartGame, true);
    }
    public void ResumeGame() => Navigate(() =>
    {
        StartCountdown(true);
        gameManager.ResumeGame();
    });
    public void RestartGame() => Navigate(gameManager.RestartGame, true);
    public void ReturnToMenu() => Navigate(gameManager.ReturnToMenu, gameManager.HasActiveGame);
    public void ReturnToLevels() => Navigate(gameManager.ReturnToLevelSelect, gameManager.HasActiveGame);
    public void ExitGame()
    {
        if (!IsTransitioning) StartCoroutine(ExitAfterFade());
    }

    private IEnumerator ExitAfterFade()
    {
        yield return PlayExitTransition();
        PlayerPrefs.Save();
        Application.Quit();
    }

    public IEnumerator PlayExitTransition()
    {
        IsTransitioning = true;
        gameManager.SetUITransition(true);
        var music = FindFirstObjectByType<MenuMusicPreview>();
        if (music != null) music.FadeForExit();
        var fade = FindFirstObjectByType<StartupFade>(FindObjectsInactive.Include);
        if (fade != null) yield return fade.Close();
    }
    public void OpenSettings() => Navigate(gameManager.OpenOptions);
    public void CloseSettings() => Navigate(gameManager.CloseOptions);
    public void DismissError() => Navigate(gameManager.DismissError);

    public void PauseGame()
    {
        // Freeze immediately on touch; the overlay can then animate using unscaled time.
        if (!IsTransitioning && countdownRoutine == null)
            StartCoroutine(PauseTransition());
    }

    private IEnumerator PauseTransition()
    {
        // The pause state is entered immediately; only its overlay fades in.
        IsTransitioning = true;
        gameManager.SetUITransition(true);
        var group = pausePanel.GetComponent<CanvasGroup>();
        if (group == null) group = pausePanel.AddComponent<CanvasGroup>();
        group.alpha = 0;
        gameManager.PauseGame();
        float duration = menu.ReducedMotion ? 0f : .18f;
        for (float elapsed = 0; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.SmoothStep(0, 1, elapsed / duration);
            yield return null;
        }
        group.alpha = 1;
        IsTransitioning = false;
        gameManager.SetUITransition(false);
    }

    private void OnStateChanged()
    {
        ShowError();
        if (!(gameManager.CurrentState is GAMESTART))
            countdownPanel.SetActive(false);
        if (!IsTransitioning)
            StartCoroutine(RevealState());
    }

    private void Navigate(Action action, bool fullScreen = false)
    {
        if (!IsTransitioning && !gameManager.IsLoading)
            StartCoroutine(Transition(action, fullScreen));
    }

    private IEnumerator Transition(Action action, bool fullScreen = false)
    {
        IsTransitioning = true;
        gameManager.SetUITransition(true);
        if (!fullScreen)
        {
            yield return FadeContents(1, 0, .12f);
            action?.Invoke();
            while (gameManager.IsLoading) yield return null;
            yield return FadeContents(0, 1, .18f);
            IsTransitioning = false;
            gameManager.SetUITransition(false);
            yield break;
        }
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
        // The cover is gone before starting the full five-second preparation timer.
        if (gameManager.HasActiveGame)
            StartCountdown();
        IsTransitioning = false;
        gameManager.SetUITransition(false);
    }

    private void StartCountdown(bool resuming = false)
    {
        if (countdownRoutine != null) StopCoroutine(countdownRoutine);
        countdownLabel.text = resuming ? "RESUMING" : "GET READY";
        gameplayInput.interactable = false;
        gameManager.SetCountdown(true);
        countdownRoutine = StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        float remaining = 5f;
        while (gameManager.HasActiveGame && !gameManager.IsLoading && remaining > 0)
        {
            bool ready = gameManager.CurrentState is GAMESTART && !IsTransitioning;
            countdownPanel.SetActive(ready);
            if (ready)
            {
                countdownText.text = Mathf.CeilToInt(remaining).ToString();
                remaining -= Time.unscaledDeltaTime;
            }
            else
            {
                // Pause and its submenus hide the timer. Resume always starts at five.
                remaining = 5f;
            }
            yield return null;
        }
        countdownPanel.SetActive(false);
        countdownRoutine = null;
        gameplayInput.interactable = true;
        gameManager.SetCountdown(false);
    }

    private IEnumerator RevealState()
    {
        IsTransitioning = true;
        gameManager.SetUITransition(true);
        yield return FadeContents(0, 1, .18f);
        IsTransitioning = false;
        gameManager.SetUITransition(false);
    }

    private IEnumerator FadeContents(float from, float to, float duration)
    {
        if (menu.ReducedMotion) duration = .01f;
        for (float elapsed = 0; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            float alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, elapsed / duration));
            foreach (var group in menuContents)
                if (group) group.alpha = alpha;
            yield return null;
        }
        foreach (var group in menuContents)
            if (group) group.alpha = to;
    }

    private void LateUpdate()
    {
        if (sharedMenuBackground)
        {
            bool menuVisible = menu.GetComponent<Canvas>().enabled && menu.CurrentPageRect.gameObject.activeInHierarchy;
            sharedMenuBackground.SetActive(resultsBackground || !gameManager.HasActiveGame || menuVisible || (scoringPanel && scoringPanel.activeInHierarchy));
        }
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        var burst = transitionCover.GetComponentInChildren<LoadingBurst>();
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float value = Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, elapsed / duration));
            transitionCover.alpha = menu.ReducedMotion || burst == null ? value : 1;
            if (burst != null)
            {
                burst.Progress = menu.ReducedMotion ? 1 : value;
                burst.SetVerticesDirty();
            }
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        transitionCover.alpha = menu.ReducedMotion || burst == null ? to : 1;
        if (burst != null)
        {
            burst.Progress = menu.ReducedMotion ? 1 : to;
            burst.SetVerticesDirty();
        }
    }

    public void HideScreens()
    {
        resultsBackground = false;
        if (resultsBackdrop != null) resultsBackdrop.SetActive(false);
        menu.SetVisible(false);
        playHud.SetActive(false);
        pausePanel.SetActive(false);
        loadingPanel.SetActive(false);
        endPanel.SetActive(false);
        if (scoringPanel != null) scoringPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
    }

    // Common scene references stay here; states choose the visible screen.
    public void PrepareScreen(bool showResults)
    {
        HideScreens();
        bool inGame = gameManager.HasActiveGame;
        bool loading = gameManager.IsLoading;
        if (comboDisplay != null && (loading || !inGame)) comboDisplay.ClearCombo();
        menuCamera.SetActive(!inGame || loading);
        if (beatPulse != null && (!inGame || loading)) beatPulse.Bind(null);
        menuEventSystem.SetActive(!loading);
        if (scoreDisplay != null && (loading || !inGame)) scoreDisplay.Bind(null);
        if (scoreDisplay != null && !showResults) scoreDisplay.ShowResults(false);
        ShowError();
    }

    public void ShowMenu(UiPage page)
    {
        menu.SetVisible(true);
        menu.ShowPage(page);
    }

    public void ShowLevels()
    {
        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(true);
        else
            ShowMenu(UiPage.Routes);
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
        var source = scoreDisplay != null ? scoreDisplay.Source : null;
        var run = source != null ? source.GetComponent<LevelRunRecord>() : null;
        // Save before the transition so Retry cannot discard a completed run.
        var result = run != null ? run.Finish(source) : null;
        StartCoroutine(RevealResults(result));
    }

    private IEnumerator RevealResults(LevelRecords.UpdateResult result)
    {
        IsTransitioning = true;
        gameManager.SetUITransition(true);
        transitionCover.gameObject.SetActive(true);
        transitionCover.blocksRaycasts = true;
        transitionLoadingLabel.SetActive(false);
        float duration = menu.ReducedMotion ? .08f : transitionDuration;
        yield return Fade(0, 1, duration * .4f);
        resultsBackground = true;
        sharedMenuBackground.SetActive(true);
        if (resultsBackdrop == null)
        {
            // Keep the menu artwork in the results canvas, above every level camera.
            resultsBackdrop = Instantiate(sharedMenuBackground.transform.GetChild(0).gameObject, endPanel.transform.parent);
            resultsBackdrop.name = "Results Background";
            // The full-screen canvas already clips the pattern at the screen edges.
            var backgroundMask = resultsBackdrop.GetComponent<RectMask2D>();
            if (backgroundMask != null) Destroy(backgroundMask);
            resultsBackdrop.transform.SetSiblingIndex(endPanel.transform.GetSiblingIndex());
            foreach (Graphic graphic in resultsBackdrop.GetComponentsInChildren<Graphic>(true))
            {
                graphic.raycastTarget = false;
                if (graphic is MaskableGraphic masked) masked.maskable = false;
                graphic.canvasRenderer.DisableRectClipping();
            }
        }
        resultsBackdrop.SetActive(true);
        yield return Fade(1, 0, duration * .6f);
        transitionCover.gameObject.SetActive(false);

        var group = endPanel.GetComponent<CanvasGroup>();
        if (group == null) group = endPanel.AddComponent<CanvasGroup>();
        group.interactable = false;
        group.alpha = 0;
        Vector3 scale = endPanel.transform.localScale;
        endPanel.SetActive(true);
        if (scoreDisplay != null) scoreDisplay.ShowResults(true);
        var records = endPanel.GetComponent<LevelRecordResults>();
        if (records != null) records.Present(result);
        float popDuration = menu.ReducedMotion ? .08f : .25f;
        for (float time = 0; time < popDuration; time += Time.unscaledDeltaTime)
        {
            float progress = Mathf.SmoothStep(0, 1, time / popDuration);
            group.alpha = progress;
            if (!menu.ReducedMotion) endPanel.transform.localScale = scale * Mathf.Lerp(.92f, 1, progress);
            yield return null;
        }
        group.alpha = 1;
        endPanel.transform.localScale = scale;
        group.interactable = true;
        IsTransitioning = false;
        gameManager.SetUITransition(false);
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
        swipeTrail.Bind(null, trailMarkSize, trailSpacing, trailFadeDuration);
        // The menu owns touch input while it hosts a level. Keep the level's standalone setup intact.
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            InputManager input = root.GetComponentInChildren<InputManager>(true);
            if (input != null) swipeTrail.Bind(input, trailMarkSize, trailSpacing, trailFadeDuration);
            RhythmManager rhythm = root.GetComponentInChildren<RhythmManager>();
            if (beatPulse != null && rhythm != null) beatPulse.Bind(rhythm);
            foreach (EventSystem events in root.GetComponentsInChildren<EventSystem>(true))
            {
                foreach (BaseInputModule module in events.GetComponents<BaseInputModule>())
                    module.enabled = false;
                events.enabled = false;
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
                {
                    scoreDisplay.Bind(score);
                    var carousel = levelSelectPanel != null ? levelSelectPanel.GetComponentInChildren<LevelCarousel>(true) : null;
                    var run = score.GetComponent<LevelRunRecord>();
                    if (run == null) run = score.gameObject.AddComponent<LevelRunRecord>();
                    run.levelId = carousel != null ? carousel.RecordIdForScene(scene.path) : null;
                }
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
            gameManager.SetCountdown(false);
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
        if (countdownPanel != null) countdownPanel.SetActive(false);
        IsTransitioning = false;
        if (transitionCover != null)
            transitionCover.gameObject.SetActive(false);
    }
}
