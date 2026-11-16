using System;
using UnityEngine;

public class ModifierManager : MonoBehaviour
{
    public ModifierBase[] activeModifiers;
    
    private void OnEnable()
    {
        foreach (ModifierBase modifier in activeModifiers)
        {
            modifier.Enabled();
        }
    }
}
