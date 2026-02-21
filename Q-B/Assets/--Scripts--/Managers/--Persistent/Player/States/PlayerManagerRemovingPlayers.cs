
public class PlayerManagerRemovingPlayers : PlayerManager.InputManagerState
{
    public PlayerManagerRemovingPlayers(PlayerManager.PlayerManagerContext context,
        PlayerManager.EPlayerManagementState key,
        PlayerManager.EPlayerManagementState[] invalidTransitions)
        : base(context, key, invalidTransitions)
    {
    }

    public override void EnterState()
    {
        // Disable player joining input
        Context.inputManagerComponent.DisableJoining(); 
        
        // // setting target state to off while managing player count
        Context.ChangeTargetPlayerSettingsConfigurationType(PlayerSettingsSO.EPlayerConfigurationType.Off);
        
        // Remove players until the number of players matches the target
        while (Context.NumPlayers > Context.TargetPlayers)  // CHEEKY WHILE LOOP ??!! :D (could it break?)
        {
            Context.RemovePlayer(Context.NumPlayers); 
        }
    }

    public override void UpdateState()
    {
        // If no longer need to remove players
        if (!Context.NeedLessPlayers) 
        {
            // back to Default state
            Context.ContextCallChangeState(PlayerManager.EPlayerManagementState.SufficientPlayers); 
            
            return;
        }
    }

    public override void ExitState()
    {
    }
}
