using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class JumpButtonInput : MonoBehaviour, IPointerDownHandler
{
    [Header("Jump Request")]
    [SerializeField]
    private UnityEvent onJumpRequested =
        new UnityEvent();

    private Button jumpButton;

    private void Awake()
    {
        jumpButton = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Accept primary mouse presses and normal touch presses.
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        // Respect component state, button state, and time-scale pause.
        if (!isActiveAndEnabled ||
            !jumpButton.IsInteractable() ||
            Time.timeScale <= 0f)
        {
            return;
        }

        // Notify the connected gameplay behavior.
        onJumpRequested.Invoke();
    }
}