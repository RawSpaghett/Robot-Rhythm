// Shared setup for the game's screen states. StateMachineBase still runs Enter/Exit.
public abstract class UIStateBase : StateBase
{
    protected readonly GameManager game;
    public virtual bool PausesGameplay => true;
    public virtual bool ShowsResults => false;

    protected UIStateBase(GameManager game)
    {
        this.game = game;
    }

    public override void EnterState()
    {
        game.SetStatePause(PausesGameplay);
        if (game.UI != null)
            game.UI.PrepareScreen(ShowsResults);
    }

    public override void ExitState()
    {
        if (game.UI != null)
            game.UI.HideScreens();
    }
}
