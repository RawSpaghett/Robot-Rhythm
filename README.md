# Robot Rhythm UI

Landscape mobile UI in Unity.

Open `Assets/Scenes/MainMenu.unity` for the connected menu and game flow.

Play opens `PotholeTimingTest`. Pause offers Resume, Restart, Controls, Settings, Scoring, and Main Menu. Scoring also opens from Home. The original menu layouts remain in `Assets/RobotRhythmUI/Scenes/RobotRhythmUIPreview.unity`.

The gameplay HUD displays ScoreManager's live total. `GameManager.EndGame()` opens Results with the final score; the looping obstacle test does not trigger an end automatically.

- [Setup](Assets/RobotRhythmUI/Documentation/Start-here.md)
- [Menu behavior](Assets/RobotRhythmUI/Documentation/Integration.md)
- [UI specification](Design/UI/Design-notes.md)
