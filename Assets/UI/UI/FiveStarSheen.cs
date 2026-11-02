using RobotRhythm.UI;
using UnityEngine;

// Gives a full five-star result a pearly finish without changing the star thresholds.
public class FiveStarSheen : MonoBehaviour
{
    public RatingStar[] stars;
    public Material finish;
    public UiController preferences;
    private Material instance;
    private float sheenTime;
    public bool IsActive { get; private set; }

    void Awake()
    {
        if (finish != null) instance = new Material(finish);
    }

    void LateUpdate()
    {
        bool full = stars != null && stars.Length == 5 && instance != null;
        if (full)
            foreach (var star in stars)
                if (star == null || star.FillAmount < 1f) full = false;

        IsActive = full;
        if (stars == null) return;
        foreach (var star in stars)
            if (star != null) star.material = full ? instance : null;

        if (!full) return;
        bool reduced = preferences != null && preferences.ReducedMotion;
        if (!reduced) sheenTime += Time.unscaledDeltaTime;
        instance.SetFloat("_SheenTime", reduced ? 0f : sheenTime);
    }

    void OnDisable()
    {
        IsActive = false;
        sheenTime = 0;
        if (stars != null)
            foreach (var star in stars)
                if (star != null) star.material = null;
    }

    void OnDestroy()
    {
        if (instance != null) Destroy(instance);
    }
}
