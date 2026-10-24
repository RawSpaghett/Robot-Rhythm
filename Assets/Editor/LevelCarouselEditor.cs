using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelCarousel))]
public class LevelCarouselEditor : Editor
{
    private bool showReferences;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.HelpBox("Add a level with +, then assign its title, scene, cover image and preview song. Drag entries to change their order. Enable the listed scenes before building.", MessageType.Info);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("levels"), true);
        serializedObject.ApplyModifiedProperties();

        var carousel = (LevelCarousel)target;
        if (GUILayout.Button("Enable listed scenes in Build Settings"))
            EnableScenes(carousel);

        if (carousel.levels != null)
        {
            foreach (var level in carousel.levels)
            {
                if (level == null) continue;
                if (string.IsNullOrWhiteSpace(level.title) || level.cover == null || level.song == null)
                    EditorGUILayout.HelpBox("Each level needs a title, cover image and preview song.", MessageType.Warning);
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(level.scenePath) == null)
                    EditorGUILayout.HelpBox("Choose a valid scene for " + level.title + ".", MessageType.Error);
                else if (!EditorBuildSettings.scenes.Any(s => s.path == level.scenePath && s.enabled))
                    EditorGUILayout.HelpBox(level.title + " is not enabled in the build. Use the button above.", MessageType.Warning);
            }
        }

        showReferences = EditorGUILayout.Foldout(showReferences, "UI references and animation", true);
        if (showReferences)
        {
            DrawPropertiesExcluding(serializedObject, "m_Script", "levels");
            serializedObject.ApplyModifiedProperties();
        }
    }

    public static void EnableScenes(LevelCarousel carousel)
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        if (carousel.levels == null) return;
        foreach (var level in carousel.levels)
        {
            if (level == null || AssetDatabase.LoadAssetAtPath<SceneAsset>(level.scenePath) == null) continue;
            var existing = scenes.Find(s => s.path == level.scenePath);
            if (existing != null) existing.enabled = true;
            else scenes.Add(new EditorBuildSettingsScene(level.scenePath, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}

[CustomPropertyDrawer(typeof(LevelCarousel.Level))]
public class LevelChoiceDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position.height = EditorGUIUtility.singleLineHeight;
        float step = position.height + EditorGUIUtility.standardVerticalSpacing;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("title"), new GUIContent("Title"));
        position.y += step;
        var path = property.FindPropertyRelative("scenePath");
        var currentScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path.stringValue);
        EditorGUI.BeginChangeCheck();
        var chosenScene = (SceneAsset)EditorGUI.ObjectField(position, "Scene", currentScene, typeof(SceneAsset), false);
        if (EditorGUI.EndChangeCheck()) path.stringValue = AssetDatabase.GetAssetPath(chosenScene);
        position.y += step;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("cover"), new GUIContent("Cover image"));
        position.y += step;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("song"), new GUIContent("Preview song"));
        EditorGUI.EndProperty();
    }
}
