using System;
using UnityEngine;

// Shared by robot animations. The Editor drawer collects
// sliced sprites when a sheet is assigned and serializes them for player builds.
[Serializable]
public class SpriteSheetSequence
{
    [SerializeField] private Texture2D sheet;
    // False preserves the original layout order for existing animation slots.
    [SerializeField] private bool sortByNumericName;
    [SerializeField, HideInInspector] private Sprite[] frames = new Sprite[0];

    public Sprite[] Frames => frames;
    public bool IsValid => sheet != null && RobotAnimationVariations.IsValid(frames);
}
