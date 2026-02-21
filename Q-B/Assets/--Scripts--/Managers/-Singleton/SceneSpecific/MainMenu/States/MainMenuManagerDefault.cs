
using UnityEngine;

public class MainMenuUIManagerDefault : MainMenuManager.MainMenuUIManagerState
{
    public MainMenuUIManagerDefault(MainMenuManager.MainMenuUIManagerContext context,
        MainMenuManager.EMainMenuUIState key,
        MainMenuManager.EMainMenuUIState[] invalidTransitions)
        : base(context, key, invalidTransitions)
    {
    }

    private PlayerInputObject pioRef;

    public override void EnterState()
    {
        Context.ToggleDelayedUIElements(false);
    }

    public override void UpdateState()
    {
        if (pioRef == null)
        {
            if (PlayerManager.Instance != null)
            {
                pioRef = PlayerManager.Instance.GetPlayer(1);
            }
            else
            {
                //find
                pioRef = Object.FindAnyObjectByType<PlayerInputObject>();
            }
            
            if (pioRef != null)
            {
                Context.ToggleDelayedUIElements(true);
            }
        }
    }

    public override void ExitState()
    {
    }
}
