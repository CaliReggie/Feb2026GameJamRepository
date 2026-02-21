
using System;
using UnityEngine;

public class GameManagerOver : GameManager.GameManagerState
{
    public GameManagerOver(GameManager.GameManagerContext context,
        GameManager.EGameState key,
        GameManager.EGameState[] invalidTransitions)
        : base(context, key, invalidTransitions)
    {
    }

    public override void EnterState()
    {
        // Pause the application time
        if (ApplicationManager.Instance != null)
        {
            ApplicationManager.Instance.RequestChangeState(ApplicationManager.EApplicationState.Paused);
        }
        else
        {
            Time.timeScale = 0f;
        }
        
        // hide play page when over if desired
        if (Context.hidePlayWhenOver)
        {
            Context.TogglePlayPage(false);
        }
        
        // show over page
        Context.ToggleOverPage(true);
    }

    public override void UpdateState()
    {
        
    }

    public override void ExitState()
    {
        // hide over page
        Context.ToggleOverPage(false);
        
        // resume application time
        if (ApplicationManager.Instance != null)
        {
            ApplicationManager.Instance.RequestChangeState(ApplicationManager.EApplicationState.Running);
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}
