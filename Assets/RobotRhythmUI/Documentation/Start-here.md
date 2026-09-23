# Editing the UI

Open `Assets/RobotRhythmUI/Scenes/RobotRhythmUIPreview.unity`. The main layout is `Prefabs/RobotRhythmUI.prefab`; its `SafeAreaFrame` contains Home, Routes, Controls, Placeholder, and Settings.

Edit text, spacing, colors, and button events in Prefab Mode. Each button's On Click list shows its menu action. The `UiController` on the root holds the screen, selection-label, and settings references. Page order is Home (0), Routes (1), Controls (2), Placeholder (3), Settings (4).

`LogoMotion` on Home/LogoPlate exposes entrance timing, offsets, and idle motion. Reduced Motion stops the animation. `UiTheme.asset` holds the fonts, palette, and panel sprites; selection colors read it at runtime. Other authored colors and fonts remain editable on their prefab components.

The Canvas uses a 1280 × 720 reference resolution. `LandscapeSafeArea` fits that frame inside the phone's safe area without stretching low-resolution text. Keep the CanvasScaler and safe-area reference sizes consistent.

`Editor/UiBuilder.cs` reconstructs the original layout. It is not needed to run or normally edit the prefab. Rebuild replaces prefab and scene edits; use it only when deliberately reconstructing the layout. Builds use the saved scene and do not rebuild it.

The project specifies Unity 6000.3.10f1. Keep the team's editor and package versions. The editable wordmark is in `Design/Logo/`; artwork and font licenses are listed in `Asset-credits.md`.
