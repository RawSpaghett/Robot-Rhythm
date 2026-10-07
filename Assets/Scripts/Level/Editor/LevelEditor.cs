using UnityEditor;
using UnityEngine;

// the level editor window
public class LevelEditor : EditorWindow
{
    // which level is open in the window, a link to the asset not a copy, anything typed here changes the real file
    private BeatMap beatMap;

    // how wide one beat is on the ruler
    private float pixelsPerBeat = 40f;

    // how far along the song the view has scrolled measured in beats
    private float scrollBeats;

    // Every obstacle type in the project and their names for a dropdown
    private ObstacleType[] obstacleTypes;
    private string[] obstacleTypeNames;

    // which name on the list is currently selected
    private int selectedTypeIndex;

    // adds this to the top menu bar and the slash makes a submenu
    [MenuItem("Robot Rhythm/Level Editor")]
    public static void Open()
    {
        GetWindow<LevelEditor>("Level Editor");
    }

    // called when the window is opened
    private void OnEnable()
    {
        LoadObstacleTypes();
    }

    // Unity calls this repeatedly to draw the window
    private void OnGUI()
    {
        beatMap = (BeatMap)EditorGUILayout.ObjectField("Beat Map", beatMap, typeof(BeatMap), false);

        if (beatMap == null)
        {
            return;
        }

        beatMap.SongID = EditorGUILayout.TextField("Song", beatMap.SongID);
        beatMap.Bpm = EditorGUILayout.FloatField("BPM", beatMap.Bpm);
        beatMap.FirstBeatOffset = EditorGUILayout.FloatField("First Beat Offset", beatMap.FirstBeatOffset);
        beatMap.BeatsPerMeasure = EditorGUILayout.IntField("Beats Per Measure", beatMap.BeatsPerMeasure);
        
        EditorGUILayout.BeginHorizontal();

        if (obstacleTypes != null && obstacleTypes.Length > 0)
        {
            selectedTypeIndex = Mathf.Clamp(selectedTypeIndex, 0, obstacleTypes.Length - 1);
            selectedTypeIndex = EditorGUILayout.Popup("Obstacle to Place", selectedTypeIndex, obstacleTypeNames);
        }

        if (GUILayout.Button("Refresh", GUILayout.Width(60f)))
        {
            LoadObstacleTypes();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Obstacles", beatMap.Obstacles.Count.ToString());

        // Takes a portion of the window for the timeline and GetRect hands this rectangle back so it can be drawn in
        Rect timeline = GUILayoutUtility.GetRect(position.width, 100f);

        // Adds a dark background to this^
        EditorGUI.DrawRect(timeline, new Color(0.1f, 0.1f, 0.2f));

        // how many beats fit across the ruler at the current zoom
        int visibleBeats = Mathf.CeilToInt(timeline.width / pixelsPerBeat);

        // guard against a zero in the inspector so it doesnt divide by 0
        int beatsPerMeasure = Mathf.Max(1, beatMap.BeatsPerMeasure);

        // start drawing from the first beat that is actually on screen
        int firstBeat = Mathf.FloorToInt(scrollBeats);

        // one line per beat spaced by pixels per beat
        for (int beat = firstBeat; beat <= firstBeat + visibleBeats; beat++)
        {
            float x = BeatToX(beat, timeline);

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

        // Draws the obstacles on the ruler
        foreach (ObstaclePlacement placement in beatMap.Obstacles)
        {
            // skips rows with no obstacles
            if (placement.Type == null)
            {
                continue;
            }

            Rect bar = GetBarRect(placement, timeline);
            EditorGUI.DrawRect(bar, placement.Type.TimelineColor);
        }

        HandleScroll(timeline);
        HandleClick(timeline);
        HandleDelete(timeline);
    }

    // Moves the view along the song with the mouse wheel
    private void HandleScroll(Rect timeline)
    {
        Event e = Event.current;

        if (e.type != EventType.ScrollWheel)
        {
            return;
        }

        if (!timeline.Contains(e.mousePosition))
        {
            return;
        }

        scrollBeats += e.delta.y;

        // stop the view running off the start of the song
        scrollBeats = Mathf.Max(0f, scrollBeats);

        e.Use();
        Repaint();
    }

    // Adds an obstacle where you click
    private void HandleClick(Rect timeline)
    {
        Event e = Event.current;

        // only react when a left button is clicked
        if (e.type != EventType.MouseDown || e.button != 0)
        {
            return;
        }

        if (!timeline.Contains(e.mousePosition))
        {
            return;
        }

        if (obstacleTypes == null || obstacleTypes.Length == 0)
        {
            return;
        }

        // turns click position into a beat, then rounds down
        float clickedBeat = XToBeat(e.mousePosition.x, timeline);
        float snappedBeat = Mathf.Floor(clickedBeat);

        ObstaclePlacement placement = new ObstaclePlacement();
        placement.Type = obstacleTypes[selectedTypeIndex];
        placement.Beat = Mathf.Max(0f, snappedBeat);

        // Snapshot the file before changing it so ctrl z can put it back
        Undo.RecordObject(beatMap, "Place Obstacle");
        beatMap.Obstacles.Add(placement);

        // Tells unity that the file changed so the edit gets written to disk
        EditorUtility.SetDirty(beatMap);

        // Marks click as handled so nothing else reacts to it
        e.Use();
        Repaint();
    }

    // Converts a beat number into an x position on the ruler
    private float BeatToX(float beat, Rect timeline)
    {
        return timeline.x + (beat - scrollBeats) * pixelsPerBeat;
    }

    // Converts an x position on the ruler back into a beat number
    private float XToBeat(float x, Rect timeline)
    {
        return scrollBeats + (x - timeline.x) / pixelsPerBeat;
    }

    // Works out the rectangle the obstacle is drawn in 
    private Rect GetBarRect(ObstaclePlacement placement, Rect timeline)
    {
        float barX = BeatToX(placement.Beat, timeline);
        float barWidth = placement.Type.WidthInBeats * pixelsPerBeat;
        return new Rect(barX, timeline.y + 20f, barWidth, timeline.height - 40f);
    }

    // Finds every obstacle type for the dropdown
    private void LoadObstacleTypes()
    {
        string[] guids = AssetDatabase.FindAssets("t:ObstacleType");

        obstacleTypes = new ObstacleType[guids.Length];
        obstacleTypeNames = new string[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);

            obstacleTypes[i] = AssetDatabase.LoadAssetAtPath<ObstacleType>(path);
            obstacleTypeNames[i] = obstacleTypes[i].DisplayName;
        }
    }

    // Right click deletes the obstacle
    private void HandleDelete(Rect timeline)
    {
        Event e = Event.current;

        if (e.type != EventType.MouseDown || e.button != 1)
        {
            return;
        }

        if (!timeline.Contains(e.mousePosition))
        {
            return;
        }

        ObstaclePlacement hit = FindObstacle(e.mousePosition, timeline);

        if (hit == null)
        {
            return;
        }

        Undo.RecordObject(beatMap, "Delete Obstacle");
        beatMap.Obstacles.Remove(hit);

        EditorUtility.SetDirty(beatMap);

        e.Use();
        Repaint();
    }
    
    // Finds the obstacle drawn under a point
    private ObstaclePlacement FindObstacle(Vector2 point, Rect timeline)
    {
        // goes backwards on the list so the whatever is on top is deleted
        for (int i = beatMap.Obstacles.Count - 1; i >= 0; i--)
        {
            ObstaclePlacement placement = beatMap.Obstacles[i];

            if (placement.Type == null)
            {
                continue;
            }

            if (GetBarRect(placement, timeline).Contains(point))
            {
                return placement;
            }
        }

        return null;
    }
}