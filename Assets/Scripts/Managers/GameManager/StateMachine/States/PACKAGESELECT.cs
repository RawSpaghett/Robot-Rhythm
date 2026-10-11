public class PACKAGESELECT : UIStateBase
{
    public PACKAGESELECT(GameManager game) : base(game) { }

    public override void EnterState()
    {
        base.EnterState();
        if (game.UI != null)
            game.UI.ShowMenu(RobotRhythm.UI.UiPage.Placeholder);
    }
}
