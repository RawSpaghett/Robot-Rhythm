using UnityEngine;
using System.Collections.Generic;

//playing, pausing, resuming music
//Justin

public class MusicManager: MonoBehaviour
{
    #region Singleton Logic

    private static MusicManager instance; //singleton

    private MusicManager()
    {}
    public static MusicManager Instance //intialize Singleton
    {
        get {
            if(instance==null) {
                instance = new MusicManager();
            }
            return instance;
        }
    }
    #endregion
    private Dictionary<string, AudioClip> musicLibrary = new Dictionary<string, AudioClip>();

    public void IntializeMusic(string musicID)
    {}

    public void PauseMusic()
    {}

    public void ResumeMusic()
    {}


}
