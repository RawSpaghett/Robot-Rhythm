using System.Collections;
using RobotRhythm.UI;
using UnityEngine;
using UnityEngine.UI;

public class LevelRecordResults : MonoBehaviour
{
    public Text announcement, bestScore, bestAccuracy;
    public GameObject ribbon;
    public PerfectConfetti confetti;
    public RectTransform scoreNumber;
    public UiController preferences;
    public AudioClip perfectSound, personalBestSound;
    private AudioSource celebrationAudio;
    private Vector3 normalScoreScale;
    private Vector3 normalRibbonScale;

    void Awake()
    {
        if(scoreNumber!=null) normalScoreScale=scoreNumber.localScale;
        if(ribbon!=null) normalRibbonScale=ribbon.transform.localScale;
    }

    public void Present(LevelRecords.UpdateResult result)
    {
        StopAllCoroutines();
        ResetMotion();
        ribbon.SetActive(result != null && result.perfectRun);
        bool hasRecord = result != null && result.record != null && result.record.completed;
        SetRecordCard(bestScore, hasRecord, "BEST SCORE", hasRecord ? result.record.bestScore.ToString() : "");
        SetRecordCard(bestAccuracy, hasRecord, "BEST ACCURACY", hasRecord ? Accuracy(result.record.bestAccuracy) : "");
        if(result == null || !result.saved)
        {
            announcement.text = "";
            return;
        }
        announcement.text = result.perfectRun ? "PERFECT DELIVERY!" : result.firstClear ? "" :
            result.newScore && result.newAccuracy ? "NEW BEST SCORE + ACCURACY!" :
            result.newScore ? "NEW BEST SCORE!" : result.newAccuracy ? "NEW BEST ACCURACY!" : "";
        bool personalBest = !result.firstClear && (result.newScore || result.newAccuracy);
        if(result.perfectRun || personalBest) StartCoroutine(Celebrate(result.perfectRun));
    }

    public static void SetRecordCard(Text label, bool visible, string heading, string value)
    {
        if (label == null) return;
        label.transform.parent.gameObject.SetActive(visible);
        label.text = visible ? RecordText(heading, value) : "";
    }

    public static string RecordText(string label, string value)
    {
        return "<size=12><color=#805B24>" + label + "</color></size>\n<size=21>" + value + "</size>";
    }

    public static string Accuracy(float value)
    {
        // Do not display a near-perfect run as a perfect 100%.
        return value>=1f?"100%":(Mathf.Floor(value*10000)/100).ToString("0.##",System.Globalization.CultureInfo.InvariantCulture)+"%";
    }

    IEnumerator Celebrate(bool perfect)
    {
        while(GameManager.Instance != null && GameManager.Instance.IsUITransitioning) yield return null;
        if(celebrationAudio == null)
        {
            celebrationAudio = gameObject.AddComponent<AudioSource>();
            celebrationAudio.playOnAwake = false;
            celebrationAudio.spatialBlend = 0;
            celebrationAudio.ignoreListenerPause = true;
            celebrationAudio.ignoreListenerVolume = true;
        }
        celebrationAudio.clip = perfect ? perfectSound : personalBestSound;
        celebrationAudio.volume = preferences != null ? preferences.UiVolume * preferences.EffectsVolume * .65f : .65f;
        if(celebrationAudio.clip != null) celebrationAudio.Play();
        if(confetti != null) confetti.Prepare(perfect);
        for(float time=0;time<7f;time+=Time.unscaledDeltaTime)
        {
            bool reduced = preferences != null && preferences.ReducedMotion;
            if(celebrationAudio != null && preferences != null)
                celebrationAudio.volume = preferences.UiVolume * preferences.EffectsVolume * .65f;
            if(confetti != null) { confetti.Progress=reduced ? 0 : time/7f; confetti.SetVerticesDirty(); }
            float age = Mathf.Max(0,time-1.1f);
            float pulse = reduced ? 1 : 1+Mathf.Sin(age*8)*(perfect?.12f:.07f)*Mathf.Exp(-age*2);
            if(scoreNumber!=null) scoreNumber.localScale=normalScoreScale*pulse;
            if(ribbon!=null && perfect)
            {
                float ribbonAge = Mathf.Max(0,time-1.35f);
                float reveal = reduced ? 1 : 1-Mathf.Exp(-ribbonAge*7)*Mathf.Cos(ribbonAge*10);
                ribbon.transform.localScale=normalRibbonScale*reveal;
            }
            yield return null;
        }
        ResetMotion();
    }

    void ResetMotion()
    {
        if(celebrationAudio!=null) celebrationAudio.Stop();
        if(ribbon!=null) ribbon.transform.localScale=normalRibbonScale==Vector3.zero?Vector3.one:normalRibbonScale;
        if(confetti!=null) { confetti.Progress=0; confetti.SetVerticesDirty(); }
        if(scoreNumber!=null) scoreNumber.localScale=normalScoreScale==Vector3.zero?Vector3.one:normalScoreScale;
    }
    void OnDisable() { StopAllCoroutines(); ResetMotion(); }
}
