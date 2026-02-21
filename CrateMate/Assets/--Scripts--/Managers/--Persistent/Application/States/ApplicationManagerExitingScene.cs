using UnityEngine;

public class ApplicationManagerExitingScene : ApplicationManager.ApplicationManagerState
{
    public ApplicationManagerExitingScene(ApplicationManager.ApplicationManagerContext context,
        ApplicationManager.EApplicationState key,
        ApplicationManager.EApplicationState[] invalidTransitions)
        : base(context, key, invalidTransitions)
    {
    }
    
    public override void EnterState()
    {
        // Pause the game while exiting
        Time.timeScale = 0f; 
    }
    
    public override void UpdateState()
    {
        // If mainUIManager exists
        if (MainUiManager.Instance != null)
        {
            // don't load the scene till the transition out is complete
            if (MainUiManager.Instance.CurrentState.State == MainUiManager.EMainUIState.TransitionOutScene) return;
        }
        
        // change to loading will exit this update loop
        Context.ContextCallChangeState(ApplicationManager.EApplicationState.LoadingScene);
    }
    public override void ExitState()
    {
        Time.timeScale = 1f; // Unpause the game
    }
}
