## Robot skins

Open Skins from the main menu. Swipe right for Accelerate and left for Brake, or use the arrows. Choose Use Skin to equip an unlocked robot. The selection saves on the device.

- Classic is available from the start.
- City unlocks Charcoal; Desert unlocks Sandstone; Ice unlocks Glacier Blue; Mountain Range unlocks Burnt Orange.
- Space unlocks Spectrum, the multicolor skin.
- Prismatic Gold unlocks after perfect completion of every configured scored level. Tutorial does not count. Gold stays unlocked once earned.

Locked robots are black silhouettes with a red lock and an objective underneath. Locked Gold shows `???` above and `????????` below.

## Adding a level reward

1. Open MainMenu and select GameOverlay > LevelSelectPanel > Layout > SafeAreaFrame > LevelCard.
2. Add a Levels entry and assign the title, scene, cover image, and preview song.
3. Enable Save records for scored levels. Leave it off for Tutorial.
4. Choose Skin reward. Available skins can be selected; skins assigned elsewhere are checked and disabled. Spectrum is assigned to Space. None removes a link.
5. Save MainMenu and click Enable listed scenes in Build Settings. Keep MainMenu first.

The card does not create a gameplay scene or a new skin. Set up the scene's beatmap, music, obstacles, and gameplay references separately.

## Artwork and animations

Color masks mark which parts of the existing robot artwork change color. They preserve the outlines, board, and reaction colors while recoloring the current animations. Prismatic Gold blends moving rainbow colors over a gold base, with broad ripples and a soft warm highlight. Reduced Motion keeps the shimmer still.

New or changed robot sprite sheets need matching masks before they are ready to use with skins. Check every action and reaction to avoid uncolored frames or flickering. Obstacle artwork is separate from these masks.

Keep SkinLibrary.previewAllUnlocked off for normal builds. The all-unlocked option and lock-preview buttons are only for local verification.

## Skin loading handoff

The skin library was keeping every animation's skin textures loaded at once. This added a lot of memory use on Android. It now loads the textures needed by the equipped skin or the skin being previewed, with a small shared cache for recently used frames.

Unlocking a skin does not load it. Locked previews use the Classic silhouette. Only the player uses the equipped skin; teammates stay Classic. Unused textures are released when switching skins or closing the preview, and unused level assets are cleaned up between levels. Android texture compression was also updated. The artwork, palette colors, animation frames, and timing were kept the same.
## Adding a skin
- Add its palette, display name, and matching unlock/objective entries to `Assets/Feature5/SkinLibrary.asset`. Keep these lists lined up by index, then assign it through the level card's Skin reward field.
- A normal color variant can reuse the existing masks. A skin with a different color layout needs matching artwork or masks too.
- Keep Classic first and Gold last. Saved selections use indexes, so account for existing saves if adding a skin changes those positions.
## Adding or replacing animations
- Prepare matching `Source`, `Regions`, `Accents`, `Face`, and `Space` textures for every new or changed animation texture. Replacing a sprite sheet alone does not update its masks.
- Put them under `Assets/Feature5/CleanAnimations/Resources/SkinAnimations/`. Update `Catalog.asset` with the exact source texture name and its resource folder, and update `manifest.json` with the source path, folder, and required import size. Follow an existing entry.
- Check every action and reaction with Classic, a regular color skin, Spectrum, and Gold. Check locked previews too, then test on Android.
## If changing the loading code
Use `SkinLibrary.Apply` to apply a skin. It handles the shared cache. Call `SkinTextureCache.Release(material)` before destroying a skin material or permanently closing its preview. Do not unload shared textures directly or put direct texture references back into the catalog, since that would load the whole library again.

## Integration

This update includes the latest Staging obstacle sizes and main's robot jump-height fix. The team's separated input/controller flow, input prefab, and pointer bindings are preserved. The current InputManager and PlayerController match the team's Dev_Jadon versions, including the held-direction charge preview. Skins change the player's material in LateUpdate; the action system still owns animation playback.
