using UnityEngine;

public class MainUIManagerTransitionInScene : MainUiManager.MainUIManagerState
{
    public MainUIManagerTransitionInScene(MainUiManager.MainUIManagerContext context,
        MainUiManager.EMainUIState key,
        MainUiManager.EMainUIState[] invalidTransitions)
        : base(context, key, invalidTransitions)
    {
    }

    private float _transitionInTimeLeft;

    public override void EnterState()
    {
        Context.transitionImage.transform.position = Context.coveredLocation.position; // cover screen
        
        if (Context.transitionStyle == MainUiManager.ETransitionStyle.ColorTransition) // if color transition
        {
            Context.transitionImage.color = Context.coveredColor; // ensure covered color
        }
        
        _transitionInTimeLeft = Context.transitionInDuration; // Set transition time left
    }
    
    public override void UpdateState()
    {
        // Calculate transition percentage left
        float transitionPercentageLeft = _transitionInTimeLeft / Context.transitionInDuration;
        
        if (transitionPercentageLeft <= 0) // Done transition if no time left
        {
            Context.ContextCallChangeState(MainUiManager.EMainUIState.Default);
            return;
        }
        
        // Use transition percentage to sample context transition out anim curve
        float i = 1 - Context.transitionInCurve.Evaluate(transitionPercentageLeft);

        // if location transition
        if (Context.transitionStyle == MainUiManager.ETransitionStyle.LocationTransition)
        {
            // Set transition image position based on anim curve
            Context.transitionImage.transform.position = 
                Vector3.Lerp(Context.coveredLocation.position, 
                    Context.uncoveredLocation.position, i);
        }
        else // if color transition
        {
            // use transition percentage to lerp color from covered to uncovered
            Context.transitionImage.color = Color.Lerp(Context.coveredColor, 
                Context.uncoveredColor, i);
        }
        
        // transition does not depend on time scale
        _transitionInTimeLeft -= Time.unscaledDeltaTime;
    }

    public override void ExitState()
    {
    }
}
