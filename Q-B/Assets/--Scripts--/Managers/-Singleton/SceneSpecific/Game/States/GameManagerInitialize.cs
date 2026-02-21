
using UnityEngine;

public class GameManagerInitialize : GameManager.GameManagerState
{
    public GameManagerInitialize(GameManager.GameManagerContext context,
        GameManager.EGameState key,
        GameManager.EGameState[] invalidTransitions)
        : base(context, key, invalidTransitions)
    {
    }

    public override void EnterState()
    {
        // assigning pioReference if no player manager
        if (PlayerManager.Instance == null)
        {
            Context.pioReference = Object.FindAnyObjectByType<PlayerInputObject>();
            
            // if didn't find a player log error need a player input object to manage inputs
            if (Context.pioReference == null) 
            {
                Debug.LogError($"{GetType().Name}: No PlayerManager found and" +
                               $" no PlayerInputObject found in scene. Destroying self.");
                
                Object.Destroy(Context.gameManager.gameObject);

                return;
            }
        }
        
        // toggle all pages off
        Context.TogglePlayPage(false);
        
        Context.TogglePausePage(false);

        Context.ToggleOverPage(false, false);
        
        Context.ToggleWinCountdown(false);
    }

    public override void UpdateState()
    {
        // wait for sufficient players to start playing
        if (PlayerManager.Instance != null &&
            PlayerManager.Instance.CurrentState.State == PlayerManager.EPlayerManagementState.SufficientPlayers)
        {
            Context.ContextCallChangeState(GameManager.EGameState.Playing);
        }
        // if no player manager, just start playing
        else if (PlayerManager.Instance == null &&
                 Context.pioReference != null)
        {
            Context.ContextCallChangeState(GameManager.EGameState.Playing);
        }

    }

    public override void ExitState()
    {
    }
}
