using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIFollowTarget : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Camera gameplayCamera;

    [Header("Position")]
    [Tooltip("Offset in UI units. Negative X places the UI to the left.")]
    [SerializeField] private Vector2 offset = new Vector2(-120f, 15f);

    private RectTransform uiRect;
    private RectTransform parentRect;
    private Canvas rootCanvas;

    private void Awake()
    {
        uiRect = GetComponent<RectTransform>();
        parentRect = transform.parent as RectTransform;

        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
            rootCanvas = canvas.rootCanvas;

        if (gameplayCamera == null)
            gameplayCamera = Camera.main;

        if (parentRect == null || rootCanvas == null)
        {
            Debug.LogError(
                "UIFollowTarget must be placed on a UI object under a Canvas.",
                this);

            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (target == null || gameplayCamera == null)
            return;

        // Locate the robot on the screen.
        Vector3 screenPosition =
            gameplayCamera.WorldToScreenPoint(target.position);

        // Overlay canvases do not use a camera for UI conversion.
        Camera uiCamera =
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : rootCanvas.worldCamera;

        if (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay &&
            uiCamera == null)
        {
            return;
        }

        // Convert to the slider parent's local UI coordinates.
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            new Vector2(screenPosition.x, screenPosition.y),
            uiCamera,
            out Vector2 localPoint))
        {
            Vector2 position = localPoint + offset;

            uiRect.localPosition =
                new Vector3(position.x, position.y, 0f);
        }
    }
}