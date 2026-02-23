using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
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
    
    public void ToggleWinCountdown(bool shouldBeCounting)
    {
        context.ToggleWinCountdown(shouldBeCounting);
    }
    
    public void Play()
    {
        context.ContextCallChangeState(EGameState.Playing);
    }
    
    public void Pause()
    {
        context.ContextCallChangeState(EGameState.Paused);
    }
    
    public void GameOver(bool gameWon)
    {
        context.gameWon = gameWon;
        
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
    
    protected override void Start()
    {
        base.Start();
        
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnBeforeStateChange += OnBeforePlayerManagerStateChange;
            
            PlayerManager.Instance.OnAfterStateChange += OnAfterPlayerManagerStateChange;
            
            if (DebugMode)
            {
                Debug.Log($"{GetType().Name}: Subscribed to PlayerManager events.");
            }
        }
        else
        {
            Debug.LogWarning($"{GetType().Name}: No PlayerManager instance found in scene. " +
                             $"Functionality will be limited without.");
        }
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnBeforeStateChange -= OnBeforePlayerManagerStateChange;
            
            PlayerManager.Instance.OnAfterStateChange -= OnAfterPlayerManagerStateChange;
            
            if (DebugMode)
            {
                Debug.Log($"{GetType().Name}: Unsubscribed from PlayerManager events.");
            }
        }
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
                    if (CurrentState.State == EGameState.Playing)
                    {
                        context.ContextCallChangeState(EGameState.Paused);
                    }
                }
                
                break;
        }
    }
    
    protected virtual void OnBeforePlayerManagerStateChange(PlayerManager.EPlayerManagementState fromState)
    {
        if (DebugMode)
        {
            Debug.Log($"[{GetType().Name}] OnBeforePlayerManagerStateChange: {fromState}");
        }
    }
    
    protected virtual void OnAfterPlayerManagerStateChange(PlayerManager.EPlayerManagementState toState)
    {
        if (DebugMode)
        {
            Debug.Log($"[{GetType().Name}] OnAfterPlayerManagerStateChange: {toState}");
        }
        
        switch (toState)
        {
            case PlayerManager.EPlayerManagementState.SufficientPlayers:
                
                // if just got sufficient players and application manager
                // exists and is in running state, go to Playing state
                if (ApplicationManager.Instance != null &&
                    ApplicationManager.Instance.CurrentState.State == ApplicationManager.EApplicationState.Running)
                {
                    context.ContextCallChangeState(EGameState.Playing);
                }
                
                break;
            case PlayerManager.EPlayerManagementState.AddingPlayers:
            case PlayerManager.EPlayerManagementState.RemovingPlayers:
                
                // if not at sufficient players anymore, reset with initialize and wait for sufficient players again
                context.ContextCallChangeState(EGameState.Initialize);
                
                break;
        }
    }

    #endregion

    [Serializable]
    public class GameManagerContext : BaseStateMachineContext
    {
        #region ContextDeclarations

        [Header("Inscribed References")]
        
        [Tooltip("The root play page transform.")]
        public Transform playPage;

        [Tooltip("The root pause page transform.")]
        public Transform pausePage;
        
        [Tooltip("The root game lost page transform.")]
        public Transform gameWonPage;
        
        [Tooltip("The root game lost page transform.")]
        public Transform gameLostPage;
        
        public Transform winCountdown;
        
        [Header("Inscribed Settings")]
        
        [Tooltip("Whether to hide the play page when Paused.")]
        public bool hidePlayWhenPaused;
        
        [Tooltip("Whether to hide the play page when GameOver.")]
        public bool hidePlayWhenOver;
        
        [Header("Dynamic References - Don't Modify In Inspector")]
        
        public GameManager gameManager;
        
        public PlayerInputObject singlePlayerPioReference;
        
        [Header("Dynamic Settings - Don't Modify In Inspector")]
        
        public bool gameWon;
        
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
                gameWonPage == null ||
                gameLostPage == null ||
                winCountdown == null)
            {
                Debug.LogError($"{GetType().Name}: Error Checking Inscribed References. Destroying self.");
                
                Destroy(gameManager.gameObject);
                
                return null;
            }

            return InitializedStates();
        }

        public override void ContextCallChangeState(EGameState newState)
        {
            gameManager.ChangeState(newState);
        }
        
        #endregion
        
        public void TogglePlayPage(bool isActive)
        {
            playPage.gameObject.SetActive(isActive);
        }
        
        public void TogglePausePage(bool isActive)
        {
            pausePage.gameObject.SetActive(isActive);
        }
        
        public void ToggleOverPage(bool won, bool isActive)
        {
            if (won)
            {
                gameWonPage.gameObject.SetActive(isActive);
                gameLostPage.gameObject.SetActive(false);
            }
            else
            {
                gameLostPage.gameObject.SetActive(isActive);
                gameWonPage.gameObject.SetActive(false);
            }
        }
        
        public void ToggleWinCountdown(bool shouldBeCounting)
        {
            winCountdown.gameObject.SetActive(shouldBeCounting);
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
