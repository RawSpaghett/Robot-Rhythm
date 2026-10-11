public class LOADING : UIStateBase
{
    public LOADING(GameManager game) : base(game) { }

    public override void EnterState()
    {
        base.EnterState();
        if (game.UI != null)
            game.UI.ShowLoading();
    }
}
