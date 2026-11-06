# Robot skins

Open Skins from the main menu. Swipe right for Accelerate and left for Brake, or use the arrows. Choose Use Skin to equip an unlocked robot. The selection saves on the device.

- Classic is available from the start.
- City unlocks Charcoal; Desert unlocks Sandstone; Ice unlocks Glacier Blue; Mountain Range unlocks Burnt Orange.
- Space is prepared but has no level assigned yet.
- Prismatic Gold unlocks after perfect completion of every configured scored level. Tutorial does not count. Gold stays unlocked once earned.

Locked robots are black silhouettes with a red lock and an objective underneath. Locked Gold shows `???` above and `????????` below.

## Adding a level reward

1. Open MainMenu and select GameOverlay > LevelSelectPanel > Layout > SafeAreaFrame > LevelCard.
2. Add a Levels entry and assign the title, scene, cover image, and preview song.
3. Enable Save records for scored levels. Leave it off for Tutorial.
4. Choose Skin reward. Available skins can be selected; skins assigned elsewhere are checked and disabled. Space is currently available. None removes a link.
5. Save MainMenu and click Enable listed scenes in Build Settings. Keep MainMenu first.

The card does not create a gameplay scene or a new skin. Set up the scene's beatmap, music, obstacles, and gameplay references separately.

## Artwork and animations

Color masks mark which parts of the existing robot artwork change color. They preserve the outlines, board, and reaction colors while recoloring the current animations. Prismatic Gold blends moving rainbow colors over a gold base, with broad ripples and a soft warm highlight. Reduced Motion keeps the shimmer still.

New or changed robot sprite sheets need matching masks before they are ready to use with skins. Check every action and reaction to avoid uncolored frames or flickering. Obstacle artwork is separate from these masks.

Keep SkinLibrary.previewAllUnlocked off for normal builds. The all-unlocked option and lock-preview buttons are only for local verification.

## Integration

This update includes the latest Staging obstacle sizes and main's robot jump-height fix. The team's separated input/controller flow, input prefab, and pointer bindings are preserved. InputManager has one additional guard for a missing pointer device, so startup or device removal does not throw errors. Skins change the player's material in LateUpdate; the action system still owns animation playback.
