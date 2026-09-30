using UnityEngine;

//playing audio cues, scoring audio, etc
//Justin

public class FXAudioManager: MonoBehaviour
{
    private AudioSource speakers;
    private RhythmManager rhythmManager;


    void Awake()
    {
        rhythmManager = RhythmManager.Instance;
        speakers = GetComponent<AudioSource>();
    }

    public void PlayEffect(string FXId, float Volume)
    {
        var fx = AudioFXDatabase.Instance.GetClip(FXId);
        if (fx != null)
        {
            speakers.Stop();
            speakers.PlayOneShot(fx,Volume);
        }
    }
}
