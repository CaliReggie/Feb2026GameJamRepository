using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationManagerLoadingScene : ApplicationManager.ApplicationManagerState
{
    public ApplicationManagerLoadingScene(ApplicationManager.ApplicationManagerContext context,
        ApplicationManager.EApplicationState key,
        ApplicationManager.EApplicationState[] invalidTransitions)
        : base(context, key, invalidTransitions)
    {
    }

    private bool _isFirstEverEnter = true;
    
    private bool _didFirstValidUpdateOccur; // Used to track if the first valid update has occurred
                                            // (i.e., target scene is active)
    
    public override void EnterState()
    {
        _didFirstValidUpdateOccur = false; // Reset the flag for the first valid update
        
        // Pause the game while loading
        Time.timeScale = 0f; 
        
        // ensure covered screen
        Context.transitionImage.transform.position = Context.coveredLocation.position;
        
        // if target scene settings is not null, ensure covered color if color transition
        if (Context.targetSceneSettings != null)
        {
            if (Context.targetSceneSettings.transitionStyle == 
                ApplicationManager.ApplicationManagerContext.ETransitionStyle.ColorTransition)
            {
                Context.transitionImage.color = Context.targetSceneSettings.CoveredColor;
            }
        }
        
        // if first enter ever, don't load scene if already active
        if (_isFirstEverEnter)
        {
            _isFirstEverEnter = false;
            
            if (SceneSettingsSO.IsActiveScene(Context.targetSceneSettings))
            {
                
                return;
            }
        }
        
        //load the target scene
        SceneManager.LoadScene(Context.targetSceneSettings.TryGetScenePathAsName());
    }
    
    public override void UpdateState()
    {
        // If target scene is indeed active
        if (!_didFirstValidUpdateOccur && SceneSettingsSO.IsActiveScene(Context.targetSceneSettings)) 
        {
            //Ensure the active scene SO is set correctly
            Context.SetActiveSceneSO(Context.targetSceneSettings);
            
            // Manually setting target scene to null (usually should use public method, but this is a safe time to
            // do so and keep things clean)
            Context.targetSceneSettings = null;
            
            _didFirstValidUpdateOccur = true; // Set the flag to indicate the first valid update has occurred

            Context.StartRunningTransitionInScene();
        }
    }
    
    public override void ExitState()
    {
        Time.timeScale = 1f; // Unpause the game
    }

}
