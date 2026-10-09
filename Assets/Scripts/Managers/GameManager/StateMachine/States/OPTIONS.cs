public class OPTIONS : UIStateBase
{
    public UIStateBase ReturnState { get; private set; }

    public OPTIONS(GameManager game) : base(game) { }

    public void Open(UIStateBase returnState)
    {
        ReturnState = returnState;
        game.ChangeState(this);
    }

    public void Close()
    {
        if (game.CurrentState == this)
            game.ChangeState(ReturnState);
    }

    public override void EnterState()
    {
        base.EnterState();
        if (game.UI != null)
            game.UI.ShowMenu(RobotRhythm.UI.UiPage.Settings);
    }
}
