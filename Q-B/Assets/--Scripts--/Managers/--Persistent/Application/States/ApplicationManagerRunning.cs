using UnityEngine;

public class ApplicationManagerRunning : ApplicationManager.ApplicationManagerState
{
    public ApplicationManagerRunning(ApplicationManager.ApplicationManagerContext context,
        ApplicationManager.EApplicationState key,
        ApplicationManager.EApplicationState[] invalidTransitions)
        : base(context, key, invalidTransitions)
    {
    }
    
    public override void EnterState()
    {
        // Ensure the game is running at normal speed
        Time.timeScale = 1f;
    }
    
    public override void ExitState()
    {
    }
    
    public override void UpdateState()
    {
    }

}
