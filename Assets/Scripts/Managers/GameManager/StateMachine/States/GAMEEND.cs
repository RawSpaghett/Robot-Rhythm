public class GAMEEND : UIStateBase
{
    public override bool ShowsResults => true;

    public GAMEEND(GameManager game) : base(game) { }

    public override void EnterState()
    {
        base.EnterState();
        if (game.UI != null)
            game.UI.ShowResults();
    }
}
