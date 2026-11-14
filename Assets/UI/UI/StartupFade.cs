using System.Collections;
using UnityEngine;
using RobotRhythm.UI;

public class StartupFade : MonoBehaviour
{
    public UiController preferences;
    public StartupSunburst burst;
    private IEnumerator Start()
    {
        var group = GetComponent<CanvasGroup>();
        group.alpha = 1;
        burst.Progress = 0;
        yield return null;
        var menu = preferences.CurrentPageRect;
        Vector3 scale = menu.localScale;
        bool reduced = preferences.ReducedMotion;
        float duration = reduced ? .2f : 1.4f;
        for (float elapsed = 0; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            float progress = Mathf.Clamp01(elapsed / duration);
            if(reduced) group.alpha = 1-Mathf.SmoothStep(0,1,progress);
            else
            {
                burst.Progress = Mathf.SmoothStep(0,1,progress);
                burst.SetVerticesDirty();
                float pop = Mathf.Clamp01((progress-.1f)/.8f);
                menu.localScale = scale * (1-.2f*Mathf.Pow(1-pop,3)+.06f*Mathf.Sin(pop*Mathf.PI));
            }
            yield return null;
        }
        menu.localScale = scale;
        gameObject.SetActive(false);
    }
    public IEnumerator Close()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        var group = GetComponent<CanvasGroup>();
        group.alpha = 0;
        burst.Progress = 0;
        burst.SetVerticesDirty();
        float duration = preferences != null && preferences.ReducedMotion ? .2f : .9f;
        for(float elapsed=0;elapsed<duration;elapsed+=Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.SmoothStep(0,1,elapsed/duration);
            yield return null;
        }
        group.alpha = 1;
    }
}
