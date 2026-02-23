using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The PlayerInputObject (Pio) class acts as a GameObject container for a combination and mix-and-match style
/// of MonoBehaviour PioComponents that handle different aspects of player functionality such as object control,
/// camera control, ui interaction, and cursor control. Functionality is tied heavily into Unity's InputSystem package
/// and related packages like CineMachine for camera control. The Pio derives from the BaseStateMachine class,
/// using an abstract framework for defining states, transitions, and settings in those states.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerObjectPioComponent))]
[RequireComponent(typeof(PlayerCameraPioComponent))]
[RequireComponent(typeof(PlayerUiPioComponent))]
[RequireComponent(typeof(PlayerCursorPioComponent))]
public class PlayerInputObject : BaseStateMachine<PlayerInputObject.EPlayerInputObjectState>
{
    public enum EPlayerInputObjectState
    {
        [InspectorName(null)] // Don't want this to show as setting in inspector
        [Tooltip("Initializing the Player Input Object and its components, settings refs, etc.")]
        ObjectInitialize,
        [InspectorName(null)] // Don't want this to show as setting in inspector
        [Tooltip("No components or input maps active for the player, inclusion reliant on MainCamera view.")]
        Off,
        [Tooltip("PlayerObject and PlayerCamera PioComponents active with Player input map.")]
        Player,
        [Tooltip("PlayerObject, PlayerCamera, Ui(Player Object's), and PlayerCursor PioComponents active with UI input map.")]
        PlayerUi,
        [Tooltip("PlayerObject, PlayerCamera, Ui(Scene's), and PlayerCursor PioComponents active with UI input map.")]
        PlayerSceneUi,
        [Tooltip("Ui(Scene's) and PlayerCursor PioComponents active with UI input map.")]
        SceneUi
    }
    
    /// <summary>
    /// The custom context for this Pio.
    /// </summary>
    [SerializeField] private PlayerInputObjectContext context;
    
    /// <summary>
    /// Flag to know if this Pio and PioComponents are initialized and ready to use.
    /// </summary>
    private bool Initialized => context.componentsInitialized;
    
    /// <summary>
    /// Flag to know if this Pio is managed by a PlayerManager in the scene.
    /// </summary>
    public bool IsPlayerManager => PlayerManager.Instance != null;
    
    /// <summary>
    /// The event for PioComponents and anything else to subscribe to for when the PlayerSettingsSO values change.
    /// Should be called accordingly on changes.
    /// </summary>
    public event Action<PlayerSettingsSO> OnAfterPlayerSettingsChanged;
    
    /// <summary>
    /// The current PlayerSettingsSO assigned to this Pio.
    /// </summary>
    public PlayerSettingsSO CurrentPlayerSettings => context.currentPlayerSettings;
    
    /// <summary>
    /// Int representing the index of the player thinking visually in split screen from top left to bottom right (1-4)
    /// in left-to-right, top-to-bottom reading style.
    /// </summary>
    public int VisualIndex
    {
        get
        {
            if (context.playerInput == null) return -1;
            
            if (context.playerInput.splitScreenIndex == -1) return -1;
            
            return context.playerInput.splitScreenIndex + 1;
        }
    }

    #region BaseMethods

    /// <summary>
    /// Defined initialization for the Pio.
    /// </summary>
    protected override void Initialize()
    {
        States = context.ContextInitialize(this);
    }

    /// <summary>
    /// Handles logic related to cleaning up on destruction of the Pio.
    /// </summary>
    private void OnDestroy()
    {
        // unsubscribe from player manager event if exists
        if (IsPlayerManager)
        {
            PlayerManager.Instance.OnAfterPlayerManagerSettingsChanged -= context.OnAfterPlayerManagerSettingsChanged;
        }
        
        if (CurrentPlayerSettings != null)
        {
            CurrentPlayerSettings.CurrentConfigurationType = PlayerSettingsSO.EPlayerConfigurationType.Off;
        }
    }

    /// <summary>
    /// Extended Update logic for the Pio. While the BaseStateMachine uses primarily internal calls to change state,
    /// the Pio uses an added target state management system. This means that in Update, the Pio checks if it is indeed
    /// in the correct target state based on the current PlayerSettingsSO assigned to it, and if not,
    /// changes to the correct target state. Allows for things like player choice and game architecture ability in
    /// externally changing Pio states/functionality.
    /// </summary>
    protected override void Update()
    {
        // still uses base Update logic
        base.Update();
        
        // extended logic cannot happen if not initialized
        if (!Initialized) { return; }
        
        // if current state doesn't match target state from PlayerSettingsSO CurrentConfiguration,
        // change to target state
        if (CurrentState.State != context.currentPlayerSettings.CurrentConfiguration.State)
        {
            ChangeState(context.currentPlayerSettings.CurrentConfiguration.State);
        }
    }

    #endregion

    #region InputMessageMethods

    /// <summary>
    /// Public message to be received by the player's clone of InputActions in the PlayerInput component.
    /// Should be named whatever the action is called in the InputActions asset.
    /// This action is intended to toggle between ConfigurationTypes on the current PlayerSettingsSO.
    /// (Think like a pause/menu button)
    /// </summary>
    public void OnBack(InputValue value)
    {
        if (value.isPressed)
        {
            TogglePlayerSettingsConfigurationType();
        }
    }
    
    #endregion
    
    /// <summary>
    /// Method to be called to act as a manual toggle request to switch between ConfigurationTypes from
    /// the player's will. (Think the player pressing a pause/menu button to get in or out of a menu, or a UI
    /// button to do the same).
    /// </summary>
    public void TogglePlayerSettingsConfigurationType()
    {
        // Cannot toggle if BaseStateMachine not started
        if (!Started) { return; }
        
        // If PlayerSettingsSO does not allow manual switching, cannot toggle
        if (!CurrentPlayerSettings.AllowManualSwitching) { return; }
        
        // cannot toggle during certain game manager states
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState.State != GameManager.EGameState.Playing &&
             GameManager.Instance.CurrentState.State != GameManager.EGameState.Paused)
        {
            return;
        }
        
        // determine target configuration type, assigned as current first
        PlayerSettingsSO.EPlayerConfigurationType targetConfigurationType = 
            CurrentPlayerSettings.CurrentConfigurationType;
        
        // determining where to go based on where we are
        switch (CurrentPlayerSettings.CurrentConfigurationType)
        {
            // default => alternate
            case PlayerSettingsSO.EPlayerConfigurationType.Default:
                targetConfigurationType = 
                    PlayerSettingsSO.EPlayerConfigurationType.Alternate;
                break;
            // alternate => default
            case PlayerSettingsSO.EPlayerConfigurationType.Alternate:
                targetConfigurationType = 
                    PlayerSettingsSO.EPlayerConfigurationType.Default;
                break;
            // anything else, cannot toggle
            default:
                
                Debug.LogWarning($"{name}: Cannot alternate player state from " +
                                 $"{CurrentPlayerSettings.CurrentConfigurationType}");
                return;
            
        }
        
        // if managed by PlayerManager, notify of desired change so it can manage accordingly
        // (this currently can cause double calls which result in warnings, but is not (currently, XD)
        // recursive or detrimental)
        if (IsPlayerManager)
        {
            PlayerManager.Instance.OnPlayerChangePlayerSettingsConfigurationType(this, targetConfigurationType);
        }
        else
        {
            // manually calling state change in context
            context.ChangePlayerSettingsConfigurationType(targetConfigurationType);
            
            // added for manual designer testing without full PlayerManager/ApplicationManager setup
            if (GameManager.Instance != null)
            {
                switch(targetConfigurationType)
                {
                    case PlayerSettingsSO.EPlayerConfigurationType.Default:
                        GameManager.Instance.Play();
                        break;
                    case PlayerSettingsSO.EPlayerConfigurationType.Alternate:
                        GameManager.Instance.Pause();
                        break;
                }
            }
        }
    }
    
    /// <summary>
    /// Pio context class that contains all relevant references and settings for the PlayerInputObject.
    /// </summary>
    [Serializable]
    public class PlayerInputObjectContext : BaseStateMachineContext 
    {
        /// <summary>
        /// The different input action maps available for the PlayerInputObject as defined in the InputActions asset.
        /// </summary>
        public enum EInputActionMap
        {  
            None,
            Player,
            UI 
        }
    
        /// <summary>
        /// Dictionary that maps EInputActionMap enum values to their corresponding string names as defined
        /// in the InputActions asset. Null results in no action map being set.
        /// </summary>
        private static readonly Dictionary<EInputActionMap, string> ActionMapTypeNames = new ()
        {
            { EInputActionMap.None, null }, 
            { EInputActionMap.Player, "Player" },
            { EInputActionMap.UI, "UI" }
        };
        
        #region ContextDeclarations
        
        [Header("Inscribed References")]
        
        [Tooltip("The default PlayerSettingsSO to use if no PlayerManager is present.")]
        public PlayerSettingsSO defaultPlayerSettings;

        [Header("Dynamic References - Don't Modify In Inspector")]
        
        [Tooltip("The Pio script/GameObject this context belongs to.")]
        public PlayerInputObject playerInputObject;
        
        [Tooltip("The PlayerInput component on the Pio GameObject.")]
        public PlayerInput playerInput;
        
        [Tooltip("The current PlayerSettingsSO assigned to this Pio.")]
        public PlayerSettingsSO currentPlayerSettings;

        [Tooltip("The list of PioComponents attached to the Pio GameObject.")]
        public List<PioComponent> playerInputObjectComponents;

        [Header("Dynamic Settings - Don't Modify In Inspector")]

        [Tooltip("Flag to know if all PioComponents have been initialized.")]
        public bool componentsInitialized;

        #endregion
        
        #region BaseMethods
        
        protected override Dictionary<EPlayerInputObjectState, EPlayerInputObjectState[]> StatesDict()
        {
            return new Dictionary<EPlayerInputObjectState, EPlayerInputObjectState[]>
            {
                { EPlayerInputObjectState.ObjectInitialize, Array.Empty<EPlayerInputObjectState>() }, // Go anywhere
                { EPlayerInputObjectState.Off, new [] { EPlayerInputObjectState.ObjectInitialize }}, // No re init
                { EPlayerInputObjectState.Player, new [] { EPlayerInputObjectState.ObjectInitialize } }, // No re init
                { EPlayerInputObjectState.PlayerUi, new [] { EPlayerInputObjectState.ObjectInitialize } }, // No re init
                { EPlayerInputObjectState.PlayerSceneUi, new [] { EPlayerInputObjectState.ObjectInitialize } }, // No re init
                { EPlayerInputObjectState.SceneUi, new [] { EPlayerInputObjectState.ObjectInitialize } } // No re init
            };
        }

        protected override Dictionary<EPlayerInputObjectState, BaseState<EPlayerInputObjectState>> InitializedStates()
        {
            Dictionary<EPlayerInputObjectState, BaseState<EPlayerInputObjectState>> states = new();

            foreach (var state in StatesDict())
            {
                switch (state.Key)
                {
                    case EPlayerInputObjectState.ObjectInitialize:
                        states.Add(state.Key, new PioInitialize(this, state.Key, state.Value));
                        break;
                    case EPlayerInputObjectState.Off:
                        states.Add(state.Key, new PioOff(this, state.Key, state.Value));
                        break;
                    case EPlayerInputObjectState.Player:
                        states.Add(state.Key, new PioPlayerObject(this, state.Key, state.Value));
                        break;
                    case EPlayerInputObjectState.PlayerUi:
                        states.Add(state.Key, new PioPlayerObjectUi(this, state.Key, state.Value));
                        break;
                    case EPlayerInputObjectState.PlayerSceneUi:
                        states.Add(state.Key, new PioPlayerObjectSceneUi(this, state.Key, state.Value));
                        break;
                    case EPlayerInputObjectState.SceneUi:
                        states.Add(state.Key, new PioSceneUi(this, state.Key, state.Value));
                        break;
                }
            }
            
            return states;
        }

        public override Dictionary<EPlayerInputObjectState, BaseState<EPlayerInputObjectState>> 
            ContextInitialize(BaseStateMachine<EPlayerInputObjectState> targetStateMachine)
        {
            // assigning target state machine as Pio
            playerInputObject = (PlayerInputObject) targetStateMachine;
            
            // checking inscribed references
            if (defaultPlayerSettings == null)
            {
                // if errors, log and destroy go
                Debug.LogError($"{GetType().Name}: No defaultPlayerSettings assigned in Pio Context. " +
                               $"Destroying PlayerInputObject.");
                
                Destroy(playerInputObject.gameObject);
                
                return null;
            }
            
            // assigning PlayerInput component
            playerInput = playerInputObject.GetComponent<PlayerInput>();
            
            // renaming GO
            playerInputObject.name = ($"Player{playerInputObject.VisualIndex}InputObject");
            
            // try to get/assign PioComponents
            try 
            { 
                playerInputObjectComponents = new List<PioComponent>(
                    playerInputObject.GetComponents<PioComponent>());
            }
            // designed to use them so error if none found
            catch (Exception e)
            {
                Debug.LogError($"No child components found for {playerInputObject.name}\n{e}");
            }
            
            
            // if there is a player manager
            if (playerInputObject.IsPlayerManager)
            { 
                try
                {
                    // getting the settings of corresponding player number from player manager settings
                    PlayerSettingsSO targetPlayerSettings = 
                        PlayerManager.Instance.CurrentPlayerManagerSettings.PlayersSettings
                            [playerInputObject.VisualIndex - 1];

                    // getting target configuration type from player manager settings
                    PlayerSettingsSO.EPlayerConfigurationType targetConfigurationType =
                        PlayerManager.Instance.CurrentPlayerManagerSettings.TargetPlayerConfigurationType;
                
                    // plugging the target configuration settings into the matched player settings
                    targetPlayerSettings.CurrentConfigurationType = targetConfigurationType;
                    
                    // setting current player settings to matched
                    SetPlayerSettings(targetPlayerSettings);
                }
                catch (Exception e)
                {
                    // use default if error and log it
                    SetPlayerSettings(defaultPlayerSettings);
                    
                    Debug.LogError($"{playerInputObject.name}:" +
                                   $" Error in matching PlayerSettingsSO on PlayerManager init:\n{e}");
                }
                
                // subscribe to player manager settings changed event, can still recover if settings change
                PlayerManager.Instance.OnAfterPlayerManagerSettingsChanged += OnAfterPlayerManagerSettingsChanged;
            }
            // if there is no player manager, set default settings
            else
            {
                SetPlayerSettings(defaultPlayerSettings);
                
                // ensure start in default configuration type
                ChangePlayerSettingsConfigurationType(PlayerSettingsSO.EPlayerConfigurationType.Default);
            }
            
            // Pio needs states initialized for its functionality
            return InitializedStates();
        }

        /// <summary>
        /// BaseStateMachineContext method necessary to override, BUT not used due to target state management system for
        /// the Pio. Logs an error if called to indicate no effect. Use ChangePlayerSettingsConfigurationType
        /// with a configuration that contains a target state instead.
        /// </summary>
        public override void ContextCallChangeState(EPlayerInputObjectState newState)
        {
            // log error this was called and will have no effect
            Debug.LogError($"{playerInputObject.name}: ContextCallChangeState was called with {newState}, " +
                           $"but PlayerInputObject uses target state management. No effect.");
        }

        #endregion
        
        #region StateAndStateSettingsMethods
        
        /// <summary>
        /// Sets the current PlayerSettingsSO for this Pio and invokes the OnAfterPlayerSettingsChanged event.
        /// </summary>
        public void SetPlayerSettings(PlayerSettingsSO newSettings)
        {
            // cannot set if null
            if (newSettings == null)
            {
                Debug.LogError($"{playerInputObject.name}: Cannot set PlayerSettingsSO, newSettings is null.");
                return;
            }
            
            // assigning new settings
            currentPlayerSettings = newSettings;
            
            // invoking event for change
            playerInputObject.OnAfterPlayerSettingsChanged?.Invoke(newSettings);
            
            if (playerInputObject.DebugMode)
            {
                Debug.Log($"{playerInputObject.name}: Set PlayerSettingsSO to {newSettings.name}");
            }
        }
        
        /// <summary>
        /// Causes the Pio to change the current PlayerSettingsSO's configuration type to the target type given.
        /// The logic for the behaviour of that configuration type being defined in the PlayerSettingsSO and
        /// then respected by the Pio and its PioComponents.
        /// </summary>
        public void ChangePlayerSettingsConfigurationType(PlayerSettingsSO.EPlayerConfigurationType configurationType)
        {
            // cannot change if null
            if (currentPlayerSettings == null)
            {
                Debug.LogError($"{playerInputObject.name}: Cannot alternate PlayerStateSettings," +
                               $" currentPlayerSettings is null.");
                
                return;
            }
            
            // assigning new configuration type
            currentPlayerSettings.CurrentConfigurationType = configurationType;
            
            // if not managed by PlayerManager, handle certain aspects related to TargetStateSettings (like timeScale).
            if (!playerInputObject.IsPlayerManager)
            {
                Time.timeScale = currentPlayerSettings.CurrentConfiguration.PauseTime ? 0f : 1f;
            }
            
            // invoking event for change
            playerInputObject.OnAfterPlayerSettingsChanged?.Invoke(currentPlayerSettings);
            
            if (playerInputObject.DebugMode)
            {
                Debug.Log($"{playerInputObject.name}: Toggled PlayerStateSettings to " +
                          $"{currentPlayerSettings.CurrentConfiguration.State}");
            }
        }
        
        /// <summary>
        /// Gets subscribed to PlayerManager's OnAfterPlayerManagerSettingsChanged event to handle
        /// managing changes in the PlayerManagerSettingsSO that may affect this Pios settings.
        /// </summary>
        public void OnAfterPlayerManagerSettingsChanged(PlayerManagerSettingsSO currentPlayerManagerSettings)
        {
            // on change, always first ensure that we have the correct player numbers settings
            try
            {
                
                // due to nature of state system, possible this called while Pio being destroyed or target
                // player count is changing, not an error just return (but not clean ig?)
                if (playerInputObject.VisualIndex > currentPlayerManagerSettings.PlayersSettings.Count)
                {
                    return;
                }
                
                // getting the settings of corresponding player number
                PlayerSettingsSO targetPlayerSettings = 
                                currentPlayerManagerSettings.PlayersSettings[playerInputObject.VisualIndex - 1];
                
                // getting target configuration type from player manager settings
                PlayerSettingsSO.EPlayerConfigurationType targetConfigurationType =
                    currentPlayerManagerSettings.TargetPlayerConfigurationType;
                
                // if the matched settings SO is different from current, set
                if (currentPlayerSettings != targetPlayerSettings)
                {
                    SetPlayerSettings(targetPlayerSettings);
                }
                // else, still need to set target state settings
                else
                {
                    // if forced to match PlayerManager simply change to given target
                    if (currentPlayerManagerSettings.ForceConfigurationTypeMatch)
                    {
                        ChangePlayerSettingsConfigurationType(targetConfigurationType);
                    }
                    // if not forced, but currently own type is off and target is not off, change to target
                    // (this IS following player manager, but only in cases where the player manager is trying to
                    // turn on or off players, not trying to change between alternate and default or anything like that,
                    // which is more of a player choice if not following the manager)
                    else if (currentPlayerSettings.CurrentConfigurationType == PlayerSettingsSO.EPlayerConfigurationType.Off && 
                             targetConfigurationType != PlayerSettingsSO.EPlayerConfigurationType.Off)
                    {
                        ChangePlayerSettingsConfigurationType(targetConfigurationType);
                    }
                    // else if not forced but currently not off, and target is off, change to target
                    // (same as above)
                    else if (currentPlayerSettings.CurrentConfigurationType != PlayerSettingsSO.EPlayerConfigurationType.Off && 
                             targetConfigurationType == PlayerSettingsSO.EPlayerConfigurationType.Off)
                    {
                        ChangePlayerSettingsConfigurationType(targetConfigurationType);
                    }
                }
            }
            // logging error if something went wrong
            catch (Exception e)
            {
                Debug.LogError($"{playerInputObject.name}:" +
                               $" Error in matching PlayerSettingsSO on PlayerManager settings change:\n{e}");
            }
        }
        
        #endregion

        #region InputManagementMethods

        /// <summary>
        /// Function to set the current input action map. Disables all other action maps first.
        /// If target map type corresponds to null/None, all maps are disabled.
        /// Should be used when transitioning to/from states based
        /// on the desired map for relevant listener components to be able to function by receiving messages from
        /// the correct map.
        /// </summary>
        public void SetCurrentInputActionMap(EInputActionMap targetActionMapType)
        {
            //disable all action maps
            foreach (var actionMap in playerInput.actions.actionMaps)
            {
                actionMap.Disable();
            }
            
            // nothing to do if no map
            if (targetActionMapType == EInputActionMap.None)
            {
                return;
            }

            try
            {
                // set current map
                playerInput.currentActionMap = playerInput.actions.FindActionMap(ActionMapTypeNames[targetActionMapType]);
                
                // enable current map
                playerInput.currentActionMap.Enable();
                
            }
            // logging error if something went wrong (likely corresponding map not found)
            catch (Exception e)
            {
                Debug.LogError($"{playerInputObject.name}: Error in setting current action map: \n{e}");
            }
            
            if (playerInputObject.DebugMode)
            {
                Debug.Log($"{playerInputObject.name}: Set current action map to {targetActionMapType}");
            }
        }

        #endregion
    }
    
    /// <summary>
    /// Base Pio state class that all Pio states derive from.
    /// </summary>
    public abstract class NewPlayerInputObjectState : BaseState<EPlayerInputObjectState>
    {
        protected NewPlayerInputObjectState(PlayerInputObjectContext context, 
            EPlayerInputObjectState key, 
            EPlayerInputObjectState[] invalidTransitions) 
            : base(key, invalidTransitions)
        {
            Context = context;
        }
        
        protected PlayerInputObjectContext Context { get; }
    }
}
