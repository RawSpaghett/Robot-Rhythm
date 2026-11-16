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
        return Line * 4f + Gap * 3f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        SerializedProperty sheet = property.FindPropertyRelative("sheet");
        SerializedProperty numericSort = property.FindPropertyRelative("sortByNumericName");
        SerializedProperty frames = property.FindPropertyRelative("frames");
        Rect row = new Rect(position.x, position.y, position.width, Line);

        // Editing one robot/prefab at a time avoids copying another robot's art
        // accidentally when the selected objects use different sheets.
        using (new EditorGUI.DisabledScope(property.serializedObject.isEditingMultipleObjects || Application.isPlaying))
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(row, sheet, label);
            row.y += Line + Gap;
            EditorGUI.PropertyField(row, numericSort, new GUIContent("Sort By Numeric Name",
                "Off: top edge first, then left to right. On: natural name order, e.g. frame_2 before frame_10."));
            // Rebuild only after an explicit assignment/toggle change, not on repaint.
            if (EditorGUI.EndChangeCheck()) CollectFrames(sheet, frames, numericSort.boolValue);

            row.y += Line + Gap;
            string status = frames.arraySize == 0
                ? "No frames: set Sprite/Multiple, slice, then Refresh."
                : frames.arraySize + (numericSort.boolValue
                    ? " frames: numeric name order."
                    : " frames: top row first, left to right.");
            EditorGUI.LabelField(row, status, EditorStyles.miniLabel);

            row.y += Line + Gap;
            if (GUI.Button(EditorGUI.IndentedRect(row), "Refresh Frames From Sheet"))
                CollectFrames(sheet, frames, numericSort.boolValue);
        }
        EditorGUI.EndProperty();
    }

    private static void CollectFrames(SerializedProperty sheetProperty, SerializedProperty framesProperty, bool sortByNumericName)
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

        if (sortByNumericName)
            sprites.Sort((a, b) => CompareNumericNames(a.name, b.name));
        else
        {
            // Deterministic reading order for grid sheets (including single rows).
            // Use slice rectangle top edges so equal-height grid rows stay together.
            sprites.Sort((a, b) =>
            {
                int row = b.rect.yMax.CompareTo(a.rect.yMax);
                if (row != 0) return row;
                int column = a.rect.xMin.CompareTo(b.rect.xMin);
                return column != 0 ? column : string.CompareOrdinal(a.name, b.name);
            });

        }

        // SerializedProperty keeps Undo and prefab override handling in the Inspector.
        framesProperty.arraySize = sprites.Count;
        for (int i = 0; i < sprites.Count; i++)
            framesProperty.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
    }
    // Compare digit groups numerically without parsing into a fixed-size integer.
    // This handles frame_2 vs frame_10, leading zeros and very large numbers.
    private static int CompareNumericNames(string a, string b)
    {
        int i = 0;
        int j = 0;
        while (i < a.Length && j < b.Length)
        {
            if (IsDigit(a[i]) && IsDigit(b[j]))
            {
                int endA = i;
                int endB = j;
                while (endA < a.Length && IsDigit(a[endA])) endA++;
                while (endB < b.Length && IsDigit(b[endB])) endB++;
                int startA = i;
                int startB = j;
                while (startA < endA && a[startA] == '0') startA++;
                while (startB < endB && b[startB] == '0') startB++;
                int lengthA = endA - startA;
                int lengthB = endB - startB;
                int comparison = lengthA.CompareTo(lengthB);
                if (comparison != 0) return comparison;
                for (int k = 0; k < lengthA; k++)
                {
                    comparison = a[startA + k].CompareTo(b[startB + k]);
                    if (comparison != 0) return comparison;
                }
                i = endA;
                j = endB;
            }
            else
            {
                int comparison = a[i].CompareTo(b[j]);
                if (comparison != 0) return comparison;
                i++;
                j++;
            }
        }
        int remaining = (a.Length - i).CompareTo(b.Length - j);
        // Deterministic tie-break for numerically equivalent names (e.g. 01 and 1).
        return remaining != 0 ? remaining : string.CompareOrdinal(a, b);
    }

    private static bool IsDigit(char value) => value >= '0' && value <= '9';
}
