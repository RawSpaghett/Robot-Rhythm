using RobotRhythm.UI;
using UnityEngine;

public class StudioCredits : MonoBehaviour
{
    public GameObject home, panel;
    public UIManager ui;
    public UiController preferences;

    public void Open()
    {
        if (ui.IsTransitioning) return;
        preferences.PlayClick();
        ui.SwitchMenuContent(home, panel);
    }

    public void Close()
    {
        if (ui.IsTransitioning) return;
        preferences.PlayBackClick();
        ui.SwitchMenuContent(panel, home);
    }
}
