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
    
    public override void EnterState()
    {
        // Pause the game while loading
        Time.timeScale = 0f; 
        
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
        if (SceneSettingsSO.IsActiveScene(Context.targetSceneSettings)) 
        {
            //Ensure the active scene SO is set correctly
            Context.SetActiveSceneSO(Context.targetSceneSettings);
            
            // Manually setting target scene to null (usually should use public method, but this is a safe time to
            // do so and keep things clean)
            Context.targetSceneSettings = null;
            
            // Change to running state will exit this update loop
            Context.ContextCallChangeState(ApplicationManager.EApplicationState.Running);
        }
    }
    
    public override void ExitState()
    {
        Time.timeScale = 1f; // Unpause the game
    }

}
