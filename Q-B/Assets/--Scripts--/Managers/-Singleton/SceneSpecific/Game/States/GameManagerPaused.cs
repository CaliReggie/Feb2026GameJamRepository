using UnityEngine;
using System;

public class GameManagerPaused : GameManager.GameManagerState
{
    public GameManagerPaused(GameManager.GameManagerContext context,
        GameManager.EGameState key,
        GameManager.EGameState[] invalidTransitions)
        : base(context, key, invalidTransitions)
    {
    }
    
    private bool firstUpdateChecked = false;

    public override void EnterState()
    {
        firstUpdateChecked = false;
        
        // hide play page if desired
        if (Context.hidePlayWhenPaused)
        {
            Context.TogglePlayPage(false);
        }
        
        // Pause the application time
        if (ApplicationManager.Instance != null)
        {
            ApplicationManager.Instance.RequestChangeState(ApplicationManager.EApplicationState.Paused);
        }
        else
        {
            Time.timeScale = 0f;
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfxOneShot(AudioManager.Instance.PauseSfx, 1);
        }
    }

    public override void UpdateState()
    {
        // determining if need to show the game manager pause page or not
        if (!firstUpdateChecked)
        {
            firstUpdateChecked = true;
            
            // if player manager exists and is in sufficient players state
            // check if any player needs to see the scene pause UI and show it if so
            if (PlayerManager.Instance != null &&
                PlayerManager.Instance.CurrentState.State == PlayerManager.EPlayerManagementState.SufficientPlayers)
            {
                // getting current player manager settings
                PlayerManagerSettingsSO currentPlayerManagerSettings =
                    PlayerManager.Instance.CurrentPlayerManagerSettings;
                
                // goings through player manager settings PlayersSettings
                for (int i = 0; i < currentPlayerManagerSettings.PlayersSettings.Count; i++)
                {
                    PlayerSettingsSO playerSettings = currentPlayerManagerSettings.PlayersSettings[i];
            
                    if (playerSettings.NeedToSeeScenePauseUi)
                    {
                        // show pause page
                        Context.TogglePausePage(true);
                        
                        // exit loop if found one
                        break;
                    }
                }
            }
            // if no player manager exists, attempt working with possible standalone Player Input Object
            else if (Context.singlePlayerPioReference != null)
            {
                PlayerSettingsSO playerSettings = Context.singlePlayerPioReference.CurrentPlayerSettings;
            
                if (playerSettings.NeedToSeeScenePauseUi)
                {
                    // show pause page
                    Context.TogglePausePage(true);
                }
            }
        }
    }

    public override void ExitState()
    {
        // hide pause page
        Context.TogglePausePage(false);
        
        // resume application time
        if (ApplicationManager.Instance != null)
        {
            ApplicationManager.Instance.RequestChangeState(ApplicationManager.EApplicationState.Running);
        }
        else
        {
            Time.timeScale = 1f;
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfxOneShot(AudioManager.Instance.PauseSfx, 1);
        }
    }
}
