using UnityEngine;

public class ApplicationManagerPaused : ApplicationManager.ApplicationManagerState
{
    public ApplicationManagerPaused(ApplicationManager.ApplicationManagerContext context,
        ApplicationManager.EApplicationState key,
        ApplicationManager.EApplicationState[] invalidTransitions)
        : base(context, key, invalidTransitions)
    {
    }
    
    public override void EnterState()
    {
        // Pause the game by setting time scale to 0
        Time.timeScale = 0f;
    }
    
    public override void UpdateState()
    {
        
    }
    
    public override void ExitState()
    {
        // Resume the game by setting time scale back to 1
        Time.timeScale = 1f;
    }
}
