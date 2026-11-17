using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModifierDatabase))]
public class ModifierDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("modifierLoader"), true);
        serializedObject.ApplyModifiedProperties();
        if (!GUILayout.Button("Add modifier")) return;
        var menu = new GenericMenu();
        foreach (var type in TypeCache.GetTypesDerivedFrom<ModifierBase>().Where(t => !t.IsAbstract && t.IsSerializable && t.GetConstructor(Type.EmptyTypes) != null).OrderBy(t => t.Name))
        {
            var choice = type;
            menu.AddItem(new GUIContent(type.Name.Replace("Modifier_", "")), false, () =>
            {
                serializedObject.Update();
                var items = serializedObject.FindProperty("modifierLoader");
                int index = items.arraySize++;
                items.GetArrayElementAtIndex(index).managedReferenceValue = Activator.CreateInstance(choice);
                serializedObject.ApplyModifiedProperties();
            });
        }
        menu.ShowAsContext();
    }
}
