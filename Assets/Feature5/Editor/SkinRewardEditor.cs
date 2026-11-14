using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RobotRhythm.Feature5
{
    public static class SkinRewardEditor
    {
        private static SkinLibrary Library => AssetDatabase.LoadAssetAtPath<SkinLibrary>("Assets/Feature5/SkinLibrary.asset");

        public static void Draw(Rect position, SerializedProperty level)
        {
            var library = Library;
            if (library == null) return;
            string id = level.FindPropertyRelative("recordId").stringValue;
            string title = level.FindPropertyRelative("title").stringValue;
            bool saves = level.FindPropertyRelative("saveRecords").boolValue;
            int current = string.IsNullOrEmpty(id) ? -1 : Array.IndexOf(library.unlockLevelIds, id);
            var field = EditorGUI.PrefixLabel(position, new GUIContent("Skin reward"));
            using (new EditorGUI.DisabledScope(!saves || string.IsNullOrEmpty(id)))
            {
                if (!EditorGUI.DropdownButton(field, new GUIContent(current > 0 ? library.names[current] : "None"), FocusType.Keyboard)) return;
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("None"), current < 1, () => Assign(library, id, title, -1));
                var carousel = (LevelCarousel)level.serializedObject.targetObject;
                for (int i = 1; i < library.palettes.Length - 1; i++)
                {
                    int skin = i;
                    string ownerId = library.unlockLevelIds[i];
                    bool assigned = !string.IsNullOrEmpty(ownerId);
                    string owner = carousel.levels.FirstOrDefault(l => l != null && l.recordId == ownerId)?.title ?? "another level";
                    string label = library.names[i] + (assigned ? " — assigned to " + owner : " — available");
                    if (assigned) menu.AddDisabledItem(new GUIContent(label), true);
                    else menu.AddItem(new GUIContent(label), false, () => Assign(library, id, title, skin));
                }
                menu.DropDown(field);
            }
        }

        public static void Assign(SkinLibrary library, string id, string title, int skin)
        {
            if (string.IsNullOrEmpty(id) || skin == 0 || skin >= library.palettes.Length - 1) return;
            if (skin > 0 && !string.IsNullOrEmpty(library.unlockLevelIds[skin]) && library.unlockLevelIds[skin] != id) return;
            Undo.RecordObject(library, "Assign skin reward");
            for (int i = 1; i < library.unlockLevelIds.Length - 1; i++)
                if (library.unlockLevelIds[i] == id) library.unlockLevelIds[i] = "";
            if (skin > 0)
            {
                library.unlockLevelIds[skin] = id;
                library.objectives[skin] = "Complete " + title + " to unlock";
            }
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssetIfDirty(library);
        }

        public static void UpdatePerfectLevels(LevelCarousel carousel)
        {
            var library = Library;
            if (library == null || carousel.levels == null || Application.isPlaying) return;
            var ids = carousel.levels.Where(l => l != null && l.saveRecords && !string.IsNullOrEmpty(l.recordId)).Select(l => l.recordId).Distinct().ToArray();
            if (library.scoredLevelIds.SequenceEqual(ids)) return;
            Undo.RecordObject(library, "Update perfect level list");
            library.scoredLevelIds = ids;
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssetIfDirty(library);
        }
    }
}
