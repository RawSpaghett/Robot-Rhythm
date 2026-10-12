using UnityEngine;
using RobotRhythm.UI;

[RequireComponent(typeof(AudioSource))]
public class MenuMusicPreview : MonoBehaviour
{
    public UiController preferences;
    public GameObject settingsPage;
    public AudioClip tutorialSong;
    private AudioSource source;
    private float envelope;
    private const float FadeSeconds = .8f;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.clip = tutorialSong;
        source.loop = true;
        source.playOnAwake = false;
        source.ignoreListenerPause = true;
        source.ignoreListenerVolume = true;
        source.volume = 0;
    }

    private void Update()
    {
        var game = GameManager.Instance;
        bool settingsVisible = preferences.GetComponent<Canvas>().enabled && settingsPage.activeInHierarchy;
        bool audible = game != null && !game.IsLoading && (!game.HasActiveGame || settingsVisible);
        envelope = Mathf.MoveTowards(envelope, audible ? 1 : 0, Time.unscaledDeltaTime / FadeSeconds);
        source.volume = Mathf.SmoothStep(0, 1, envelope) * preferences.UiVolume * preferences.MusicVolume;
        if (audible && !source.isPlaying) source.Play();
        if (!audible && envelope == 0 && source.isPlaying) source.Stop();
    }
}
