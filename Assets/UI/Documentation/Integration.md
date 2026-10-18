# Menu and game flow

`GameManager` runs the existing state machine. Each screen has a state in `Scripts/Managers/GameManager/StateMachine/States`: main menu, level selection, package selection, controls, settings, scoring guide, loading, gameplay, pause, and results. A state shows its screen in `EnterState` and hides it in `ExitState`. `UIManager` holds the Inspector references and handles fades. The existing button events still work.

`GameSceneLoader` handles loading and unloading the level. Controls, settings, and the scoring guide remember which state opened them, so Back returns to the right screen. `UiController.OpenPage` sends menu navigation to the state machine; `ShowPage` only displays the requested page. To add a screen, follow one of the small existing state classes.

`MainMenu` stays loaded while `GameManager` loads the configured gameplay scene additively. Play opens Level Select, where Tutorial loads `Assets/Scenes/Tutorial.unity`. To change that destination, set GameManager's **Gameplay Scene Path** and enable the scene in Build Settings. Keep MainMenu first. Restart unloads and reloads the level; Main Menu unloads it and restores the menu camera.

Pause suspends `Time.timeScale` and `AudioListener.pause`, including DSP time. Resume restores their previous values after the UI transition finishes. Opening Controls or Settings during a run keeps it paused, and Back returns to Pause. Losing app focus also pauses; returning to the app waits for Resume. UIManager uses unscaled time for fades and panel entrances, blocks touches during transitions, and respects Reduced Motion.

MainMenu owns one EventSystem using `TouchUI.inputactions`. UIManager disables a loaded level's EventSystem and input modules at runtime. The level retains its EventSystem for standalone use. No keyboard or mouse bindings are added.

`UiController` retains local volume and reduced-motion preferences under `rr.ui.v1`. UIManager applies volume to `AudioListener.volume`; menu clicks use the same value while remaining audible during pause. **Click Sound** accepts a team-supplied AudioClip; leaving it empty retains the short menu tone.

The connected Controls screen describes the current gesture actions: tap for Jump; swipe, hold, then release for the directional actions. Tutorial connects all five action events to PlayerActionController. Its beatmap currently uses potholes and duck hazards; brake and accelerate have no matching hazard types yet.

SwipeTrailUI follows the pointer used by the player's Press and Drag actions. It draws a fading trail without consuming input or triggering gameplay actions. UIManager controls its color, size, spacing, glow, and fade.

`ScoreManager` owns the total. Its `ObstacleBase.OnObstacleResolved` subscription is currently commented out after the obstacle-event signature changed, so it does not receive new hazard scores yet. UIManager binds `ScoreDisplay` to that loaded scene's manager. Each hazard adds its existing timing accuracy multiplied by 100. The HUD shows the total rounded to a whole point. Restart and Try Again reload the level, so the new manager starts at zero. Missing score sources display a dash.

ScoreManager also counts resolved hazards. Average accuracy is total points divided by (resolved hazards × 100). Misses contribute zero accuracy and remain in that count. Full stars use 50%, 65%, 80%, 90%, and 98%. Half stars use 25%, 57.5%, 72.5%, 85%, and 94%. Both sets are editable on ScoreDisplay. The HUD shows the current rating; Results snapshots the score, accuracy, and rating together. A run with no resolved hazards receives no stars and displays a dash for accuracy.

The team can call `GameManager.Instance.EndGame()` when a real end condition occurs. Results offers Try Again and Main Menu. `ShowScoreboard()` keeps the same result. Tutorial opens Results after the song ends and all scheduled hazards have resolved. The older obstacle test remains separate. Package damage and saving still need their corresponding gameplay systems. Level selection and the combo display are described below.

Swipes can begin anywhere on the gameplay screen.

HazardWarningUI follows the next unresolved obstacle from the spawner using the music clock. The attention bubble appears 1.1 seconds before its target, changes to the action arrow after 0.25 seconds, and clears on resolution or pause. Its camera and Speaker Visual are assigned in Tutorial. Head Offset centers it over the robot, above the beat pulse. Potholes use an up arrow; duck hazards use a down arrow.

Route and package labels in the original preview are examples. The connected menu bypasses those pages until real route/package data is available.

`UIReview` contains the optional `UiMobileChecks` and `UiWalkthrough` tools. They only run with `-uiQA` or `-uiVideo` in the Editor or a development build. They are not part of the main UI prefab and are not needed when integrating that prefab. Their button-name lookups serve automated captures only. Save captures outside the repository.

Tutorial's ObstacleSpawner prepares each hazard with its beat time in seconds. PlayerActionController sends a release to the nearest unresolved hazard inside its timing window. Input outside all windows is ignored. ObstacleBase still judges the action and timing; ScoreManager still adds the points. Cue pictures have no ObstacleBase and do not count toward accuracy. The spawner checks for misses even without input.

RhythmManager stays in the level scene so retry starts a new song clock. AudioListener.pause freezes that clock along with the music. BeatPulse under PlayHud reads this same clock: teal on each beat, gold on the first beat of a measure. Fade In Beats, Size Change, and colors are editable in the Inspector. Reduced Motion keeps the color pulse without resizing it.

BeatPulse follows the top of the robot sprite, including jumps and the shorter duck pose. Head Offset controls the small gap above it. UIManager connects the level speaker and camera when Tutorial loads. The pulse keeps its original 26-by-7 UI footprint with a navy outline for contrast.

## Level selection and combo display

Open MainMenu and select GameOverlay > LevelSelectPanel > Layout > SafeAreaFrame > LevelCard. LevelCarousel holds the level list. Each entry needs a title, scene path, cover texture in Cover Layers, and song clip. Keep the scene enabled in Build Settings. Tutorial and Desert are assigned to their existing scenes and music.

Swipe horizontally on the card or use the arrow buttons to browse. Tap the card to start the selected level. A drag cannot also start a level. The card slides and fades between choices; Reduced Motion skips that movement. MenuMusicPreview fades between the selected songs using Master and Music volume. Back restores the menu song. The shared background stays visible while browsing. Arrows are hidden at the ends of the list. Starting or retrying a level shows a five-second countdown over the level before gameplay and the music clock resume.

ComboDisplay is under GameOverlay > PlayHud > SafeAreaFrame. It shows a count and multiplier supplied by gameplay, with a short pulse when the count increases. It stays hidden until the count is greater than zero. UIManager clears it when loading a level or returning to the menu; pause keeps the current values.

The gameplay component can use the existing UI reference:

```csharp
ComboDisplay display = GameManager.Instance.UI.comboDisplay;
display.SetCombo(currentCombo, currentMultiplier);
// When the streak breaks:
display.ClearCombo();
```

Call these methods after the gameplay system updates its streak. ComboDisplay does not decide which hits count, change multipliers, or add points. There is no combo calculation connected yet. ScoreManager's obstacle subscription is currently disabled on Staging following the obstacle-event change; the gameplay scoring connection must be restored separately.
