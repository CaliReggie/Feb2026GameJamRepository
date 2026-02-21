using UnityEditor;
using UnityEngine;

public class ApplicationManagerClosing : ApplicationManager.ApplicationManagerState
{
    public ApplicationManagerClosing(ApplicationManager.ApplicationManagerContext context,
        ApplicationManager.EApplicationState key,
        ApplicationManager.EApplicationState[] invalidTransitions)
        : base(context, key, invalidTransitions)
    {
    }
    
    public override void EnterState()
    {
        // ensure timescale is normal
        Time.timeScale = 1f; 
    }
    
    public override void UpdateState()
    {
        // after enter logic called, quit the application (this can be made to wait for other circumstance in future)
        // quit will exit this update loop
        if (Application.isPlaying)
        { 
            #if UNITY_EDITOR
            
            EditorApplication.isPlaying = false;
            
            #else
            
            Application.Quit();
            
            #endif
        }
    }
    
    public override void ExitState()
    {
    }
}
