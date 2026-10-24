using UnityEngine;

//Justin
//Plays sound effects

public class FXManager: MonoBehaviour
{
    [SerializeField]private AudioSource speakers;
    private float sourceVolume;
    public static FXManager Instance {get; private set;}

    void Awake()
    {
        IntializeInstance();
        speakers = GetComponent<AudioSource>();
        if (speakers != null) sourceVolume = speakers.volume;
        ApplyVolume();
    }

    void Update()
    {
        ApplyVolume();
    }

    private void ApplyVolume()
    {
        if (speakers != null)
            speakers.volume = sourceVolume * Mathf.Clamp01(PlayerPrefs.GetFloat("rr.effects", 1f));
    }

    public void PlaySFX(string SongID)
    {
        if (speakers == null)
        {
            Debug.LogError("No Speakers attatched! FXManager");
            return;
        }

        if (AudioFXDatabase.Instance != null && AudioFXDatabase.Instance.fxLibrary.ContainsKey(SongID))
        {
            ApplyVolume();
            speakers.PlayOneShot(AudioFXDatabase.Instance.GetClip(SongID));
        }
        
    }

    public void MuteAllSFX()
    {
        speakers.Stop();
    }

    public void ResumeSFX() 
    {
        speakers.UnPause();
    }

    public void PauseSFX()
    {
        speakers.Pause();
    }
    
    private void IntializeInstance()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // The level owns its song. Reloading the level starts a fresh clock.
    }


    
}
