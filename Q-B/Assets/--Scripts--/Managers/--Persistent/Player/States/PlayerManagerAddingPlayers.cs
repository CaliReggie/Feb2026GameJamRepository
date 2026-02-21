
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
    }
}
