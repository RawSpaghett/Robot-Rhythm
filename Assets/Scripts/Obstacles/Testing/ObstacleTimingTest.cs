using TMPro;
using UnityEngine;

public class ObstacleTimingTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ObstacleBase obstacle;
    [SerializeField] private SpriteRenderer obstacleRenderer;
    [SerializeField] private TMP_Text statusText;

    [Header("Test Timing")]
    [SerializeField, Min(0.1f)] private float preparationTime = 2f;
    [SerializeField, Min(0.1f)] private float resultDisplayTime = 2f;

    [Header("Test Colors")]
    [SerializeField] private Color waitingColor = Color.yellow;
    [SerializeField] private Color perfectColor = Color.green;
    [SerializeField] private Color zeroAccuracyColor = Color.red;

    private bool initialized;
    private bool showingResult;

    private int attemptNumber;
    private double nextAttemptTime;
    private string resultDescription;

    private void Start()
    {
        if (obstacle == null ||
            obstacleRenderer == null ||
            statusText == null)
        {
            Debug.LogError(
                "ObstacleTimingTest needs an obstacle, renderer, and text.",
                this);

            enabled = false;
            return;
        }

        initialized = true;
        BeginAttempt();
    }

    private void OnEnable()
    {
        // Start fresh if this test controller is re-enabled.
        if (initialized)
            BeginAttempt();
    }

    private void Update()
    {
        if (!initialized || Time.timeScale <= 0f)
            return;

        // Do not keep evaluating a disabled obstacle.
        if (!obstacle.isActiveAndEnabled)
            return;

        double currentTime = Time.timeAsDouble;

        if (showingResult)
        {
            if (currentTime >= nextAttemptTime)
                BeginAttempt();

            return;
        }

        // Recover if the obstacle was canceled or disabled and re-enabled.
        if (!obstacle.IsActive && !obstacle.IsResolved)
        {
            BeginAttempt();
            return;
        }

        obstacle.CheckForMiss(currentTime);

        if (obstacle.IsResolved)
        {
            ShowResult(currentTime);
            return;
        }

        RefreshPrompt(currentTime);
    }

    public void RegisterJumpAttempt()
    {
        SubmitInput(ObstacleInputType.Jump, 0f);
    }

    public void RegisterLongJumpAttempt(float charge)
    {
        SubmitInput(ObstacleInputType.LongJump, charge);
    }

    // RELEASE EVENT CONNECTIONS:
    // Assign this test controller to each action's request event in the Inspector.
    // These methods are called immediately when GestureButtonInput executes the
    // matching action on release, not when the hold or swipe first begins.
    public void RegisterBrakeAttempt()
    {
        SubmitInput(ObstacleInputType.Brake, 0f);
    }

    public void RegisterAccelerateAttempt()
    {
        SubmitInput(ObstacleInputType.Accelerate, 0f);
    }

    public void RegisterDuckAttempt()
    {
        // The gesture is called Duck; the existing obstacle enum calls it Dodge.
        SubmitInput(ObstacleInputType.Dodge, 0f);
    }

    // FUTURE RHYTHM CONNECTION:
    // This is a single-obstacle test harness. In a full level, a rhythm/input
    // router should select the relevant active obstacle and submit the request.
    // Do not broadcast every input to every obstacle in the level.
    // Keep Prepare, EvaluateInput, and CheckForMiss on one shared clock.
    // This test uses Time.timeAsDouble; production music timing can replace it
    // in all three places together. Do not use gesture hold duration as beat time.
    private void SubmitInput(
        ObstacleInputType inputType,
        float charge)
    {
        if (!isActiveAndEnabled ||
            !initialized ||
            showingResult ||
            Time.timeScale <= 0f ||
            !obstacle.isActiveAndEnabled ||
            !obstacle.IsActive)
        {
            return;
        }

        double inputTime = Time.timeAsDouble;

        // The obstacle owns the judgment, not this test controller.
        obstacle.EvaluateInput(inputType, inputTime, charge);

        if (obstacle.IsResolved)
        {
            if (obstacle.WasCleared)
            {
                string side = inputTime < obstacle.TargetTime
                    ? "EARLY"
                    : "LATE";

                resultDescription = obstacle.WasPerfect
                    ? "PERFECT"
                    : $"SUCCESS - {side}";
            }

            ShowResult(inputTime);
        }
    }

    private void BeginAttempt()
    {
        if (!obstacle.isActiveAndEnabled)
            return;

        attemptNumber++;
        showingResult = false;
        resultDescription = "";

        float delay = Mathf.Max(
            preparationTime,
            obstacle.EarlyWindow + 0.1f);

        obstacle.Prepare(Time.timeAsDouble + delay);

        RefreshPrompt(Time.timeAsDouble);
    }

    private void RefreshPrompt(double currentTime)
    {
        double timeUntilTarget =
            obstacle.TargetTime - currentTime;

        float accuracy = obstacle.CalculateAccuracy(currentTime);

        bool windowOpen =
            currentTime >= obstacle.TargetTime - obstacle.EarlyWindow;

        obstacleRenderer.color = windowOpen
            ? Color.Lerp(waitingColor, perfectColor, accuracy)
            : waitingColor;

        if (timeUntilTarget > 0.0)
        {
            // Avoid displaying zero before the actual target.
            double countdown =
                System.Math.Ceiling(timeUntilTarget * 100.0) / 100.0;

            statusText.text =
                $"Obstacle attempt {attemptNumber}\n" +
                $"Release at 0: {countdown:0.00}s\n" +
                $"Current accuracy: {accuracy:0.000}";
        }
        else
        {
            statusText.text =
                $"Obstacle attempt {attemptNumber}\n" +
                $"Target passed by {-timeUntilTarget:0.000}s\n" +
                $"Current accuracy: {accuracy:0.000}";
        }
    }

    private void ShowResult(double currentTime)
    {
        if (showingResult)
            return;

        showingResult = true;

        if (!obstacle.WasCleared)
        {
            switch (obstacle.LastResult)
            {
                case ObstacleResult.TooEarly:
                    resultDescription = "TOO EARLY";
                    break;

                case ObstacleResult.TooLate:
                    resultDescription = "TOO LATE";
                    break;

                case ObstacleResult.WrongInput:
                    resultDescription = "WRONG INPUT";
                    break;

                case ObstacleResult.Missed:
                    resultDescription = "MISSED";
                    break;

                default:
                    resultDescription = "FAILED";
                    break;
            }
        }
        else if (string.IsNullOrEmpty(resultDescription))
        {
            resultDescription = obstacle.WasPerfect
                ? "PERFECT"
                : "SUCCESS";
        }

        obstacleRenderer.color = Color.Lerp(
            zeroAccuracyColor,
            perfectColor,
            obstacle.LastAccuracy);

        statusText.text =
            $"Obstacle attempt {attemptNumber}\n" +
            $"{resultDescription}\n" +
            $"Accuracy: {obstacle.LastAccuracy:0.000}";

        Debug.Log(
            $"Obstacle attempt {attemptNumber}: " +
            $"{resultDescription}, accuracy = " +
            $"{obstacle.LastAccuracy:0.000}",
            obstacle);

        nextAttemptTime =
            currentTime + Mathf.Max(0.1f, resultDisplayTime);
    }

    private void OnDisable()
    {
        if (obstacle != null)
            obstacle.CancelAttempt();
    }
}