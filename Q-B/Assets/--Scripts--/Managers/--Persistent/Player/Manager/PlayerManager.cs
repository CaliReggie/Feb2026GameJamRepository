using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputManager))]
public class PlayerManager : BaseStateManagerApplicationListener<PlayerManager, PlayerManager.EPlayerManagementState>
{
    /// <summary>
    /// The different states of player management.
    /// </summary>
    public enum EPlayerManagementState
    {
        [Tooltip("State where players are being added (via waiting on input) to reach target player count.")]
        AddingPlayers,
        [Tooltip("State where the current player count matches the target player count.")]
        SufficientPlayers,
        [Tooltip("State where players are being removed to reach target player count.")]
        RemovingPlayers
    }
    
    [SerializeField] private PlayerManagerContext context;
    
    /// <summary>
    /// Public event called after PlayerManagerSettingsSO is changed that other systems can subscribe to.
    /// (Such as PlayerInputObject classes to update their settings)
    /// </summary>
    public event Action<PlayerManagerSettingsSO> OnAfterPlayerManagerSettingsChange;
    
    /// <summary>
    /// The current PlayerManagerSettingsSO in use by the PlayerManager.
    /// </summary>
    public PlayerManagerSettingsSO CurrentPlayerManagerSettings => context.currentPlayerManagerSettings;

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
            Debug.Log($"PlayerManager: OnBeforeApplicationStateChange called with state: {toState}");
        }

        switch (toState)
        {
            case ApplicationManager.EApplicationState.LoadingScene:
                
                // setting target state to off while loading a new scene
                context.ChangeTargetPlayerSettingsConfigurationType(PlayerSettingsSO.EPlayerConfigurationType.Off);
                
                break;
            
            case ApplicationManager.EApplicationState.Running:
                
                if (ApplicationManager.Instance.CurrentState.State ==
                    ApplicationManager.EApplicationState.LoadingScene)
                {
                    // if going from loading scene, set players settings from active scene settings SO
                    context.SetPlayersSettings(ApplicationManager.Instance.ActiveSceneSettings.PlayerManagerSettings);
                }
                
                break;
        }
    }
    
    
    protected override void OnAfterApplicationStateChange(ApplicationManager.EApplicationState toState)
    {
        if (DebugMode) 
        {
            Debug.Log($"PlayerManager: OnAfterApplicationStateChange called with state: {toState}");
        }
        
        switch (toState)
        {
            case ApplicationManager.EApplicationState.Running:
                
                // if sufficient players can set target player settings to default
                if (CurrentState.State == EPlayerManagementState.SufficientPlayers)
                {
                    // we set to default settings since from running
                    context.ChangeTargetPlayerSettingsConfigurationType(PlayerSettingsSO.EPlayerConfigurationType.Default);
                }
                
                break;
            case ApplicationManager.EApplicationState.Paused:
                
                // if sufficient players can set target player settings to alternate
                if (CurrentState.State == EPlayerManagementState.SufficientPlayers)
                {
                    // we set to alternate settings since from alternate running
                    context.ChangeTargetPlayerSettingsConfigurationType(PlayerSettingsSO.EPlayerConfigurationType.Alternate);
                }
                
                break;
        }
    }
    
    protected override void OnDestroy()
    {
        // reset settings state when closing / quitting
        if (CurrentPlayerManagerSettings != null)
        {
            CurrentPlayerManagerSettings.TargetPlayerConfigurationType = PlayerSettingsSO.EPlayerConfigurationType.Off;
        }
        
        base.OnDestroy();
    }

    #endregion

    #region PublicMethods
    
    /// <summary>
    /// Way to publicly try to get a Player Input Object by its 1-based index (1-4)
    /// </summary>
    public PlayerInputObject GetPlayer(int index)
    {
        return context.GetPlayer(index);
    }

    #region PlayerManagementMethods
    
    /// <summary>
    /// Method called by a PlayerInputObject when it successfully changes its own PlayerSettings configuration type
    /// to the passed configuration type. PlayerManager will call a change to target state for all players
    /// (has validation checks).
    /// </summary>
    public void OnPlayerChangePlayerSettingsConfigurationType(PlayerInputObject requestingPlayer,
        PlayerSettingsSO.EPlayerConfigurationType configurationType)
    {
        //cannot change if player not managed by this manager
        if (context.GetPlayer(requestingPlayer) == null) return;
        
        // propagate change to all players (with validation checks)
        context.ChangeTargetPlayerSettingsConfigurationType(configurationType);
    }

    #endregion
    
    #region PlayerInputManagerBroadcastMethods

    /// <summary>
    /// Message sent by PlayerInputManager when a new player joins (manually or automatically).
    /// Passed PlayerInput GameObject should also have an attached PlayerInputObject script to be added to context.
    /// </summary>
    /// <param name="playerInput"></param> The PlayerInput component of the joined player,
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        context.AddJoinedPlayer(playerInput);
    }
    
    /// <summary>
    /// Message sent by PlayerInputManager when a player leaves
    /// (PlayerInput component/gameobject is destroyed/removed/disabled, manually or automatically)
    /// </summary>
    /// <param name="playerInput"></param> The PlayerInput component of the player that left, will be removed from context
    /// by finding associated "PlayerInputObject" if it exists. (It likely won't as this is happening after destruction)
    public void OnPlayerLeft(PlayerInput playerInput)
    {
        if (context.GetPlayer(playerInput) != null) 
        {
            context.RemovePlayer(playerInput);
        }
    }

    #endregion
    
    #endregion
    
    [Serializable]
    public class PlayerManagerContext : BaseStateMachineContext
    {
        #region ContextDeclarations
        
        [Header("Inscribed References")]
        
        [Tooltip("The default PlayerManagerSettingsSO to use on initialization.")]
        public PlayerManagerSettingsSO defaultPlayerManagerSettings;
        
        [Tooltip("The prefab GameObject for PlayerInputObject to be instantiated for new players.")]
        public GameObject playerInputObjectPrefab;
        
        [Tooltip("The parent Transform to hold instantiated PlayerInputObjects under.")]
        public Transform playersParent;
        
        [Tooltip("The Canvas to use for player alerts (like joining prompts).")]
        public Canvas alertsCanvas;

        [Tooltip("The Transform page to show when adding players.")]
        public Transform addingPlayersPage;
        
        [Tooltip("The text to alternate based on players needed.")]
        public TextMeshProUGUI playersNeededText;

        [Tooltip("The text tip to show / alter to guide players on how to join.")]
        public TextMeshProUGUI playersJoinTipText;

        public TextMeshProUGUI qbPlayersJoinTipText;
        
        [Header("Dynamic References - Don't Modify In Inspector")]
        
        [Tooltip("The PlayerManager this context belongs to.")]
        public PlayerManager playerManager;
        
        [Tooltip("The PlayerInputManager component attached to the PlayerManager GameObject.")]
        public PlayerInputManager inputManagerComponent;

        [Tooltip("The list of currently managed PlayerInputObjects.")]
        public List<PlayerInputObject> playerInputObjects;
        
        [Tooltip("The current PlayerManagerSettingsSO in use by the PlayerManager.")]
        public PlayerManagerSettingsSO currentPlayerManagerSettings;
        
        
        /// <summary>
        /// The current number of existing players, cleans null entries whenever queried. Should ALWAYS use this
        /// when needing to know current player count.
        /// </summary>
        public int NumPlayers
        {
            get
            {
                playerInputObjects.RemoveAll(playerInputObject => playerInputObject == null);
                
                return playerInputObjects.Count;
            }
        }

        /// <summary>
        /// The target number of players as defined by the current PlayerManagerSettingsSO.
        /// </summary>
        public int TargetPlayers => currentPlayerManagerSettings.TargetPlayers;
        
        /// <summary>
        /// True if more players are needed to reach target player count.
        /// </summary>
        public bool NeedMorePlayers => NumPlayers < TargetPlayers;
    
        /// <summary>
        /// True if less players are needed to reach target player count.
        /// </summary>
        public bool NeedLessPlayers => NumPlayers > TargetPlayers;
        
        #endregion
       
        #region BaseMethods

        protected override Dictionary<EPlayerManagementState, EPlayerManagementState[]> StatesDict()
        {
            return new Dictionary<EPlayerManagementState, EPlayerManagementState[]>
            {
                { EPlayerManagementState.AddingPlayers, new EPlayerManagementState[] {} }, // No invalid transitions from AddingPlayers
                { EPlayerManagementState.SufficientPlayers, new EPlayerManagementState[] {}}, // No invalid transitions from SufficientPlayers
                { EPlayerManagementState.RemovingPlayers, new EPlayerManagementState[] {} } // No invalid transitions from RemovingPlayers
            };
        }

        protected override Dictionary<EPlayerManagementState, BaseState<EPlayerManagementState>> InitializedStates()
        {
            Dictionary<EPlayerManagementState, BaseState<EPlayerManagementState>> states =
                new Dictionary<EPlayerManagementState, BaseState<EPlayerManagementState>>();

            foreach (var state in StatesDict())
            {
                switch (state.Key)
                {
                    case EPlayerManagementState.AddingPlayers:
                        states.Add(state.Key, new PlayerManagerAddingPlayers(this, state.Key, state.Value));
                        break;
                    case EPlayerManagementState.SufficientPlayers:
                        states.Add(state.Key, new PlayerManagerSufficientPlayers(this, state.Key, state.Value));
                        break;
                    case EPlayerManagementState.RemovingPlayers:
                        states.Add(state.Key, new PlayerManagerRemovingPlayers(this, state.Key, state.Value));
                        break;
                }
            }
            
            return states;
        }

        public override Dictionary<EPlayerManagementState, BaseState<EPlayerManagementState>>
            ContextInitialize(BaseStateMachine<EPlayerManagementState> targetStateMachine)
        {
            // cast / set player manager reference 
            playerManager = (PlayerManager) targetStateMachine;
            
            // get PlayerInputManager component
            inputManagerComponent = playerManager.GetComponent<PlayerInputManager>();
            
            // check inscribed references
            if (defaultPlayerManagerSettings == null ||
                playerInputObjectPrefab == null ||
                playersParent == null ||
                alertsCanvas == null ||
                addingPlayersPage == null)
            {
                Debug.LogError($"{GetType().Name}: Error Checking Inscribed References. Destroying self.");
                
                Destroy(playerManager.gameObject);
                
                return null;
            }
            
            // set PlayerInputManager player prefab
            inputManagerComponent.playerPrefab = playerInputObjectPrefab; 
            
            // Initialize the list of PlayerInput objects
            playerInputObjects = new List<PlayerInputObject>(); 
            
            // set default settings on init
            SetPlayersSettings(defaultPlayerManagerSettings);

            return InitializedStates();
        }
        
        public override void ContextCallChangeState(EPlayerManagementState newState)
        {
            playerManager.ChangeState(newState);
        }

        #endregion

        #region PlayerMethods

        #region GettingPlayers
        
        /// <summary>
        /// Private method used by GetPlayer(int) to validate index input when querying players by 1-based index(1-4).
        /// </summary>
        private bool IsValidPlayerIndex(int index)
        {
            bool isValid = index > 0 && index <= NumPlayers;
            
            // if (!isValid)
            // {
            //     Debug.LogWarning($"Index {index} playerInputObject does not exist when checking for valid player.");
            // }
            
            return isValid;
        }
        
        /// <summary>
        /// Public method to get a PlayerInputObject by its 1-based index(1-4) in the playerInputObjects list.
        /// Null if invalid.
        /// </summary>
        public PlayerInputObject GetPlayer(int index)
        {
            if (IsValidPlayerIndex(index))
            {
                PlayerInputObject targetPlayer = playerInputObjects[index - 1]; // Convert to zero-based index
                
                if (targetPlayer != null)
                {
                    if (playerManager.DebugMode)
                    {
                        Debug.Log($"Getting player at index {index}: {playerInputObjects[index].name}");
                        
                    }

                    return targetPlayer;
                }
                else
                {
                    Debug.LogWarning($"No player found at index {index}.");
                    
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        
        /// <summary>
        /// Public method to get a PlayerInputObject by its associated PlayerInput component. Null if invalid.
        /// </summary>
        public PlayerInputObject GetPlayer(PlayerInput playerInput)
        {
            if (playerInput == null)
            {
                Debug.LogWarning("Provided PlayerInput is null.");
                
                return null;
            }
            
            // try to get associated PlayerInputObject and see if managed by this PlayerManager
            PlayerInputObject targetPlayerInputObject = playerInput.GetComponent<PlayerInputObject>();
            
            if (targetPlayerInputObject != null && playerInputObjects.Contains(targetPlayerInputObject))
            {
                if (playerManager.DebugMode)
                {
                    Debug.Log($"Getting player: {targetPlayerInputObject.name}");
                }
                
                return targetPlayerInputObject;
            }
            else
            {
                return null;
            }
        }
        
        /// <summary>
        /// Public method to get a PlayerInputObject by its own reference. Null if invalid.
        /// (Seems redundant but useful for validation)
        /// </summary>
        public PlayerInputObject GetPlayer(PlayerInputObject playerInputObject)
        {
            if (playerInputObject == null)
            {
                Debug.LogWarning("GetPlayer: Provided PlayerInputObject is null.");
                
                return null;
            }
            
            // if the playerInputObject is in the managed list, return it
            if (playerInputObjects.Contains(playerInputObject))
            {
                if (playerManager.DebugMode)
                {
                    Debug.Log($"GetPlayer: Returning player: {playerInputObject.name}");
                }
                
                return playerInputObject;
            }
            else
            {
                return null;
            }
        }

        #endregion
        
        #region AddingPlayers
        
        /// <summary>
        /// Public method to catch and add a newly added Player Input component to the managed list as part of a
        /// PlayerInputObject after being added by PlayerInputManager.
        /// </summary>
        public void AddJoinedPlayer(PlayerInput playerInput)
        {
            // cannot add null player input
            if (playerInput == null)
            {
                Debug.LogWarning("Provided PlayerInput is null.");
                
                return;
            }
            
            // cannot add more players than target
            if (NumPlayers >= TargetPlayers)
            {
                Debug.LogWarning("Cannot add more players. Maximum limit reached.");
                
                Destroy(playerInput.gameObject);
                
                return;
            }
            
            // try to get attached PlayerInputObject
            PlayerInputObject attachedPlayerInputObject = playerInput.GetComponent<PlayerInputObject>();
            
            // cannot add if no attached PlayerInputObject
            if (attachedPlayerInputObject == null)
            {
                Debug.LogWarning("Provided PlayerInput does not have a PlayerInputObject component.");
                
                Destroy(playerInput.gameObject);
                
                return;
            }
            
            // see if already managed by this PlayerManager
            if (playerInputObjects.Contains(attachedPlayerInputObject))
            {
                Debug.LogWarning("Provided PlayerInput is already managed by this PlayerManager.");
                
                return;
            }
            
            // add to managed list
            playerInputObjects.Add(attachedPlayerInputObject);
            
            // set parent to the holder from context
            attachedPlayerInputObject.transform.SetParent(playersParent); 
            
            if (playerManager.DebugMode)
            {
                Debug.Log($"Added player: {attachedPlayerInputObject.name}. Total players now: {NumPlayers}");
            }
        }
        
        /// <summary>
        /// Toggles the adding players page on or off, and updates texts based on players needed.
        /// </summary>
        public void ToggleAddingPlayersPage(bool isActive, int playersNeeded = 0)
        {
            addingPlayersPage.gameObject.SetActive(isActive);
            
            //qb addition
            int currNextPlayerNum = NumPlayers + 1;
            
            String qbPlayerNeededString =
                        currNextPlayerNum == 1 ? "" :
                        currNextPlayerNum == 2 ? "2" :
                        currNextPlayerNum == 3 ? "3" :
                        currNextPlayerNum == 4 ? "4" : "Many";
            
            qbPlayersJoinTipText.text = $"first {qbPlayerNeededString}Q <b>-</b> B,\n\n\npress enter or start.\n";
            
            if (isActive)
            {
                if (playersNeeded > 1)
                {
                    String playersNeededString =
                        playersNeeded == 2 ? "Two" :
                        playersNeeded == 3 ? "Three" :
                        playersNeeded == 4 ? "Four" : "Many";
                    
                    playersNeededText.text = playersNeededString + " Players Needed";
                    playersJoinTipText.text = "New Players Press \"Enter\" or \"Start\"";
                }
                else if (playersNeeded == 1)
                {
                    playersNeededText.text = "One Player Needed";
                    playersJoinTipText.text = "New Player Press \"Enter\" or \"Start\"";
                }
                else
                {
                    playersNeededText.text = "All Players Joined";
                }
            }
        }

        #endregion

        #region RemovingPlayers

        /// <summary>
        /// Public method for removing a PlayerInputObject by its 1-based index(1-4) in the playerInputObjects list.
        /// </summary>
        public void RemovePlayer(int index)
        {
            // try to get target player by index
            PlayerInputObject targetPlayer = GetPlayer(index);
            
            if (targetPlayer != null)
            {
                playerInputObjects.Remove(targetPlayer);
                
                Destroy(targetPlayer.gameObject);
                
                if (playerManager.DebugMode)
                {
                    Debug.Log($"Removed player at index {index}: {targetPlayer.name}");
                }
            }
            else
            {
                Debug.LogWarning($"No player found at index {index}.");
            }
        }
        
        /// <summary>
        /// Public method for removing a PlayerInputObject by its associated PlayerInput component.
        /// </summary>
        public void RemovePlayer(PlayerInput playerInput)
        {
            // cannot remove null player input
            if (playerInput == null)
            {
                Debug.LogWarning("Provided PlayerInput is null.");
                
                return;
            }
            
            // try to get associated PlayerInputObject
            PlayerInputObject targetPlayerInputObject = GetPlayer(playerInput);
            
            if (targetPlayerInputObject != null)
            {
                playerInputObjects.Remove(targetPlayerInputObject);
                
                Destroy(targetPlayerInputObject.gameObject);
                
                if (playerManager.DebugMode)
                {
                    Debug.Log($"Removed player: {targetPlayerInputObject.name}");
                }
            }
            else
            {
                Debug.LogWarning("Provided PlayerInput is not managed by this PlayerManager.");
            }
        }
        
        /// <summary>
        /// Public method for removing a PlayerInputObject by its own reference.
        /// </summary>
        public void RemovePlayer(PlayerInputObject playerInputObject)
        {
            // cannot remove null player input object
            if (playerInputObject == null)
            {
                Debug.LogWarning("Provided PlayerInputObject is null.");
                
                return;
            }
            
            // try to get associated PlayerInputObject
            PlayerInputObject targetPlayerInputObject = GetPlayer(playerInputObject);
            
            if (targetPlayerInputObject != null)
            {
                playerInputObjects.Remove(targetPlayerInputObject);
                
                Destroy(targetPlayerInputObject.gameObject);
                
                if (playerManager.DebugMode)
                {
                    Debug.Log($"Removed player: {targetPlayerInputObject.name}");
                }
            }
            else
            {
                Debug.LogWarning("Provided PlayerInputObject is not managed by this PlayerManager.");
            }
        }
        
        #endregion

        #region PlayerStateAndStateSettings
        
        /// <summary>
        /// Sets the current PlayerManagerSettingsSO to the passed new settings SO and invokes event.
        /// </summary>
        public void SetPlayersSettings(PlayerManagerSettingsSO newPlayerManagerSettings)
        {
            // cannot set null settings
            if (newPlayerManagerSettings == null)
            {
                Debug.LogError("Cannot set null PlayersSettingsSO as current settings.");
                
                return;
            }
            
            if (playerManager.DebugMode)
            {
                Debug.Log($"Setting current players settings to {newPlayerManagerSettings.name} in {playerManager.GetType().Name}.");
            }
            
            // set current settings
            currentPlayerManagerSettings = newPlayerManagerSettings;
            
            // invoke event for after settings changed
            playerManager.OnAfterPlayerManagerSettingsChange?.Invoke(currentPlayerManagerSettings);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ChangeTargetPlayerSettingsConfigurationType(PlayerSettingsSO.EPlayerConfigurationType configurationType)
        {
            // cannot change if current settings is null
            if (currentPlayerManagerSettings == null)
            {
                Debug.LogError("Cannot change player state settings when current PlayersSettingsSO is null.");
                
                return;
            }
            
            //set target configuration type
            currentPlayerManagerSettings.TargetPlayerConfigurationType = configurationType;
            
            // invoke event for after settings changed
            playerManager.OnAfterPlayerManagerSettingsChange?.Invoke(currentPlayerManagerSettings);
            
            // finally manage time scale based on new settings
            ManageTimeScale();
            
            return;
            
            // manages times scaled based on settings of PlayerManagerSettingsSO and player's PlayerSettingsSOs
            void ManageTimeScale()
            {
                bool shouldPauseTime = false;
                
                switch (currentPlayerManagerSettings.TargetPlayerConfigurationType)
                {
                    case PlayerSettingsSO.EPlayerConfigurationType.Off:
                        
                        // application manager manages time in situations where players are off
                        if (playerManager.IsApplicationManager) {return;}
                        
                        break;
                    default:
                        // if all need to pause count if they all are requesting pause
                        if (currentPlayerManagerSettings.AllNeededToPauseTime)
                        {
                            int pausingPlayers = 0;
                            
                            for (int i = 0; i < currentPlayerManagerSettings.PlayersSettings.Count;  i++)
                            {
                                if (currentPlayerManagerSettings.PlayersSettings[i].CurrentConfiguration.PauseTime)
                                {
                                    pausingPlayers++;
                                }
                            }
                            
                            if (pausingPlayers == NumPlayers)
                            {
                                shouldPauseTime = true;
                            }
                            
                        }
                        // else if anyone wants to pause then pause
                        else
                        {
                            for (int i = 0; i < currentPlayerManagerSettings.PlayersSettings.Count; i++)
                            {
                                if (currentPlayerManagerSettings.PlayersSettings[i].CurrentConfiguration.PauseTime)
                                {
                                    shouldPauseTime = true;
                                    break;
                                }
                            }
                        }
                        
                        // if application manager, request state change instead of directly changing timescale
                        if (playerManager.IsApplicationManager && 
                            ApplicationManager.Instance.Started)
                        {
                            if (shouldPauseTime && 
                                ApplicationManager.Instance.CurrentState.State !=
                                ApplicationManager.EApplicationState.Paused)
                            {
                                ApplicationManager.Instance.RequestChangeState(
                                    ApplicationManager.EApplicationState.Paused);
                            }
                            else if (!shouldPauseTime && 
                                     ApplicationManager.Instance.CurrentState.State ==
                                     ApplicationManager.EApplicationState.Paused)
                            {
                                ApplicationManager.Instance.RequestChangeState(
                                    ApplicationManager.EApplicationState.Running);
                            }
                        }
                        // else if no application manager, directly change timescale
                        else
                        {
                            Time.timeScale = shouldPauseTime ? 0f : 1f;
                        }
                        
                        return;
                }
            }
        }

        #endregion
        
        #endregion
    }
    
    public abstract class InputManagerState : BaseState<EPlayerManagementState>
    {
        protected InputManagerState(PlayerManagerContext context, 
            EPlayerManagementState key, 
            EPlayerManagementState[] invalidTransitions) 
            : base(key, invalidTransitions)
        {
            Context = context;
        }
        
        protected PlayerManagerContext Context { get; }
    }
}