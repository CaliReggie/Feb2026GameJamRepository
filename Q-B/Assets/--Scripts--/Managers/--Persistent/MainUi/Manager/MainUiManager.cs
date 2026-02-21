using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The MainUiManager class extends the BaseStateManagerApplicationListener to manage the main UI of the application.
/// While a lot of Ui work is done seperately by the Player Manager, Players, or other systems, this manager handles
/// the most broad and basic functions, such as scene transitions and global UI elements.
/// </summary>
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
[RequireComponent(typeof(GraphicRaycaster))]
public class MainUiManager : BaseStateManagerApplicationListener<MainUiManager, MainUiManager.EMainUIState>
{
    /// <summary>
    /// States the MainUiManager can be in.
    /// </summary>
    public enum EMainUIState
    {
        [Tooltip("Default state to be in based on application or self settings.")]
        Default,
        [Tooltip("Transitioning in scene from covered to uncovered screen.")]
        TransitionInScene,
        [Tooltip("Transitioning out scene from uncovered to covered screen.")]
        TransitionOutScene,
    }
    
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

    [SerializeField] private MainUIManagerContext context;

    #region BaseMethods

    protected override void SetInstanceType()
    {
        InstanceType = EInstanceType.PersistentSingleton;
    }
    
    protected override void Initialize()
    {
        States = context.ContextInitialize(this);
    }
    
    protected override void OnBeforeApplicationStateChange(ApplicationManager.EApplicationState toState)
    {
        if (DebugMode)
        {
            Debug.Log($"{GetType().Name}: OnBeforeApplicationStateChange to: {toState}");
        }
    }
    
    protected override void OnAfterApplicationStateChange(ApplicationManager.EApplicationState toState)
    {
        if (DebugMode)
        {
            Debug.Log($"{GetType().Name}: OnAfterApplicationStateChange to: {toState}");
        }
        
        switch (toState)
        {   
            case ApplicationManager.EApplicationState.LoadingScene:
                
                // when loading a scene, enter transition in state
                ChangeState(EMainUIState.TransitionInScene);
                
                break;
            
            case ApplicationManager.EApplicationState.ExitingScene:
                
                // when exiting a scene, enter transition out state
                ChangeState(EMainUIState.TransitionOutScene);
                
                break;
        }
    }
    
    #endregion
    
    [Serializable]
    public class MainUIManagerContext : BaseStateMachineContext
    {
        #region ContextDeclarations
        
        [Header("Inscribed References")]
        
        [Tooltip("The image used to cover/uncover the screen during transitions.")]
        public Image transitionImage;
        
        [Tooltip("The location where the transition image fully covers the screen.")]
        public Transform coveredLocation;
        
        [Tooltip("The location where the transition image is fully off the screen.")]
        public Transform uncoveredLocation;
        
        
        [Header("Inscribed Settings")]
        
        [Tooltip("The style of transition to use for scene transitions.")]
        public ETransitionStyle transitionStyle = ETransitionStyle.LocationTransition;
        
        [Space]
        
        [Tooltip("The animation curve used for transitioning in the scene.")]
        public AnimationCurve transitionInCurve = AnimationCurve.Linear(0, 0, 1, 1);
        
        [Tooltip("The duration of the transition in the scene.")]
        public float transitionInDuration = 1f;
        
        [Tooltip("The color of the transition image when the screen is uncovered.")]
        public Color uncoveredColor = Color.clear;
        
        [Space]
        
        [Tooltip("The animation curve used for transitioning out of the scene.")]
        public AnimationCurve transitionOutCurve = AnimationCurve.Linear(0, 0, 1, 1);
        
        [Tooltip("The duration of the transition out of the scene.")]
        public float transitionOutDuration = 1f;
        
        [Tooltip("The color of the transition image when the screen is covered.")]
        public Color coveredColor = Color.black;
        
        [Header("Dynamic References - Don't Modify In Inspector")]
        
        [Tooltip("Reference to the MainUiManager this context belongs to.")]
        public MainUiManager mainUIManager;
        
        #endregion
        
        #region BaseMethods
        
        protected override Dictionary<EMainUIState, EMainUIState[]> StatesDict()
        {
            return new Dictionary<EMainUIState, EMainUIState[]>
            {
                {EMainUIState.Default, new EMainUIState[] {}}, // No invalid transitions for Default state
                {EMainUIState.TransitionInScene, new EMainUIState[] { }}, // No invalid transitions for TransitionInScene state
                {EMainUIState.TransitionOutScene, new EMainUIState[] { }}, // No invalid transitions for TransitionOutScene state
            };
        }

        protected override Dictionary<EMainUIState, BaseState<EMainUIState>> InitializedStates()
        {
            Dictionary<EMainUIState, BaseState<EMainUIState>> states = 
                new Dictionary<EMainUIState, BaseState<EMainUIState>>();
            
            foreach (var state in StatesDict())
            {
                switch (state.Key)
                {
                    case EMainUIState.Default:
                        states.Add(state.Key, new MainUIManagerDefault(this, state.Key, state.Value));
                        break;
                    case EMainUIState.TransitionInScene:
                        states.Add(state.Key, 
                            new MainUIManagerTransitionInScene(this, state.Key, state.Value));
                        break;
                    case EMainUIState.TransitionOutScene:
                        states.Add(state.Key, 
                            new MainUIManagerTransitionOutScene(this, state.Key, state.Value));
                        break;
                }
            }
            
            return states;
        }

        public override Dictionary<EMainUIState, BaseState<EMainUIState>> 
            ContextInitialize(BaseStateMachine<EMainUIState> targetStateMachine)
        {
            mainUIManager = (MainUiManager) targetStateMachine;
            
            if (transitionImage == null ||
                coveredLocation == null ||
                uncoveredLocation == null)
            {
                Debug.LogError($"{GetType().Name}: Error Checking Inscribed References. Destroying self.");
                
                Destroy(mainUIManager.gameObject);
                
                return null;
            }

            return InitializedStates();
        }
        
        public override void ContextCallChangeState(EMainUIState newState)
        {
            mainUIManager.ChangeState(newState);
        }

        #endregion
    }
    
    public abstract class MainUIManagerState : BaseState<EMainUIState>
    {
        
        protected MainUIManagerState(MainUIManagerContext context, 
            EMainUIState key, 
            EMainUIState[] invalidTransitions) 
            : base(key, invalidTransitions)
        {
            Context = context;
        }
        
        protected MainUIManagerContext Context { get; }
    }
}
