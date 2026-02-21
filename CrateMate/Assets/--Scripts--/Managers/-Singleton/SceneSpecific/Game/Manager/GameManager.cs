using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
[RequireComponent(typeof(GraphicRaycaster))]
public class GameManager : BaseStateManagerApplicationListener<GameManager, GameManager.EGameState>
{
    /// <summary>
    /// The different states of the Game Manager.
    /// </summary>
    public enum EGameState
    {
        [Tooltip("Game is initializing / resetting.")]
        Initialize,
        [Tooltip("Game is being played")]
        Playing,
        [Tooltip("Game is paused.")]
        Paused,
        [Tooltip("Game is over.")]
        GameOver
    }
    
    /// <summary>
    /// Called to end the game and transition to GameOver state.
    /// </summary>
    public void GameOver(bool gameWon)
    {
        context.ContextCallChangeState(EGameState.GameOver);
    }

    
    [SerializeField] private GameManagerContext context;
    
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

        switch (toState)
        {
            case ApplicationManager.EApplicationState.Running:
                
                // if going to running and PlayerManager with sufficient players exists, go to Playing state
                if (PlayerManager.Instance != null &&
                    PlayerManager.Instance.CurrentState.State == PlayerManager.EPlayerManagementState.SufficientPlayers)
                {
                    context.ContextCallChangeState(EGameState.Playing);
                }
                
                break;
                
            case ApplicationManager.EApplicationState.Paused:
                
                // if going to paused, check if any player needs to see the scene pause UI and show it if so
                if (PlayerManager.Instance != null &&
                    PlayerManager.Instance.CurrentState.State == PlayerManager.EPlayerManagementState.SufficientPlayers)
                {
                    context.ContextCallChangeState(EGameState.Paused);
                }
                
                break;
        }
    }
    
    #endregion
    
    /// <summary>
    /// Used for manual changes when application manager is not present. Implemented for designer testing without
    /// full application manager setup.
    /// </summary>
    public void ManualChangeState(EGameState newState)
    {
        context.ContextCallChangeState(newState);
    }

    [Serializable]
    public class GameManagerContext : BaseStateMachineContext
    {
        #region ContextDeclarations

        [Header("Inscribed References")]
        
        [Tooltip("The root play page transform.")]
        public Transform playPage;

        [Tooltip("The root pause page transform.")]
        public Transform pausePage;
        
        [Tooltip("The root game over page transform.")]
        public Transform overPage;
        
        [Header("Inscribed Settings")]
        
        [Tooltip("Whether to hide the play page when Paused.")]
        public bool hidePlayWhenPaused;
        
        [Tooltip("Whether to hide the play page when GameOver.")]
        public bool hidePlayWhenOver;
        
        [Header("Dynamic References - Don't Modify In Inspector")]
        
        public GameManager gameManager;

        public PlayerInputObject pioReference;
        #endregion
        
        #region BaseMethods

        protected override Dictionary<EGameState, EGameState[]> StatesDict()
        {
            return new Dictionary<EGameState, EGameState[]>()
            {
                { EGameState.Initialize, new [] { EGameState.GameOver } }, //Cannot transition from Initialize to GameOver
                { EGameState.Playing, new EGameState[]{ } }, //No invalid transitions for Playing state
                { EGameState.Paused, new [] { EGameState.GameOver } }, //Cannot transition from Paused to GameOver
                { EGameState.GameOver, new [] { EGameState.Playing, EGameState.Paused} } //Cannot transition from GameOver to Playing or Paused
            };
        }

        protected override Dictionary<EGameState, BaseState<EGameState>> InitializedStates()
        {  
            Dictionary<EGameState, BaseState<EGameState>> states
                = new Dictionary<EGameState, BaseState<EGameState>>();

            foreach (var state in StatesDict())
            {
                switch (state.Key)
                {
                    case EGameState.Initialize:
                        states.Add(state.Key, new GameManagerInitialize(this, state.Key, state.Value));
                        break;
                    case EGameState.Playing:
                        states.Add(state.Key, new GameManagerPlaying(this, state.Key, state.Value));
                        break;
                    case EGameState.Paused:
                        states.Add(state.Key, new GameManagerPaused(this, state.Key, state.Value));
                        break;
                    case EGameState.GameOver:
                        states.Add(state.Key, new GameManagerOver(this, state.Key, state.Value));
                        break;
                }
            }

            return states;
        }

        public override Dictionary<EGameState, BaseState<EGameState>> ContextInitialize(BaseStateMachine<EGameState> targetStateMachine)
        {
            gameManager = (GameManager) targetStateMachine;
            
            if (pausePage == null ||
                playPage == null ||
                overPage == null)
            {
                Debug.LogError($"{GetType().Name}: Error Checking Inscribed References. Destroying self.");
                
                Destroy(gameManager.gameObject);
                
                return null;
            }
            
            return InitializedStates();
        }

        public override void ContextCallChangeState(EGameState newState)
        {
            if (newState != EGameState.Initialize)
            {
                if (PlayerManager.Instance != null)
                {
                    pioReference = PlayerManager.Instance.GetPlayer(1);
                    
                    if (pioReference == null)
                    {
                        Debug.LogError($"{GetType().Name}: No PlayerInputObject found for Player 1. " +
                                       $"Cannot start Game. Staying in Initialize state.");
                        
                        return;
                    }
                }
            }
            
            gameManager.ChangeState(newState);
        }
        
        #endregion
        
        public void TogglePlayPage(bool isActive)
        {
            if (playPage != null)
            {
                playPage.gameObject.SetActive(isActive);
            }
        }
        
        public void TogglePausePage(bool isActive)
        {
            if (pausePage != null)
            {
                pausePage.gameObject.SetActive(isActive);
            }
        }
        
        public void ToggleOverPage(bool isActive)
        {
            if (overPage != null)
            {
                overPage.gameObject.SetActive(isActive);
            }
        }
    }
    
    public abstract class GameManagerState : BaseState<EGameState>
    {
        protected GameManagerContext Context { get; }
        
        protected GameManagerState(GameManagerContext context,
            EGameState key,
            EGameState[] invalidTransitions) 
            : base(key, invalidTransitions)
        {
            Context = context;
        }
    }
}
