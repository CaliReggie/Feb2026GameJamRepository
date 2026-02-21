using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
[RequireComponent(typeof(GraphicRaycaster))]
public class MainMenuManager : BaseStateManagerApplicationListener<MainMenuManager, MainMenuManager.EMainMenuUIState>
{
    public enum EMainMenuUIState
    {
        Default
    }
    
    [SerializeField] private MainMenuUIManagerContext context;

    #region BaseMethods
    
    protected override void SetInstanceType()
    {
        InstanceType = EInstanceType.Singleton;
    }
    
    protected override void Initialize()
    {
        States = context.ContextInitialize(this);
    }
    
    protected override void OnBeforeApplicationStateChange(ApplicationManager.EApplicationState toState)
    {
        if (DebugMode)
        {
            Debug.Log($"[{GetType().Name}] OnBeforeApplicationStateChange: {toState}");
        }
    }
    
    protected override void OnAfterApplicationStateChange(ApplicationManager.EApplicationState toState)
    {
        if (DebugMode)
        {
            Debug.Log($"[{GetType().Name}] OnAfterApplicationStateChange: {toState}");
        }
    }
    
    #endregion
    
    [Serializable]
    public class MainMenuUIManagerContext : BaseStateMachineContext
    {
        #region ContextDeclarations
        
        [Header("Inscribed References")]
        
        public List<GameObject> delayedUiElements;
        
        [Header("Dynamic References - Don't Modify In Inspector")]
        
        public MainMenuManager mainMenuManager;

        #endregion
        
        #region BaseMethods
        
        protected override Dictionary<EMainMenuUIState, EMainMenuUIState[]> StatesDict()
        {
            return new Dictionary<EMainMenuUIState, EMainMenuUIState[]>()
            {
                { EMainMenuUIState.Default, new EMainMenuUIState[]{ } } //No invalid transitions for Default state
            };
        }

        protected override Dictionary<EMainMenuUIState, BaseState<EMainMenuUIState>> InitializedStates()
        {
            Dictionary<EMainMenuUIState, BaseState<EMainMenuUIState>> states =
                new Dictionary<EMainMenuUIState, BaseState<EMainMenuUIState>>();
            
            foreach (var state in StatesDict())
            {
                switch (state.Key)
                {
                    case EMainMenuUIState.Default:
                        states.Add(state.Key, new MainMenuUIManagerDefault(this, state.Key, state.Value));
                        break;
                }
            }

            return states;
        }

        public override Dictionary<EMainMenuUIState, BaseState<EMainMenuUIState>> ContextInitialize(BaseStateMachine<EMainMenuUIState> targetStateMachine)
        {
            mainMenuManager = (MainMenuManager)targetStateMachine;
            
            // if delayed ui elements is null or contains null error and destroy self
            if (delayedUiElements == null || delayedUiElements.Contains(null))
            {
                Debug.LogError($"[{GetType().Name}] Delayed UI Elements list is null or contains null references." +
                               $" Destroying MainMenuManager.");
                Destroy(mainMenuManager.gameObject);
                return new Dictionary<EMainMenuUIState, BaseState<EMainMenuUIState>>();
            }
            
            return InitializedStates();
        }
        
        public override void ContextCallChangeState(EMainMenuUIState newState)
        {
            mainMenuManager.ChangeState(newState);
        }
        
        #endregion
        
        public void ToggleDelayedUIElements(bool isActive)
        {
            foreach (var uiElement in delayedUiElements)
            {
                if (uiElement != null)
                {
                    uiElement.SetActive(isActive);
                }
            }
        }
    }
    
    public abstract class MainMenuUIManagerState : BaseState<EMainMenuUIState>
    {
        protected MainMenuUIManagerContext Context { get; }
        
        protected MainMenuUIManagerState(MainMenuUIManagerContext context,
            EMainMenuUIState key,
            EMainMenuUIState[] invalidTransitions) 
            : base(key, invalidTransitions)
        {
            Context = context;
        }
    }
}
