using TMPro;
using UnityEngine;

public class PotholeTimingTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer potholeRenderer;
    [SerializeField] private TMP_Text statusText;

    [Header("Placeholder Timing")]
    [SerializeField, Min(0.1f)] private float preparationTime = 2f;
    [SerializeField, Min(0.01f)] private float earlyWindow = 0.25f;
    [SerializeField, Min(0.01f)] private float lateWindow = 0.25f;
    [SerializeField, Min(0.1f)] private float resultDisplayTime = 2f;

    [Header("Placeholder Colors")]
    [SerializeField] private Color waitingColor = Color.yellow;
    [SerializeField] private Color windowColor = Color.cyan;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color failureColor = Color.red;

    private double targetTime;
    private double nextAttemptTime;
    private bool attemptActive;
    private bool initialized;
    private int attemptNumber;

    private void Start()
    {
        if (potholeRenderer == null || statusText == null)
        {
            Debug.LogError(
                "PotholeTimingTest needs a SpriteRenderer and status text.",
                this);

            enabled = false;
            return;
        }

        initialized = true;
        BeginAttempt();
    }

    private void Update()
    {
        if (!initialized || Time.timeScale <= 0f)
            return;

        double currentTime = Time.timeAsDouble;

        if (!attemptActive)
        {
            // Keep the result visible before starting another attempt.
            if (currentTime >= nextAttemptTime)
                BeginAttempt();

            return;
        }

        // Resolve a miss even when the player never presses Jump.
        if (currentTime > targetTime + lateWindow)
        {
            ResolveAttempt(false, "MISSED - no valid jump in time.");
            return;
        }

        RefreshPrompt(currentTime);
    }

    public void RegisterJumpAttempt()
    {
        // Ignore input after this attempt has already finished.
        if (!isActiveAndEnabled ||
            !initialized ||
            !attemptActive ||
            Time.timeScale <= 0f)
        {
            return;
        }

        double timingDifference = Time.timeAsDouble - targetTime;

        if (timingDifference < -earlyWindow)
        {
            ResolveAttempt(false, "TOO EARLY!");
        }
        else if (timingDifference > lateWindow)
        {
            ResolveAttempt(false, "TOO LATE!");
        }
        else
        {
            ResolveAttempt(true, "SUCCESS - correctly timed jump!");
        }
    }

    private void BeginAttempt()
    {
        attemptNumber++;
        attemptActive = true;

        // Keep a preparation period before the early window opens.
        float delay = Mathf.Max(
            preparationTime,
            earlyWindow + 0.1f);

        targetTime = Time.timeAsDouble + delay;

        RefreshPrompt(Time.timeAsDouble);
    }

    private void RefreshPrompt(double currentTime)
    {
        bool windowOpen = currentTime >= targetTime - earlyWindow;

        potholeRenderer.color =
            windowOpen ? windowColor : waitingColor;

        if (windowOpen)
        {
            statusText.text =
                $"Pothole attempt {attemptNumber}\nJUMP NOW!";
        }
        else
        {
            double secondsUntilWindow =
                targetTime - earlyWindow - currentTime;

            statusText.text =
                $"Pothole attempt {attemptNumber}\n" +
                $"Get ready... {secondsUntilWindow:0.0}s";
        }
    }

    private void ResolveAttempt(bool success, string message)
    {
        // Lock the result so repeated presses cannot resolve it again.
        attemptActive = false;

        potholeRenderer.color =
            success ? successColor : failureColor;

        statusText.text =
            $"Pothole attempt {attemptNumber}\n{message}";

        nextAttemptTime =
            Time.timeAsDouble + Mathf.Max(0.1f, resultDisplayTime);

        Debug.Log(
            $"Pothole attempt {attemptNumber}: {message}",
            this);
    }
}