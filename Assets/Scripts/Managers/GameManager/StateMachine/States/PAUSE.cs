public class PAUSE : UIStateBase
{
    public PAUSE(GameManager game) : base(game) { }

    public override void EnterState()
    {
        base.EnterState();
        if (game.UI != null)
            game.UI.ShowPause();
    }
}
