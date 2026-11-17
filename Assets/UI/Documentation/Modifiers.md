## Modifier menu

Open MainMenu and choose a level. Play starts a standard run. Modifiers opens the separate game-mode options. Tutorial only has Play.

Tap a modifier to turn it on or off, then press Start Run. Gold cards are selected. Retry keeps the selection. Choosing a level again clears it.

## Adding a modifier

Select ModifierDatabase in MainMenu and use Add modifier. The menu rebuilds from the database each time it opens. Do not add buttons by hand.

A modifier must inherit ModifierBase, have [System.Serializable], and implement Enabled(). Its gameplay effect stays in that class. The UI fills ModifierManager.activeModifiers before the effects run. Selected instances are reused on Retry, so reset any per-run state in Enabled().

On ModifierMenu, add a matching entry under Descriptions: use the exact class name for modifierType, the player-facing name for title, a short subtitle for badge, an accent color, and an explanation. NO SIGHT is the displayed name for the existing Modifier_Brutal class.

One card is wider and centered. Two sit side by side. Three have a centered last row. Four use a two-by-two layout. Larger lists scroll without shrinking the text. Longer descriptions can increase card height.

## Records and unlocks

Modifiers are a separate game mode. Modified runs display score and accuracy, but do not save standard records, award standard perfect ribbons, or unlock skins. Standard runs keep the existing behavior.

## Gameplay issue found in Android testing

No Sight selection reaches the modifier system, but obstacles can remain visible. Modifier_Brutal.Enabled() clears levelSprites[].Sprite. ObstacleSpawner.ChangeSprite() only replaces the prefab sprite when that value is not null, so a cleared override leaves the default art visible. Animated obstacles also have their own art path. The programmers need to handle hiding the rendered obstacle in the gameplay implementation; the menu does not control obstacle rendering.
