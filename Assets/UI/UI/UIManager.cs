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
    public void StartDesert() { gameManager.SelectLevel("Assets/Scenes/Desert Radio Feelings Level.unity"); StartGame(); }
    public void StartTutorial() { gameManager.SelectLevel("Assets/Scenes/Tutorial.unity"); StartGame(); }
    public void StartGame()
    {
        Navigate(gameManager.StartGame, true);
    }
    public void ResumeGame() => Navigate(gameManager.ResumeGame);
    public void RestartGame() => Navigate(gameManager.RestartGame, true);
    public void ReturnToMenu() => Navigate(gameManager.ReturnToMenu);
    public void OpenSettings() => Navigate(gameManager.OpenOptions);
    public void CloseSettings() => Navigate(gameManager.CloseOptions);
    public void DismissError() => Navigate(gameManager.DismissError);

    public void PauseGame()
    {
        // Freeze immediately on touch; the overlay can then animate using unscaled time.
        if (!IsTransitioning)
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
        IsTransitioning = false;
        gameManager.SetUITransition(false);
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
            sharedMenuBackground.SetActive(!gameManager.HasActiveGame || menuVisible || (scoringPanel && scoringPanel.activeInHierarchy));
        }
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
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
    }

    // Common scene references stay here; states choose the visible screen.
    public void PrepareScreen(bool showResults)
    {
        HideScreens();
        bool inGame = gameManager.HasActiveGame;
        bool loading = gameManager.IsLoading;
        menuCamera.SetActive(!inGame || loading);
        if (beatPulse != null && (!inGame || loading)) beatPulse.Bind(null);
        menuEventSystem.SetActive(!loading);
        if (scoreDisplay != null && (loading || !inGame)) scoreDisplay.Bind(null);
        if (scoreDisplay != null) scoreDisplay.ShowResults(showResults);
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
        endPanel.SetActive(true);
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
