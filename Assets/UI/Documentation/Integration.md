# Menu and game flow

`GameManager` runs the existing state machine. Each screen has a state in `Scripts/Managers/GameManager/StateMachine/States`: main menu, level selection, package selection, controls, settings, scoring guide, loading, gameplay, pause, and results. A state shows its screen in `EnterState` and hides it in `ExitState`. `UIManager` holds the Inspector references and handles fades. The existing button events still work.

`GameSceneLoader` handles loading and unloading the level. Controls, settings, and the scoring guide remember which state opened them, so Back returns to the right screen. `UiController.OpenPage` sends menu navigation to the state machine; `ShowPage` only displays the requested page. To add a screen, follow one of the small existing state classes.

`MainMenu` stays loaded while `GameManager` loads the configured gameplay scene additively. Play opens Level Select. LevelCarousel supplies the selected scene, cover image, and preview song. Tutorial, City, Desert, Ice, and Mountain Range use scenes under `Assets/Scenes/Levels`. The four non-tutorial entries point to the scenes marked NEW; OLD scenes are not in the playable build. Keep MainMenu first in Build Settings. Restart unloads and reloads the level; Main Menu unloads it and restores the menu camera.

Pause suspends `Time.timeScale` and `AudioListener.pause`, including DSP time. Resume shows RESUMING with a five-second countdown before restoring gameplay and audio. Gameplay input and the pause button stay blocked during both the starting and resuming countdowns. The song clock remains paused until the countdown finishes. Opening Controls or Settings during a run keeps it paused, and Back returns to Pause. Losing app focus also pauses; returning to the app waits for Resume. UIManager uses unscaled time for fades and panel entrances, blocks touches during transitions, and respects Reduced Motion.

MainMenu owns one EventSystem using `TouchUI.inputactions`. UIManager disables a loaded level's EventSystem and input modules at runtime. The level retains its EventSystem for standalone use. Desktop testing uses InputManager mouse handling: click and release to jump, or hold the left mouse button, drag and release: up for Long Jump, down for Duck, left for Brake, or right for Accelerate.

`UiController` retains local volume and reduced-motion preferences under `rr.ui.v1`. UIManager applies volume to `AudioListener.volume`; menu clicks use the same value while remaining audible during pause. **Click Sound** accepts a team-supplied AudioClip; leaving it empty retains the short menu tone.

The connected Controls screen shows all five actions: Tap for Jump, swipe left for Brake, swipe right for Accelerate, swipe down for Duck, and swipe up for Long Jump. Touch swipes trigger when the direction is detected; they do not wait for release or gain strength from holding longer.

SwipeTrailUI follows the pointer used by the player's Press and Drag actions. It draws a fading trail without consuming input or triggering gameplay actions. UIManager controls its color, size, spacing, glow, and fade.

`ScoreManager` owns the total. It listens to ComboTracker.OnScoreReady when the level has a combo tracker. ComboTracker sends timing accuracy and the multiplier after updating the combo. Without an active tracker, ScoreManager listens directly to ObstacleBase and uses x1. It ignores obstacles from other scenes. UIManager binds `ScoreDisplay` to that loaded scene's manager. Each hazard adds its existing timing accuracy multiplied by 100 and the combo multiplier. The first successful hit uses x1.1, and the multiplier caps at x2. The HUD shows the total rounded to a whole point. Restart and Try Again reload the level, so the new manager starts at zero. Missing score sources display a dash.

ScoreManager also counts resolved hazards. Average accuracy is the sum of timing accuracy divided by the number of resolved hazards. Combo bonus points are not included. Misses contribute zero accuracy and remain in that count. Full stars use 50%, 65%, 80%, 90%, and 98%. Half stars use 25%, 57.5%, 72.5%, 85%, and 94%. Both sets are editable on ScoreDisplay. The HUD shows the current rating; Results snapshots the score, accuracy, and rating together. A run with no resolved hazards receives no stars and displays a dash for accuracy.

The team can call `GameManager.Instance.EndGame()` when a real end condition occurs. Results offers Retry, Level Selector, and Main Menu. Level Selector unloads the current level and returns to the level cards. `ShowScoreboard()` keeps the same result. Tutorial opens Results after the song ends and all scheduled hazards have resolved. The older obstacle test remains separate. Package damage and saving still need their corresponding gameplay systems. Level selection and the combo display are described below.

Swipes can begin anywhere on the gameplay screen.

HazardWarningUI follows the next unresolved obstacle from the spawner using the music clock. The attention bubble appears 1.1 seconds before its target, changes to the action arrow after 0.25 seconds, and clears on resolution or pause. Its camera and Speaker Visual are assigned in Tutorial. Head Offset positions it above the robot. Potholes use an up arrow; duck hazards use a down arrow.

Route and package labels in the original preview are examples. The connected menu bypasses those pages until real route/package data is available.

`UIReview` contains the optional `UiMobileChecks` and `UiWalkthrough` tools. They only run with `-uiQA` or `-uiVideo` in the Editor or a development build. They are not part of the main UI prefab and are not needed when integrating that prefab. Their button-name lookups serve automated captures only. Save captures outside the repository.

ObstacleSpawner prepares each hazard with its target time from the beatmap and timing offset. PlayerTimingFeedback forwards the player's action to the nearest eligible obstacle using RhythmManager.SongTime. ObstacleBase judges the action and timing; ScoreManager adds the points. Cue pictures have no ObstacleBase and do not count toward accuracy. The spawner checks for misses even without input.

RhythmManager stays in the level scene so retry starts a new song clock. AudioListener.pause freezes that clock along with the music. The shared HUD displays song progress; the overhead beat pulse is not enabled.

## Level selection and combo display

Open MainMenu and select GameOverlay > LevelSelectPanel > Layout > SafeAreaFrame > LevelCard. In LevelCarousel, expand Levels and press + to add an entry. Enter its title, drag in the scene asset, assign a cover image, and assign the preview song. Press **Enable listed scenes in Build Settings**, then save MainMenu. Drag entries to reorder them; remove an entry with the minus button. No code changes are needed. The Inspector warns about missing fields or scenes that are not enabled. Tutorial, City, Desert, Ice, and Mountain Range are configured. Each preview uses the SongID assigned by that scene's beatmap. The level scene still owns gameplay and its song clock; the preview clip should match that scene's music. Removing a card does not delete its scene or disable it in Build Settings.

The card material uses the existing paper panel artwork to preserve its uneven edge without enlarging the original artwork shadow, with the level cover filling the interior. Cover images work best at 16:9. Keep titles and important artwork away from the paper edges.

Swipe horizontally on the card or use the arrow buttons to browse. Tap the card to start the selected level. A drag cannot also start a level. The card slides and fades between choices; Reduced Motion skips that movement. MenuMusicPreview fades between the selected songs using Master and Music volume. Back restores the menu song. The shared background stays visible while browsing. Arrows are hidden at the ends of the list. Starting or retrying a level shows GET READY with a five-second countdown. Resuming a paused run shows RESUMING with the same countdown. Gameplay input and the pause button stay blocked until the countdown ends; the music clock remains frozen throughout.

ComboDisplay is under GameOverlay > PlayHud > SafeAreaFrame. It shows a count and multiplier supplied by gameplay, with a short pulse when the count increases. It stays hidden until the count is greater than zero. UIManager clears it when loading a level or returning to the menu; pause keeps the current values.

The gameplay component can use the existing UI reference:

```csharp
ComboDisplay display = GameManager.Instance.UI.comboDisplay;
display.SetCombo(currentCombo, currentMultiplier);
// When the streak breaks:
display.ClearCombo();
```

Call these methods after the gameplay system updates its streak. ComboDisplay does not decide which hits count, change multipliers, or add points. Each listed level has one active ComboTracker connected to the shared HUD. The ScoreManager prefab already includes that component. Do not add a second tracker or a separate combo HUD. The shared MainMenu HUD supplies the same score, accuracy, stars, combo, feedback, and song progress display for every level. The tracker sends the updated multiplier to ScoreManager through OnScoreReady, which applies it to awarded points. Assign the player InputManager to PlayerTimingFeedback, and assign that bridge the player ActionManager, ObstacleSpawner, and RhythmManager. Leave PlayerObstacleScoring disabled when using this bridge so the same gesture cannot judge two obstacles.

GameplayFeedback on GameOverlay > PlayHud displays the existing obstacle result in BubbleCircuit. Perfect uses the obstacle's WasPerfect flag; other successful hits show Good. Too Early, Too Late, and Miss use the reported result. Feedback Duration is editable in the Inspector. Reduced Motion removes the pop animation. Pause hides and clears the judgment.

SongProgress under PlayHud > SafeAreaFrame reads RhythmManager.songPosition divided by the beatmap SongLength. Keep SongLength correct for each level. It uses the existing hand-drawn slider artwork and freezes with the song during pause. LiveAccuracy is inside ScorePlate and uses the same ScoreManager accuracy as the results screen.

Effects volume applies to menu sounds and gameplay sound effects. FXManager reads rr.effects and multiplies it by the AudioSource volume captured at startup. Changes apply to active and new effects; Master Volume controls the overall mix and Music controls songs independently.
