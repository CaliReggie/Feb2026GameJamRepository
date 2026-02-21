using UnityEngine;

/// <summary>
/// The PioComponent class serves as a foundational modular abstract class for components that are associated with
/// a PlayerInputObject (PIO). It provides a structured framework for components to initialize and respond to
/// Pio state / settings changes, adding whatever functionality is needed in derived classes.
/// </summary>
[RequireComponent(typeof(PlayerInputObject))]
public abstract class PioComponent : MonoBehaviour
{
    [Header("Base Inscribed Settings")]

    [Tooltip("If true, component will log and enact debug behavior or information.")]
    [SerializeField] protected bool debugMode;
    
    [Header("Base Dynamic References - Don't Modify In Inspector")]

    [Tooltip("Exists for serialization purposes. No effect, click away!")]
    [SerializeField] private bool baseEmptyBool;
    
    /// <summary>
    /// The PlayerInputObject this component is associated with.
    /// </summary>
    [field: SerializeField] protected PlayerInputObject Pio { get; private set; }
    
    /// <summary>
    /// Flag indication initialization logic such as checking and setting references, along with any additional logic
    /// being completed. To be set to true at the end of Initialize().
    /// </summary>
    [field: SerializeField] public bool Initialized { get; protected set; }
    
    /// <summary>
    /// Base method handles initialization call and Pio event subscription.
    /// Initializes and turns off waiting for state changes. Make sure to set initialized to true in Initialize().
    /// </summary>
    protected virtual void Start()
    {
        if (!Initialized)
        {
            // get Pio reference
            Pio = GetComponent<PlayerInputObject>();
            
            // subscribe to Pio events
            Pio.OnBeforeStateChange += OnBeforePioStateChange;
            
            Pio.OnAfterStateChange += OnAfterPioStateChange;
            
            Pio.OnAfterPlayerSettingsChanged += OnAfterPioSettingsChange;
            
            // run initialization logic in derived class
            Initialize();
            
            // Disable script. Will enable or disable self based on Pio state changes
            enabled = false; 
        }
    }
    
    /// <summary>
    /// Base methods handles event unsubscription.
    /// </summary>
    protected virtual void OnDestroy()
    {
        // cannot unsubscribe if Pio is null
        if (Pio != null)
        {
            // unsubscribe from Pio events
            Pio.OnBeforeStateChange -= OnBeforePioStateChange;

            Pio.OnAfterStateChange -= OnAfterPioStateChange;
            
            Pio.OnAfterPlayerSettingsChanged -= OnAfterPioSettingsChange;
        }
    }
    
    /// <summary>
    /// To be defined in derived classes. This is where references should be checked and set, default or base logic run,
    /// and whatever else is needed to get the component ready to be enabled and used. Make sure to set Initialized to true.
    /// </summary>
    protected abstract void Initialize();
    
    /// <summary>
    /// To be defined in derived classes. This will be used for necessary logic to be done right before changing
    /// to a new target state.
    /// </summary>
    protected abstract void OnBeforePioStateChange(PlayerInputObject.EPlayerInputObjectState toState);
    
    /// <summary>
    /// To be defined in derived classes. This will be used for necessary logic right after a new target state has been
    /// switched to.
    /// </summary>
    protected abstract void OnAfterPioStateChange(PlayerInputObject.EPlayerInputObjectState toState);
    
    /// <summary>
    /// To be defined in derived classes. This will be used to bridge necessary logic info when player settings change.
    /// Things like preferences, cursors, spawn locations,canvas types, camera types, etc. 
    /// </summary>
    protected abstract void OnAfterPioSettingsChange(PlayerSettingsSO pioSettings);
}
