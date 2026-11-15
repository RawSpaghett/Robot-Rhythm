using System.Collections;
using UnityEngine;
using RobotRhythm.UI;

public class StartupFade : MonoBehaviour
{
    public UiController preferences;
    public StartupSunburst burst;
    private void Awake()
    {
        // Block menu audio before its first Update, including during asset loading.
        StudioSplash.Prepare();
        GetComponent<CanvasGroup>().alpha = 1;
        burst.Progress = 0;
        burst.SetVerticesDirty();
    }
    private IEnumerator Start()
    {
        var group = GetComponent<CanvasGroup>();
        group.alpha = 1;
        burst.Progress = 0;
        yield return null;
        var studio = gameObject.AddComponent<StudioSplash>();
        yield return studio.Play(preferences, burst);
        Destroy(studio);
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
