
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
            
            if (Context.gameWon)
            {
                // this game levels are just pass fail, so if the player won, set the best score to 1 (pass)
                PlayerPrefsManager.SetSceneBestScore(ApplicationManager.Instance.ActiveSceneSettings.ChronologicalId,
                    1);
            }
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
        Context.ToggleOverPage(Context.gameWon, true);

        Context.ToggleWinCountdown(false);
    }

    public override void UpdateState()
    {
        
    }

    public override void ExitState()
    {
        // hide over page
        Context.ToggleOverPage(Context.gameWon, false);
        
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
