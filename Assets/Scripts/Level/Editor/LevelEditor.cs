using UnityEditor;
using UnityEngine;

// the level editor window
public class LevelEditor : EditorWindow
{
    // which level is open in the window, a link to the asset not a copy, anything typed here changes the real file
    private BeatMap beatMap;

    // how wide one beat is
    private float pixelsPerBeat = 40f;

    // adds this to the top menu bar and the slash makes a submenu
    [MenuItem("Robot Rhythm/Level Editor")]
    public static void Open()
    {
        GetWindow<LevelEditor>("Level Editor");
    }

    // Unity calls this repeatedly to draw the window
    private void OnGUI()
    {
        beatMap = (BeatMap)EditorGUILayout.ObjectField("Beat Map", beatMap, typeof(BeatMap), false);

        if (beatMap == null)
        {
            return;
        }

        beatMap.Song = (AudioClip)EditorGUILayout.ObjectField("Song", beatMap.Song, typeof(AudioClip), false);
        beatMap.Bpm = EditorGUILayout.FloatField("BPM", beatMap.Bpm);
        beatMap.FirstBeatOffset = EditorGUILayout.FloatField("First Beat Offset", beatMap.FirstBeatOffset);
        beatMap.BeatsPerMeasure = EditorGUILayout.IntField("Beats Per Measure", beatMap.BeatsPerMeasure);

        EditorGUILayout.LabelField("Obstacles", beatMap.Obstacles.Count.ToString());

        // Takes a portion of the window for the timeline and GetRect hands this rectangle back so it can be drawn in
        Rect timeline = GUILayoutUtility.GetRect(position.width, 100f);

        // Adds a dark background to this^
        EditorGUI.DrawRect(timeline, new Color(0.1f, 0.1f, 0.2f));

        // how many beats fit across the ruler at the current zoom
        int visibleBeats = Mathf.CeilToInt(timeline.width / pixelsPerBeat);

        // guard against a zero in the inspector so it doesnt divide by 0
        int beatsPerMeasure = Mathf.Max(1, beatMap.BeatsPerMeasure);

        // one line per beat spaced by pixels per beat
        for (int beat = 0; beat <= visibleBeats; beat++)
        {
            float x = timeline.x + beat * pixelsPerBeat;

            // first beat of each bar is a lil bigger
            bool isMeasureStart = beat % beatsPerMeasure == 0;

            float height;
            Color color;

            if (isMeasureStart)
            {
                height = timeline.height;
                color = new Color(1f, 1f, 1f, 0.5f);
            }
            else
            {
                height = timeline.height * 0.6f;
                color = new Color(1f, 1f, 1f, 0.3f);
            }

            Rect line = new Rect(x, timeline.y, 1f, height);
            EditorGUI.DrawRect(line, color);

            if (isMeasureStart)
            {
                // bars numbered from 1
                int measureNumber = beat / beatsPerMeasure + 1;

                GUI.Label(new Rect(x + 3f, timeline.y, 40f, 16f), measureNumber.ToString(), EditorStyles.miniLabel);
            }
        }
    }
}
