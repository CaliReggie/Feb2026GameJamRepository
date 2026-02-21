using UnityEngine;

public class MainUIManagerTransitionOutScene : MainUiManager.MainUIManagerState
{
    public MainUIManagerTransitionOutScene(MainUiManager.MainUIManagerContext context,
        MainUiManager.EMainUIState key,
        MainUiManager.EMainUIState[] invalidTransitions)
        : base(context, key, invalidTransitions)
    {
    }
    
    private float _transitionOutTimeLeft;

    public override void EnterState()
    {
        if (Context.transitionStyle == MainUiManager.ETransitionStyle.LocationTransition) // if location transition
        {
            Context.transitionImage.transform.position = Context.uncoveredLocation.position; // uncover screen
        }
        else // if color transition
        {
            Context.transitionImage.transform.position = Context.coveredLocation.position; // cover screen
            
            Context.transitionImage.color = Context.uncoveredColor; // ensure uncovered color 
        }
        
        _transitionOutTimeLeft = Context.transitionOutDuration; // Set transition time left
    }
    
    public override void UpdateState()
    {
        // Calculate transition percentage left
        float transitionPercentageLeft = _transitionOutTimeLeft / Context.transitionOutDuration;
        
        if (transitionPercentageLeft <= 0) // Done transition if no time left
        {
            Context.ContextCallChangeState(MainUiManager.EMainUIState.Default);
            
            return;
        }
        
        // Use transition percentage to sample transition out anim curve
        float i = 1 - Context.transitionOutCurve.Evaluate(transitionPercentageLeft);

        // if location transition
        if (Context.transitionStyle == MainUiManager.ETransitionStyle.LocationTransition)
        {
            // Set transition image position based on anim curve
            Context.transitionImage.transform.position = 
                Vector3.Lerp(Context.uncoveredLocation.position, 
                    Context.coveredLocation.position, i);
        }
        else // if color transition
        {
            // use transition percentage to lerp color from uncovered to covered
            Context.transitionImage.color = Color.Lerp(Context.uncoveredColor, 
                Context.coveredColor, i);
        }
        
        // transition does not depend on time scale
        _transitionOutTimeLeft -= Time.unscaledDeltaTime;
    }

    public override void ExitState()
    {
    }
}
