public class GAMESTART : UIStateBase
{
    public GAMESTART(GameManager game) : base(game) { }
    public override bool PausesGameplay => false;

    public override void EnterState()
    {
        base.EnterState();
        if (game.UI != null)
            game.UI.ShowGameplay();
    }
}
