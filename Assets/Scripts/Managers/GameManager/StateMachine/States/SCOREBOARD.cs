public class SCOREBOARD : UIStateBase
{
    public override bool ShowsResults => true;

    public SCOREBOARD(GameManager game) : base(game) { }

    public override void EnterState()
    {
        base.EnterState();
        if (game.UI != null)
            game.UI.ShowResults();
    }
}
