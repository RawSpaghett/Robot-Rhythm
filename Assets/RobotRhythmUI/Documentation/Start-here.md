# Editing the UI

Open `Assets/Scenes/MainMenu.unity` and enter Play Mode for the connected flow. Its `GameManager` selects the gameplay scene, and `UIManager` holds the menu, camera, touch input, and overlay references. Start from this scene to include the menus when testing a level.

The shared menu layout is `Prefabs/RobotRhythmUI.prefab`. The MainMenu instance overrides its Play, Controls, and Settings button events, volume label, and gesture instructions. `Prefabs/GameOverlay.prefab` contains the pause button, pause menu, loading, end, and error panels; their button events are assigned in MainMenu. Edit those instance events in the scene, preserving the manager references.

MainMenu also holds the icon-only Pause button and `ScreenTransition` canvas. Tune **Transition Duration** on UIManager. Its button events handle navigation; `ButtonPressFeedback` presses the button shapes while labels and touch targets stay fixed. Reduced Motion keeps the fade short and disables positional motion.

The score HUD and Results layout are scene overrides under GameOverlay. `ScoreDisplay` on UIManager references **Live Score**, **Final Score**, and the menu preferences. Tune **Result Duration** there. The gameplay score stays navy and does not move when points change. The connected scene uses the existing Lilita One font for buttons, large headings, and score digits; body copy keeps its previous font. Reduced Motion skips the number roll and shows the final total immediately.

ScoreDisplay's **Star Thresholds** use normalized accuracy, from 0 to 1, in ascending order. The five full-star defaults are 0.50, 0.65, 0.80, 0.90, and 0.98. **Half Star Thresholds** are 0.25, 0.575, 0.725, 0.85, and 0.94. Live and result star arrays are ordered left to right. Delivery feedback and the accuracy percentage use the same captured result as the score. Empty stars have a navy inset finish. Earned stars rise in gold and settle into a raised bevel. On each RatingStar, **Star Finish** controls the colors and bevel width; **Pop Animation** controls the jump duration, height, and scale. Reduced Motion shows the earned state immediately. RollingNumber on FinalScore and AccuracyValue rolls each digit upward; ScoreDisplay controls the shared result duration.

`Prefabs/GesturePad.prefab` contains the gameplay pad's arrows, touch markers, and charge bar. Its instance sits under Canvas/TouchSafeArea/GameplayButton in PotholeTimingTest. Edit the visuals in Prefab Mode; keep the GestureButtonInput action references on GameplayButton. **Release Duration** on GesturePadFeedback controls how quickly the highlight settles. **Pad Motion** sets arrow growth and the held-direction tilt. Visual Surface is the GesturePad root; the GameplayButton touch target stays still.

GameplayButton displays at two-thirds of its previous size. TouchPadLayout keeps its current size and applies the saved control side. The warning controller sits on TouchSafeArea; set **Obstacle**, **Speaker**, **World Camera**, **World Offset**, and the bubble's graphics there. **Advance Warning** and **Attention Duration** control the cue timing. The Fill graphic draws the entire warning bubble; its Outline color, Border Width, and Shadow Offset keep the curved body and rounded tail consistent.

`Assets/RobotRhythmUI/Scenes/RobotRhythmUIPreview.unity` retains the original menu-only layouts. Its Routes and Placeholder pages are not used by the connected flow.

Edit text, spacing, colors, and button events in Prefab Mode. Each button's On Click list shows its menu action. The `UiController` on the root holds the screen, selection-label, and settings references. Page order is Home (0), Routes (1), Controls (2), Placeholder (3), Settings (4).

`LogoMotion` on Home/LogoPlate exposes entrance timing, offsets, and idle motion. Reduced Motion stops the animation. `UiTheme.asset` holds the fonts, palette, and panel sprites; selection colors read it at runtime. Other authored colors and fonts remain editable on their prefab components.

Logo textures use trilinear filtering, mipmaps, and uncompressed platform imports. CutPanel, DirectionArrow, RatingStar, and SpeechBubble feather their outer edges to keep tilted and curved shapes smooth without changing the fonts or wordmark geometry.

The Canvas uses a 1280 Ã— 720 reference resolution. `LandscapeSafeArea` fits that frame inside the phone's safe area without stretching low-resolution text. Keep the CanvasScaler and safe-area reference sizes consistent.

`Editor/UiBuilder.cs` reconstructs the original layout. It is not needed to run or normally edit the prefab. Rebuild replaces prefab and scene edits; use it only when deliberately reconstructing the layout. Builds use the saved scene and do not rebuild it.

The project specifies Unity 6000.3.10f1. Keep the team's editor and package versions. The editable wordmark is in `Design/Logo/`; artwork and font licenses are listed in `Asset-credits.md`.

ScoreDisplay **Number Colors** uses timing accuracy for the results accuracy number (the gameplay score stays navy): 0–49% red, 50–59% orange, 60–69% yellow, 70–79% green, 80–89% cyan, 90–94% blue, and 95–100% violet. Bands use the rounded percentage shown on results. NumberFinish adds a raised edge and navy depth behind each number. These colors do not change points or star cutoffs. UIManager hides the level touch pad on results and restores it for a new run.

The logo keeps its entrance and gentle bob. The logo and gesture pad use their own canvases with Pixel Perfect off, so motion passes smoothly between pixels. The main canvases keep Pixel Perfect on for stationary text. Menu changes use fades; score values and button labels stay in place. DragLine uses CutPanel with feathered edges instead of a plain Image.

For UI tuning, select the UIManager object in MainMenu for screen references and fade timing. ScoreDisplay holds the half/full star thresholds, result duration, and Accuracy Bands (minimum percentage plus color). RollingNumber on each result label controls full turns and digit delay. On the GesturePad prefab, GesturePadFeedback exposes line width, arrow growth, tilt, held offset/scale, and settle speed; DragLine controls its color. RatingStar exposes inset colors, bevel, and pop timing. HazardWarningUI exposes the warning timing, placement, and entrance. Button events stay on their Button components.

NumberFinish on LiveScore, FinalScore, and AccuracyValue controls the raised number style: shadow color, depth, bevel width, highlight, and shade. It only changes how text is drawn. RollingNumber copies that finish to its digit faces.

SCORING opens from Home or Pause. The ScoringGuide canvas holds the guide text and example buttons; edit those in MainMenu. ScoringGuide only updates its own example points and hazard count. It reads the existing ScoreDisplay colors and star thresholds, and never changes the active run. Back returns to the menu that opened it.

Results score colors use their own editable Score Bands: red below 100 points, orange from 100, yellow from 250, green from 500, cyan from 1,000, blue from 1,500, and violet from 2,000. Accuracy still uses its percentage bands. For example, 1,600 points can be blue while 55% accuracy is orange. The guide example uses these separate lookups too.

During the results roll, score and accuracy move through their own color bands from zero to the final value. Reduced Motion shows the final colors immediately.
