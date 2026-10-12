using UnityEngine;
using RobotRhythm.UI;

// Entrance and gentle idle motion for the supplied layered logo.
public class HandDrawnLogoEntrance : MonoBehaviour
{
    public UiController preferences;
    public RectTransform[] pieces;
    private float elapsed;
    private bool settled;

    private void OnEnable() { elapsed = 0; settled = false; }
    private void Update()
    {
        if (preferences != null && preferences.ReducedMotion) { Settle(); return; }
        elapsed += Time.unscaledDeltaTime;
        if (settled)
        {
            float idle = Mathf.Max(0, elapsed - 1.3f);
            float ramp = Mathf.SmoothStep(0, 1, Mathf.Clamp01(idle));
            float sway = Mathf.Sin(idle * 1.2f) * ramp;
            for (int i = 0; i < pieces.Length; i++)
            {
                pieces[i].anchoredPosition = new Vector2(0, sway * 6f);
                pieces[i].localRotation = Quaternion.Euler(0, 0, Mathf.Sin(idle * .9f) * 1.1f * ramp);
            }
            return;
        }
        for (int i = 0; i < pieces.Length; i++)
        {
            float t = Mathf.Clamp01((elapsed - i * .13f) / .85f);
            float remaining = 1f - t;
            float bounce = Mathf.Sin(t * Mathf.PI * 2f) * remaining * remaining;
            pieces[i].anchoredPosition = new Vector2(0, 48f * remaining * remaining + bounce * 15f);
            pieces[i].localScale = Vector3.one * (1f - .08f * remaining + .035f * bounce);
            pieces[i].localRotation = Quaternion.Euler(0, 0, (i % 2 == 0 ? -1 : 1) * 6f * remaining * remaining);
            var fade = pieces[i].GetComponent<CanvasGroup>();
            if (fade != null) fade.alpha = Mathf.Clamp01(t * 5f);
        }
        if (elapsed > 1.3f) Settle();
    }
    private void Settle()
    {
        foreach (var piece in pieces)
        {
            piece.anchoredPosition = Vector2.zero;
            piece.localScale = Vector3.one;
            piece.localRotation = Quaternion.identity;
            var fade = piece.GetComponent<CanvasGroup>();
            if (fade != null) fade.alpha = 1;
        }
        settled = true;
    }
}

