using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// ApplicationManager class extends BaseStateManager and exists as a persistent singleton. It manages the highest
/// level of application states such as loading scenes, running, pausing, exiting scenes, and closing the application.
/// Works as a Single Point of Entry (SPE) for application logic and state management.
/// </summary>
public class ApplicationManager : BaseStateManager<ApplicationManager, ApplicationManager.EApplicationState>
{
    /// <summary>
    /// Super states the application can be in.
    /// </summary>
    public enum EApplicationState
    {
        [Tooltip("Scene is loading.")]
        LoadingScene,
        [Tooltip("Scene is running in basic state.")]
        Running,
        [Tooltip("Scene/time is paused.")]
        Paused,
        [Tooltip("Scene is being exited.")]
        ExitingScene,
        [Tooltip("Application is closing/quitting.")]
        Closing
    }
    
    [Header("Application State Manager Settings")]
    
    [SerializeField] private  ApplicationManagerContext context;
    
    /// <summary>
    /// The currently active scene settings SO being managed by the ApplicationManager.
    /// </summary>
    public SceneSettingsSO ActiveSceneSettings => context.activeSceneSettings; 

    #region BaseMethods
    
    protected override void SetInstanceType()
    {
        InstanceType = EInstanceType.PersistentSingleton;
    }
    
    protected override void Start()
    {
        // delaying start to ensure all other managers or listeners are fully awake and enabled
        StartCoroutine(DelayedStart());
    }
    
    private IEnumerator DelayedStart()
    {
        // Wait one frame to ensure all Awake/enable calls are done (possible issue in future?)
        yield return null; 

        base.Start();
    }
    
    protected override void Initialize()
    {
        States = context.ContextInitialize(this);
    }
    
    #endregion

    #region PublicMethods
    
    /// <summary>
    /// Public method to request change game state. Internal denials can happen.
    /// </summary>
    public void RequestChangeState(EApplicationState toState)
    {
        ChangeState(toState);
    }
    
    /// <summary>
    /// Call to toggle between running and alternate running states. IE: Playing v Paused in game.
    /// (Currently, there is only one alternate state, more states could be added, logic would have to be modified).
    /// </summary>
    public void ToggleRunningOrPausedState()
    {
        // determine current state
        EApplicationState currentState = CurrentState.State;
        
        // running => paused
        if (currentState == EApplicationState.Running)
        {
            ChangeState(EApplicationState.Paused);
        }
        // paused => running
        else if (currentState == EApplicationState.Paused)
        {
            ChangeState(EApplicationState.Running);
        }
        // otherwise can't
        else
        {
            Debug.LogWarning($"Cannot toggle running state from current state: {currentState}");
        }
    }
    
    /// <summary>
    /// Takes in a Scene SO and attempts to begin exiting the current scene and loading it if valid.
    /// </summary>
    public void TryLoadScene(SceneSettingsSO targetSceneSettingsSo)
    {
        if (targetSceneSettingsSo == null) { Debug.LogWarning("Target SceneSO is null, cannot load new scene."); return; }
        
        if (DebugMode) { Debug.Log($"Trying to load scene: {targetSceneSettingsSo.TryGetScenePathAsName()}"); }
        
        //Has to have valid path
        if (SceneSettingsSO.IsValidScene(targetSceneSettingsSo))
        {
            context.LoadScene(targetSceneSettingsSo);
        }
        else
        {
            Debug.LogWarning($"Cannot currently switch to SceneSO: {targetSceneSettingsSo.name}.");
        }
    }
    
    /// <summary>
    /// Call when application is to be closed.
    /// </summary>
    public void Quit()
    {
        if (DebugMode) { Debug.Log("Quit called in ApplicationManager."); }
        
        ChangeState(EApplicationState.Closing);
    }
    
    /// <summary>
    /// On top of base, managing the transition effects during loading and exiting scenes.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        
        if (!Started || IsChangingState) return;
        
        if (context.currentTransitionTimeLeft > 0f)
        {
            switch (CurrentState.State)
            {
                case EApplicationState.LoadingScene:
                case EApplicationState.Paused:
                case EApplicationState.Running:
                    
                    float loadingTransitionTimeLeft = context.currentTransitionTimeLeft;
                    
                    float loadingTransitionPercentageLeft = loadingTransitionTimeLeft / context.activeSceneSettings.TransitionInDuration;
                    
                    float loadingI = 1 - context.activeSceneSettings.TransitionInCurve.Evaluate(loadingTransitionPercentageLeft);
                    
                    if (context.activeSceneSettings.transitionStyle ==
                        ApplicationManagerContext.ETransitionStyle.LocationTransition)
                    {
                        context.transitionImage.transform.position =
                            Vector3.Lerp(context.coveredLocation.position,
                                context.uncoveredLocation.position, loadingI);
                    }
                    else
                    {
                        context.transitionImage.color = Color.Lerp(context.activeSceneSettings.coveredColor,
                            context.activeSceneSettings.uncoveredColor, loadingI);
                    }
                    
                    context.currentTransitionTimeLeft -= Time.unscaledDeltaTime;
                    
                    if (context.currentTransitionTimeLeft <= 0f)
                    {
                        context.transitionImage.transform.position = context.uncoveredLocation.position;
                    }
                    
                    break;
                
                case EApplicationState.ExitingScene:
                    
                    float exitingTransitionTimeLeft = context.currentTransitionTimeLeft;
                    
                    float exitingTransitionPercentageLeft = exitingTransitionTimeLeft / context.activeSceneSettings.TransitionOutDuration;
                    
                    float exitingI = 1 - context.activeSceneSettings.TransitionOutCurve.Evaluate(exitingTransitionPercentageLeft);
                    
                    if (context.activeSceneSettings.transitionStyle ==
                        ApplicationManagerContext.ETransitionStyle.LocationTransition)
                    {
                        context.transitionImage.transform.position =
                            Vector3.Lerp(context.uncoveredLocation.position,
                                context.coveredLocation.position, exitingI);
                    }
                    else
                    {
                        context.transitionImage.color = Color.Lerp(context.activeSceneSettings.uncoveredColor,
                            context.activeSceneSettings.coveredColor, exitingI);
                    }
                    
                    context.currentTransitionTimeLeft -= Time.unscaledDeltaTime;
                    
                    if (context.currentTransitionTimeLeft <= 0f)
                    {
                        context.transitionImage.transform.position = context.coveredLocation.position;
                        
                        context.ContextCallChangeState(EApplicationState.LoadingScene);
                    }
                    
                    break;
            }
        }
    }

    #endregion

    [Serializable] 
    public class ApplicationManagerContext : BaseStateMachineContext
    {
        /// <summary>
        /// The style of transition to use for scene transitions.
        /// </summary>
        public enum ETransitionStyle
        {
            [Tooltip("Transition by moving an image covering the screen.")]
            LocationTransition,
            [Tooltip("Transition by changing the color/opacity of an image covering the screen.")]
            ColorTransition,
        }
        
        #region Context Declarations
        
        [Header("Inscribed References")]
        
        [Tooltip("SceneSettingsSO to load with on init; also set by self when loading a new scene. " +
                 "SET IN INSPECTOR as the corresponding sceneSO that this manager is starting in.")]
        public SceneSettingsSO targetSceneSettings;
        
        [Tooltip("The image used to cover/uncover the screen during transitions.")]
        public Image transitionImage;
        
        [Tooltip("The location where the transition image fully covers the screen.")]
        public Transform coveredLocation;
        
        [Tooltip("The location where the transition image is fully off the screen.")]
        public Transform uncoveredLocation;
        
        [Header("Dynamic References - Don't Modify in Inspector")]
            
        [Tooltip("Reference to the ApplicationManager that owns this context.")]
        public ApplicationManager applicationManager;
        
        [Tooltip("SceneSO currently being managed by the AppManager, set by self on init or when loading new scene.")]
        public SceneSettingsSO activeSceneSettings;
        
        [Header("Dynamic Settings - Don't Modify in Inspector")]
        
        [Tooltip("The current time left in the scene transition. Set by self when transitioning scenes.")]
        public float currentTransitionTimeLeft;
        
        #endregion
        
        #region BaseMethods
        
        protected override Dictionary<EApplicationState, EApplicationState[]> StatesDict()
        {
            return new Dictionary<EApplicationState, EApplicationState[]>
            {
                { EApplicationState.LoadingScene, Array.Empty<EApplicationState>() }, // No invalid transitions from loading scene
                { EApplicationState.Running, new [] { EApplicationState.LoadingScene } }, // Cannot go to loading new scene from running, must exit first
                { EApplicationState.Paused, new [] { EApplicationState.LoadingScene } }, // Cannot go to loading new scene from menu, must exit first
                { EApplicationState.ExitingScene, new [] { EApplicationState.Running, EApplicationState.Paused } }, // Cannot go to running or menu from exiting scene, must load first
                { EApplicationState.Closing, new [] { EApplicationState.LoadingScene, EApplicationState.Running, EApplicationState.Paused, EApplicationState.ExitingScene } } // Cannot go anywhere else from closing
            };
        }
        
        protected override Dictionary<EApplicationState, BaseState<EApplicationState>> InitializedStates()
        {
            Dictionary<EApplicationState, BaseState<EApplicationState>> states = 
                new Dictionary<EApplicationState, BaseState<EApplicationState>>();
            
            foreach (var state in StatesDict())
            {
                switch (state.Key)
                {
                    case EApplicationState.LoadingScene:
                        states.Add(state.Key, new ApplicationManagerLoadingScene(this, state.Key, state.Value));
                        break;
                    case EApplicationState.Running:
                        states.Add(state.Key, new ApplicationManagerRunning(this, state.Key, state.Value));
                        break;
                    case EApplicationState.Paused:
                        states.Add(state.Key, new ApplicationManagerPaused(this, state.Key, state.Value));
                        break;
                    case EApplicationState.ExitingScene:
                        states.Add(state.Key, new ApplicationManagerExitingScene(this, state.Key, state.Value));
                        break;
                    case EApplicationState.Closing:
                        states.Add(state.Key, new ApplicationManagerClosing(this, state.Key, state.Value));
                        break;
                }
            }
            
            return states;
            
        }
        
        public override Dictionary<EApplicationState, BaseState<EApplicationState>> 
                        ContextInitialize(BaseStateMachine<EApplicationState> targetStateMachine)
        {
            // cast / set the application manager reference
            applicationManager = (ApplicationManager) targetStateMachine; 
            
            // check inscribed references
            if (targetSceneSettings == null ||
                !SceneSettingsSO.IsValidScene(targetSceneSettings) ||
                transitionImage == null ||
                coveredLocation == null ||
                uncoveredLocation == null
                )
            {
                Debug.LogError($"{GetType().Name}: Error Checking Inscribed References. Destroying self.");
                
                Destroy(applicationManager.gameObject);
                
                return null;
            }
            
            currentTransitionTimeLeft = 0f;

            return InitializedStates();
        }
        
        public override void ContextCallChangeState(EApplicationState newState)
        {
            applicationManager.ChangeState(newState);
        }

        #endregion

        #region SceneMethods
        
        /// <summary>
        /// Sets the current scene SO to the one passed in, but does not initiate a load, state, or scene change.
        /// </summary>
        public void SetActiveSceneSO(SceneSettingsSO sceneSettingsSo)
        {
            // cannot set to invalid scene
            if (!SceneSettingsSO.IsValidScene(sceneSettingsSo))
            {
                Debug.LogWarning($"Unable to set current sceneSO {sceneSettingsSo.name}.");
                
                return;
            }
            
            // set active scene
            activeSceneSettings = sceneSettingsSo;
            
            //set target frame rate from scene settings
            Application.targetFrameRate = activeSceneSettings.TargetFrameRate;
            
            Application.targetFrameRate = activeSceneSettings.TargetFrameRate;
            
            if (applicationManager.DebugMode) { Debug.Log($"Set activeSceneSO to: {sceneSettingsSo.TryGetScenePathAsName()} " +
                                                          $"in {applicationManager.GetType().Name}"); }
        }
        
        /// <summary>
        /// Sets the target scene SO to the one passed in, but does not initiate a load, state, or scene change.
        /// </summary>
        public void SetTargetSceneSO(SceneSettingsSO sceneSettingsSo)
        {
            // cannot set invalid scene
            if (!SceneSettingsSO.IsValidScene(sceneSettingsSo))
            {
                Debug.LogWarning($"Unable to set target sceneSO {sceneSettingsSo.name}.");
                
                return;
            }
            
            // set target scene
            targetSceneSettings = sceneSettingsSo;
            
            if (applicationManager.DebugMode) { Debug.Log($"Set targetSceneSO to: {sceneSettingsSo.TryGetScenePathAsName()} " +
                                                          $"in {applicationManager.GetType().Name}"); }
        }
        
        /// <summary>
        /// Attempts to set a target scene and change state to exiting scene if valid.
        /// </summary>
        public void LoadScene(SceneSettingsSO newSceneSettingsSo)
        {
            // cannot change to exiting scene state
            if (!applicationManager.CanChangeToState(EApplicationState.ExitingScene)) return;
            
            // cannot load invalid scene
            if (!SceneSettingsSO.IsValidScene(newSceneSettingsSo)) { return;}
            
            if (applicationManager.DebugMode) { Debug.Log($"Exiting to scene: {newSceneSettingsSo.TryGetScenePathAsName()}"); }

            // set target scene
            SetTargetSceneSO(newSceneSettingsSo);
            
            // change to exiting scene state
            ContextCallChangeState(EApplicationState.ExitingScene);
        }
        
        public void StartRunningTransitionInScene()
        {
            currentTransitionTimeLeft = 
                activeSceneSettings != null ? activeSceneSettings.TransitionInDuration : 0f;
            
            if (applicationManager.Started && applicationManager.CurrentState.State != EApplicationState.Running)
            {
                ContextCallChangeState(EApplicationState.Running);
            }
        }
        
        public void StartExitTransitionOutScene()
        {
            currentTransitionTimeLeft = 
                activeSceneSettings != null ? activeSceneSettings.TransitionOutDuration : 0f;
            
            if (applicationManager.Started && applicationManager.CurrentState.State != EApplicationState.ExitingScene)
            {
                ContextCallChangeState(EApplicationState.ExitingScene);
            }
        }

        #endregion
    }
    
    public abstract class ApplicationManagerState : BaseState<EApplicationState>
    { 
        protected ApplicationManagerState(ApplicationManagerContext context, 
            EApplicationState key, 
            EApplicationState[] invalidTransitions) 
            : base(key, invalidTransitions)
        {
            Context = context;
        }
        
        protected ApplicationManagerContext Context { get; }
    }
}
