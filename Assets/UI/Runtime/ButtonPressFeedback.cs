using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RobotRhythm.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class ButtonPressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RectTransform[] visuals;
        [SerializeField] private UiController preferences;
        [SerializeField, Min(0f)] private float pressDistance = 4f;
        private Vector2[] positions;
        private Button button;
        private bool pressed;
        private bool inside;
        private int pointerId;
        private float amount;

        private void Awake()
        {
            button = GetComponent<Button>();
            positions = new Vector2[visuals.Length];
            for (int i = 0; i < visuals.Length; i++)
                positions[i] = visuals[i].anchoredPosition;
        }

        public void OnPointerDown(PointerEventData data)
        {
            if (pressed || !button.IsInteractable() || data.button != PointerEventData.InputButton.Left)
                return;
            pressed = inside = true;
            pointerId = data.pointerId;
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (data.pointerId == pointerId)
                pressed = false;
        }

        public void OnPointerEnter(PointerEventData data)
        {
            if (data.pointerId == pointerId)
                inside = true;
        }

        public void OnPointerExit(PointerEventData data)
        {
            if (data.pointerId == pointerId)
                inside = false;
        }

        private void Update()
        {
            bool reduced = preferences != null && preferences.ReducedMotion;
            float target = pressed && inside && button.IsInteractable() && !reduced ? 1f : 0f;
            amount = Mathf.MoveTowards(amount, target, Time.unscaledDeltaTime * 18f);
            for (int i = 0; i < visuals.Length; i++)
                if (visuals[i].GetComponent<Text>() == null)
                    visuals[i].anchoredPosition = positions[i] + new Vector2(pressDistance * 0.5f, -pressDistance) * amount;
        }

        private void OnDisable()
        {
            pressed = false;
            amount = 0f;
            if (positions == null)
                return;
            for (int i = 0; i < visuals.Length; i++)
                visuals[i].anchoredPosition = positions[i];
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused)
                pressed = false;
        }
    }
}
