# Menu and game flow

`GameManager` owns the existing state machine, scene loading, and pause/resume. `UIManager` observes its `StateChanged` event and shows the corresponding menu or overlay. Buttons call the managers through Inspector UnityEvents; gameplay scripts do not need references to menu panels.

`MainMenu` stays loaded while `GameManager` loads the configured gameplay scene additively. Play opens Level Select, where Tutorial loads `Assets/Scenes/Tutorial.unity`. To change that destination, set GameManager's **Gameplay Scene Path** and enable the scene in Build Settings. Keep MainMenu first. Restart unloads and reloads the level; Main Menu unloads it and restores the menu camera.

Pause suspends `Time.timeScale` and `AudioListener.pause`, including DSP time. Resume restores their previous values after the UI transition finishes. Opening Controls or Settings during a run keeps it paused, and Back returns to Pause. Losing app focus also pauses; returning to the app waits for Resume. UIManager uses unscaled time for fades and panel entrances, blocks touches during transitions, and respects Reduced Motion.

MainMenu owns one EventSystem using `TouchUI.inputactions`. UIManager disables a loaded level's EventSystem and input modules at runtime, so the existing touch pad uses that same EventSystem. The level retains its EventSystem for standalone use. No keyboard or mouse bindings are added.

`UiController` retains local volume and reduced-motion preferences under `rr.ui.v1`. UIManager applies volume to `AudioListener.volume`; menu clicks use the same value while remaining audible during pause. **Click Sound** accepts a team-supplied AudioClip; leaving it empty retains the short menu tone.

The connected Controls screen describes the current gesture actions: tap for Jump; swipe, hold, then release for the directional actions. Tutorial connects all five action events to PlayerActionController. Its beatmap currently uses potholes and duck hazards; brake and accelerate have no matching hazard types yet.

Tutorial's touch pad uses `GesturePadFeedback` for direction arrows, a touch trace, and the existing action's charge value. `GestureButtonInput` passes its recognized gesture to the feedback component; action matching and execution remain in the team's input scripts. Pausing cancels held gestures. The older direction/charge graphics and test readout are hidden. The pad fits inside the landscape safe area.

`ScoreManager` in Tutorial receives `ObstacleBase.OnObstacleResolved` and owns the total. UIManager binds `ScoreDisplay` to that loaded scene's manager. Each hazard adds its existing timing accuracy multiplied by 100. The HUD shows the total rounded to a whole point. Restart and Try Again reload the level, so the new manager starts at zero. Missing score sources display a dash.

ScoreManager also counts resolved hazards. Average accuracy is total points divided by (resolved hazards × 100). Misses contribute zero accuracy and remain in that count. Full stars use 50%, 65%, 80%, 90%, and 98%. Half stars use 25%, 57.5%, 72.5%, 85%, and 94%. Both sets are editable on ScoreDisplay. The HUD shows the current rating; Results snapshots the score, accuracy, and rating together. A run with no resolved hazards receives no stars and displays a dash for accuracy.

The team can call `GameManager.Instance.EndGame()` when a real end condition occurs. Results offers Try Again and Main Menu. `ShowScoreboard()` keeps the same result. Tutorial opens Results after the song ends and all scheduled hazards have resolved. The older obstacle test remains separate. Combo, package damage, saving, and level music selection still need their corresponding gameplay systems.

Controls default to the right. The LEFT and RIGHT buttons in Settings select the matching bottom corner and saves the preference under `rr.ui.v1.controlsRight`. TouchPadLayout moves the existing input target, including its feedback, without changing gesture thresholds.

HazardWarningUI follows the next unresolved obstacle from the spawner using the music clock. The attention bubble appears 1.1 seconds before its target, changes to the action arrow after 0.25 seconds, and clears on resolution or pause. Its camera and Speaker Visual are assigned in Tutorial. Head Offset centers it over the robot, above the beat pulse. Potholes use an up arrow; duck hazards use a down arrow.

Route and package labels in the original preview are examples. The connected menu bypasses those pages until real route/package data is available.

`UIReview` contains the optional `UiMobileChecks` and `UiWalkthrough` tools. They only run with `-uiQA` or `-uiVideo` in the Editor or a development build. They are not part of the main UI prefab and are not needed when integrating that prefab. Their button-name lookups serve automated captures only. Save captures outside the repository.

Tutorial's ObstacleSpawner prepares each hazard with its beat time in seconds. PlayerActionController sends a release to the nearest unresolved hazard inside its timing window. Input outside all windows is ignored. ObstacleBase still judges the action and timing; ScoreManager still adds the points. Cue pictures have no ObstacleBase and do not count toward accuracy. The spawner checks for misses even without input.

RhythmManager stays in the level scene so retry starts a new song clock. AudioListener.pause freezes that clock along with the music. BeatPulse under PlayHud reads this same clock: teal on each beat, gold on the first beat of a measure. Fade In Beats, Size Change, and colors are editable in the Inspector. Reduced Motion keeps the color pulse without resizing it.

BeatPulse follows the top of the robot sprite, including jumps and the shorter duck pose. Head Offset controls the small gap above it. UIManager connects the level speaker and camera when Tutorial loads. The pulse keeps its original 26-by-7 UI footprint with a navy outline for contrast.
