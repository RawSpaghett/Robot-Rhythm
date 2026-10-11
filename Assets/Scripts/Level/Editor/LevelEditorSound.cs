using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Plays the the sound through the editor audio preview
public static class LevelEditorSound
{
    private static MethodInfo play;
    private static MethodInfo stop;
    private static MethodInfo isPlaying;
    private static MethodInfo getPosition;

    static LevelEditorSound()
    {
        Type audioUtil = typeof(Editor).Assembly.GetType("UnityEditor.AudioUtil");
        BindingFlags flags = BindingFlags.Static | BindingFlags.Public;

        play = audioUtil.GetMethod("PlayPreviewClip", flags);
        stop = audioUtil.GetMethod("StopAllPreviewClips", flags);
        isPlaying = audioUtil.GetMethod("IsPreviewClipPlaying", flags);
        getPosition = audioUtil.GetMethod("GetPreviewClipPosition", flags);
    }
    public static bool IsPlaying
    {
        get { return (bool)isPlaying.Invoke(null, null); }
    }
    
    // Where the song is in seconds
    public static float Position
    {
        get { return (float)getPosition.Invoke(null, null); }
    }

    public static void Play(AudioClip clip, int startSample)
    {
        if (clip == null)
        {
            return;
        }

        // Have to stop unity's audio inspector first or it will layer with each click
        Stop();
        play.Invoke(null, new object[] { clip, startSample, false});
    }

    public static void Stop()
    {
        stop.Invoke(null, null);
    }
}
