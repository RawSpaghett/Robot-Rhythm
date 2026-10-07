using UnityEngine;
using UnityEngine.UI;

// Attach to the charge Slider itself, not the button or Canvas.
[DisallowMultipleComponent]
[RequireComponent(typeof(Slider))]
public class ChargeIndicator : MonoBehaviour
{
    private Slider slider;

    private void Awake() => Initialize();

    private void Initialize()
    {
        if (slider != null) return;
        slider = GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
    }

    public void Show(float charge)
    {
        Initialize();
        gameObject.SetActive(true);
        slider.SetValueWithoutNotify(Mathf.Clamp01(charge));
    }

    public void Hide()
    {
        Initialize();
        slider.SetValueWithoutNotify(0f);
        gameObject.SetActive(false);
    }
}
