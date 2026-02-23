
using UnityEngine;

public class PlayerManagerSufficientPlayers : PlayerManager.InputManagerState
{
    public PlayerManagerSufficientPlayers(PlayerManager.PlayerManagerContext context,
        PlayerManager.EPlayerManagementState key,
        PlayerManager.EPlayerManagementState[] invalidTransitions)
        : base(context, key, invalidTransitions)
    {
    }

    public override void EnterState()
    {
        Context.inputManagerComponent.DisableJoining(); // Disable player joining input
        
        // determining target player state V
        
        // if setup application manager
        if (Context.playerManager.IsApplicationManager && ApplicationManager.Instance.Started)
        {
            // depends on application state
            switch (ApplicationManager.Instance.CurrentState.State)
            {
                case ApplicationManager.EApplicationState.Running:
                    Context.ChangeTargetPlayerSettingsConfigurationType(PlayerSettingsSO.EPlayerConfigurationType.Default);
                    break;
                case ApplicationManager.EApplicationState.Paused:
                    Context.ChangeTargetPlayerSettingsConfigurationType(PlayerSettingsSO.EPlayerConfigurationType.Alternate);
                    break;
                default:
                    Context.ChangeTargetPlayerSettingsConfigurationType(PlayerSettingsSO.EPlayerConfigurationType.Off);
                    break;
            }
        }
        // if no app manager or not yet setup, use context defaults
        else
        {
            Context.ChangeTargetPlayerSettingsConfigurationType(PlayerSettingsSO.EPlayerConfigurationType.Default);
        }
        
        // cursor confined and invisible since managed by player cursor components
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void UpdateState()
    {
        // If target players is greater than current players
        if (Context.NeedMorePlayers) 
        { 
            // Transition to AddingPlayers state
            Context.ContextCallChangeState(PlayerManager.EPlayerManagementState.AddingPlayers);
            
            return;
        }
        
        // If target players is less than current players
        if (Context.NeedLessPlayers) 
        {
            // Transition to RemovingPlayers state
            Context.ContextCallChangeState(PlayerManager.EPlayerManagementState.RemovingPlayers);
            
            return;
        }
    }

    public override void ExitState()
    {
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
