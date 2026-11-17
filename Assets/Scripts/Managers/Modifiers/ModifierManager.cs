using System;
using UnityEngine;

public class ModifierManager : MonoBehaviour
{
    public ModifierBase[] activeModifiers;
    
    private void OnEnable()
    {
        ModifierMenu.ApplySelection(this);
        if (activeModifiers == null) return;
        foreach (ModifierBase modifier in activeModifiers)
        {
            if (modifier != null) modifier.Enabled();
        }
    }
}
