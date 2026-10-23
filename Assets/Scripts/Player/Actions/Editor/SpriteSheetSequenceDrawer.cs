using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Keep this file inside an Editor folder. Only the imported Sprite references,
// not AssetDatabase calls, are used when playing a built game.
[CustomPropertyDrawer(typeof(SpriteSheetSequence))]
public class SpriteSheetSequenceDrawer : PropertyDrawer
{
    private static float Line => EditorGUIUtility.singleLineHeight;
    private static float Gap => EditorGUIUtility.standardVerticalSpacing;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return Line * 3f + Gap * 2f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        SerializedProperty sheet = property.FindPropertyRelative("sheet");
        SerializedProperty frames = property.FindPropertyRelative("frames");
        Rect row = new Rect(position.x, position.y, position.width, Line);

        // Editing one robot/prefab at a time avoids copying another robot's art
        // accidentally when the selected objects use different sheets.
        using (new EditorGUI.DisabledScope(property.serializedObject.isEditingMultipleObjects || Application.isPlaying))
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(row, sheet, label);
            if (EditorGUI.EndChangeCheck()) CollectFrames(sheet, frames);

            row.y += Line + Gap;
            string status = frames.arraySize == 0
                ? "No frames: set Sprite/Multiple, slice, then Refresh."
                : frames.arraySize + " frames: top row first, left to right.";
            EditorGUI.LabelField(row, status, EditorStyles.miniLabel);

            row.y += Line + Gap;
            if (GUI.Button(EditorGUI.IndentedRect(row), "Refresh Frames From Sheet"))
                CollectFrames(sheet, frames);
        }
        EditorGUI.EndProperty();
    }

    private static void CollectFrames(SerializedProperty sheetProperty, SerializedProperty framesProperty)
    {
        Texture2D sheet = sheetProperty.objectReferenceValue as Texture2D;
        var sprites = new List<Sprite>();
        if (sheet != null)
        {
            string path = AssetDatabase.GetAssetPath(sheet);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType == TextureImporterType.Sprite &&
                importer.spriteImportMode == SpriteImportMode.Multiple)
            {
                foreach (Object asset in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
                    if (asset is Sprite sprite) sprites.Add(sprite);
            }
        }

        // Deterministic reading order for grid sheets (including single rows).
        // Use slice rectangle top edges so equal-height grid rows stay together.
        sprites.Sort((a, b) =>
        {
            int row = b.rect.yMax.CompareTo(a.rect.yMax);
            if (row != 0) return row;
            int column = a.rect.xMin.CompareTo(b.rect.xMin);
            return column != 0 ? column : string.CompareOrdinal(a.name, b.name);
        });

        // SerializedProperty keeps Undo and prefab override handling in the Inspector.
        framesProperty.arraySize = sprites.Count;
        for (int i = 0; i < sprites.Count; i++)
            framesProperty.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
    }
}
