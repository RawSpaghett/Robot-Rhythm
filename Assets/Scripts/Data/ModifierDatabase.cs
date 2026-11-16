using UnityEngine;
using System.Collections.Generic;

public class ModifierDatabase: MonoBehaviour
{
    public static ModifierDatabase Instance {get; private set;}//singleton
    public ModifierBase[] modifierLoader;
    public Dictionary<string, ModifierBase> ModifierLibrary = new Dictionary<string, ModifierBase>();

    void Awake()
    {
        IntializeInstance();
        LoadDatabase();
    }

    private void IntializeInstance()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void LoadDatabase()
    {
        foreach (ModifierBase modifier in modifierLoader)
        {
            if (modifier != null && !ModifierLibrary.ContainsKey(modifier.ToString()))
            {
                ModifierLibrary.Add(modifier.ToString(), modifier);
            }
        }
    }

    public ModifierBase GetModifier(string ModifierName)
    {
        if(ModifierLibrary.TryGetValue(ModifierName, out ModifierBase modifier))
        {
            return modifier;
        }
        else
        {
            Debug.Log($"{ModifierName} not found.");
            return null;
        }
    }
}
