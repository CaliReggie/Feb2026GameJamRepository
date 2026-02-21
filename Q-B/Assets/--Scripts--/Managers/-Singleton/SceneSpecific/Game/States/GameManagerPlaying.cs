
using UnityEngine;

public class GameManagerPlaying : GameManager.GameManagerState
{
    public GameManagerPlaying(GameManager.GameManagerContext context,
        GameManager.EGameState key,
        GameManager.EGameState[] invalidTransitions)
        : base(context, key, invalidTransitions)
    {
    }

    public override void EnterState()
    {
        // show play page
        Context.TogglePlayPage(true);
        
        // ensure application time
        if (ApplicationManager.Instance != null)
        {
            ApplicationManager.Instance.RequestChangeState(ApplicationManager.EApplicationState.Running);
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
    }
}
