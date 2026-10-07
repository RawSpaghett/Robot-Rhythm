public class MAINMENU : UIStateBase
{
    public MAINMENU(GameManager game) : base(game) { }

    public override void EnterState()
    {
        base.EnterState();
        if (game.UI != null)
            game.UI.ShowMenu(RobotRhythm.UI.UiPage.Home);
    }
}
