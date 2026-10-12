using UnityEngine;
using UnityEngine.UI;

public class HandDrawnVolumeFill : MonoBehaviour
{
    public Slider slider;
    public Image fill;
    private void OnEnable() { slider.onValueChanged.AddListener(Refresh); Refresh(slider.value); }
    private void OnDisable() { slider.onValueChanged.RemoveListener(Refresh); }
    private void Refresh(float value)
    {
        // The supplied brush image includes transparent padding on both sides.
        fill.fillAmount = value <= 0 ? 0 : Mathf.Lerp(.09f, .90f, value);
    }
}
