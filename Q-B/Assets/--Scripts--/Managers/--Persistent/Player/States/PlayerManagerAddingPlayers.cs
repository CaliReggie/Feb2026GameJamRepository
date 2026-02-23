using UnityEngine;


public class PlayerManagerAddingPlayers : PlayerManager.InputManagerState
{
    public PlayerManagerAddingPlayers(PlayerManager.PlayerManagerContext context,
        PlayerManager.EPlayerManagementState key,
        PlayerManager.EPlayerManagementState[] invalidTransitions)
        : base(context, key, invalidTransitions)
    {
    }
    /// <summary>
    /// Indicates how many more players are needed to reach the target player count.
    /// </summary>
    private int PlayersNeeded => Context.TargetPlayers - Context.NumPlayers;

    public override void EnterState()
    {
        // Enable player joining input
        Context.inputManagerComponent.EnableJoining(); 

        // setting target state to off while managing player count
        Context.ChangeTargetPlayerSettingsConfigurationType(PlayerSettingsSO.EPlayerConfigurationType.Off);
        
        // Pause the application time
        if (ApplicationManager.Instance != null&&
            ApplicationManager.Instance.Started)
        {
            ApplicationManager.Instance.RequestChangeState(ApplicationManager.EApplicationState.Paused);
        }
        else
        {
            Time.timeScale = 0f;
        }
        
        // cursor unlocked and visible in case players can't add and need to cancel/quit
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // toggling adding players page
        Context.ToggleAddingPlayersPage(true, PlayersNeeded);
    }

    public override void UpdateState()
    {
        // If no longer need to add players
        if (!Context.NeedMorePlayers) 
        {
            // Disable player joining input
            Context.inputManagerComponent.DisableJoining(); 
            
            // back to SufficientPlayers state
            Context.ContextCallChangeState(PlayerManager.EPlayerManagementState.SufficientPlayers); 
            
            return;
        }
        else
        {
            //updating players needed display
            Context.ToggleAddingPlayersPage(true, PlayersNeeded);
        }
    }

    public override void ExitState()
    {
        // toggling adding players page off
        Context.ToggleAddingPlayersPage(false);
        
        // resume application time
        if (ApplicationManager.Instance != null&&
            ApplicationManager.Instance.Started)
        {
            ApplicationManager.Instance.RequestChangeState(ApplicationManager.EApplicationState.Running);
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}
