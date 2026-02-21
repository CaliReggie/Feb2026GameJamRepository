
public class MainUIManagerDefault : MainUiManager.MainUIManagerState
{
    public MainUIManagerDefault(MainUiManager.MainUIManagerContext context,
        MainUiManager.EMainUIState key,
        MainUiManager.EMainUIState[] invalidTransitions)
        : base(context, key, invalidTransitions)
    {
    }

    public override void EnterState()
    {
        // Logic depends on existence of AppManager
        
        // If yes
        if (Context.mainUIManager.IsApplicationManager) 
        {
            //If application initialized and Running or Paused
            if (ApplicationManager.Instance.CurrentState != null && (
                ApplicationManager.Instance.CurrentState.State == ApplicationManager.EApplicationState.Running ||
                ApplicationManager.Instance.CurrentState.State == ApplicationManager.EApplicationState.Paused))
            {
                Context.transitionImage.transform.position =  // ensure uncovered screen
                    Context.uncoveredLocation.position;
                
                if (Context.transitionStyle == MainUiManager.ETransitionStyle.ColorTransition) // if color transition
                {
                    Context.transitionImage.color = Context.uncoveredColor; // ensure uncovered color
                }
            }
            else // if (Loading, Exiting, or Quitting)
            {
                Context.transitionImage.transform.position = // ensure covered screen
                Context.coveredLocation.position;
                
                if (Context.transitionStyle == MainUiManager.ETransitionStyle.ColorTransition) // if color transition
                {
                    Context.transitionImage.color = Context.coveredColor; // ensure covered color
                }
            }
        }
        else // If no AppManager
        {
            Context.transitionImage.transform.position =
                Context.uncoveredLocation.position; // ensure uncovered screen
            
            if (Context.transitionStyle == MainUiManager.ETransitionStyle.ColorTransition) // if color transition
            {
                Context.transitionImage.color = Context.uncoveredColor; // ensure uncovered color
            }
        }
    }
    
    public override void UpdateState()
    {
    }
    
    public override void ExitState()
    {
    }

}