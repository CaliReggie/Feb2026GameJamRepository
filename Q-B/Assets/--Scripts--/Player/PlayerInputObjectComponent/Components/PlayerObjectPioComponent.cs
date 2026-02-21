using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


/// <summary>
/// The PlayerObjectPioComponent extends PioComponent to manage the player object GameObject. Includes a basic
/// movement framework using a player controller, but could be changed or extended for different use cases. Includes
/// logic to implement animations on the player object based on movement state.
/// </summary>
public class PlayerObjectPioComponent : PioComponent
{
    /// <summary>
    /// The various states the player object can be in based on player input and object conditions.
    /// </summary>
    public enum EPlayerObjectState
    {
        Inactive,
        Idle,
        Moving
    }
    
    [Header("Inscribed References")]
    
    [Tooltip("The transform of the player object to be controlled by this component.")]
    [SerializeField] private Transform playerObject;
    
    [SerializeField] private Rigidbody playerObjectRigidbody;

    [SerializeField] private Collider playerObjectCollider;
    
    [SerializeField] private Transform playerArmsRootTransform;
    
    [SerializeField] private Rigidbody playerArmsRigidbody;
    
    [SerializeField] private ConfigurableJoint playerArmsJoint;
    
    [SerializeField] private Collider[] playerArmsColliders;
    
    [Tooltip("The Animator component attached to the player object for state based animations.")]
    [SerializeField] private Animator animator;

    [Header("Inscribed Settings")]
    
    [SerializeField] private float maxArmExtendDistance = 2f;

    [SerializeField] private float extendDistanceOffset = 0f;

    [SerializeField] private LayerMask cursorDetectionLayers;
    
    [Header("Dynamic References - Don't Modify In Inspector")]
    
    [SerializeField] private PlayerCursorPioComponent cursorPioComponentReference;
    
    [SerializeField] private MainCamera mainCameraReference;
    
    [Header("Dynamic Settings - Don't Modify In Inspector")]
    
    [Tooltip("The target spawn position for the player object.")]
    [SerializeField] private Vector3 targetSpawnPosition;

    [Tooltip("The target spawn euler rotation for the player object.")]
    [SerializeField] private Vector3 targetSpawnEulerRotation;
    
    [Tooltip("The current state of the player object based on input/player conditions.")]
    [SerializeField] private EPlayerObjectState currentState;

    [SerializeField] private bool extendArmsPressed;
    
    [SerializeField] private Vector3 targetCursorHitPosition;
    
    // extending the x position of joint to using this (difference in position of
    // root transform and hit point) clamped to maxArmExtendDistance
    private float TargetArmXExtension => Mathf.Clamp(Vector3.Distance(targetCursorHitPosition, playerArmsRootTransform.position) + extendDistanceOffset, 0f, maxArmExtendDistance);
    
    // rotating the joint to look at the hit point using the difference in position
    // of root transform and hit point to get the angle
    private float TargetArmZRotation
    {
        get
        {
            Vector3 directionToHit = targetCursorHitPosition - playerArmsRootTransform.position;
            
            return -Mathf.Atan2(directionToHit.y, directionToHit.x) * Mathf.Rad2Deg;
        }
    }
    
    /// <summary>
    /// The current world position of the player object.
    /// </summary>
    public Vector3 CurrentObjectPosition => playerObject.position;
    
    /// <summary>
    /// The current euler rotation of the player object.
    /// </summary>
    public Vector3 CurrentObjectEulerRotation => playerObject.rotation.eulerAngles;
    
    public void OnExtendArms(InputValue extendArmsButtonValue)
    {
        extendArmsPressed = extendArmsButtonValue.isPressed;
    }
    
    /// <summary>
    /// Teleports the player object to the target location. Optionally also rotates to match target Euler rotation.
    /// </summary>
    public void TpPlayerObject(Vector3 targetPosition, Vector3 targetEulerRotation, bool alsoRotate)
    {
        // if the character controller is enabled and active we have to use its move method
        if (playerObject.gameObject.activeSelf)
        {
            playerObjectRigidbody.Move(targetPosition, alsoRotate ? Quaternion.Euler(targetEulerRotation) : playerObject.rotation);
        }
        // otherwise use transform directly
        else
        {
            playerObject.position = targetPosition;
            
            playerObject.SetPositionAndRotation(targetPosition, alsoRotate ? Quaternion.Euler(targetEulerRotation) : playerObject.rotation);
        }
    }
    
    protected override void Initialize()
    {
        // check inscribed references
        if (!CheckInscribedReferences()) return;

        //set dynamic references
        if (!SetDynamicReferences()) return;
        
        // ensure player starts despawned
        DeSpawn();
        
        Initialized = true;
        
        return;
        
        // check that all inscribed references are set
        bool CheckInscribedReferences()
        {
            if (playerObject == null ||
                animator == null ||
                playerObjectRigidbody == null ||
                playerObjectCollider == null ||
                playerArmsRootTransform == null ||
                playerArmsRigidbody == null ||
                playerArmsJoint == null ||
                playerArmsColliders == null ||
                playerArmsColliders.Contains(null))
            {
                Debug.LogError($"{GetType().Name}: Error checking inscribed references.");
                
                return false;
            }
            
            return true;
        }
        
        // set and check dynamic references
        bool SetDynamicReferences()
        {
            try
            {
                cursorPioComponentReference = Pio.GetComponent<PlayerCursorPioComponent>();
                
                mainCameraReference = MainCamera.Instance;
                
                if (cursorPioComponentReference == null ||
                    mainCameraReference == null)
                {
                    Debug.LogError($"{GetType().Name}: Error setting dynamic references.");
                    
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"{GetType().Name}: Exception setting dynamic references: {e.Message}");
                
                return false;
            }
            
            return true;
        }
    }
    
    protected override void OnBeforePioStateChange(PlayerInputObject.EPlayerInputObjectState toState)
    {
        switch (toState)
        {   
            case PlayerInputObject.EPlayerInputObjectState.Off:
            case PlayerInputObject.EPlayerInputObjectState.SceneUi:
                
                // if on, deactivate
                if (enabled)
                {
                    DeSpawn();
                    
                    enabled = false;
                }
                
                break;
            
        }
    }
    
    protected override void OnAfterPioStateChange(PlayerInputObject.EPlayerInputObjectState toState)
    {
        switch (toState)
        {   
            case PlayerInputObject.EPlayerInputObjectState.Player:
            case PlayerInputObject.EPlayerInputObjectState.PlayerUi:
            case PlayerInputObject.EPlayerInputObjectState.PlayerSceneUi:
                
                // if off, activate
                if (!enabled)
                {
                    Spawn();

                    enabled = true;
                }
                
                break;
        }
    }

    protected override void OnAfterPioSettingsChange(PlayerSettingsSO playerSettings)
    {
        // can add the spawn point logic here if desired or required
    }

    /// <summary>
    /// Spawns player at the current target spawn point (default if none set).
    /// </summary>
    private void Spawn()
    {
        // updating ref for main camera here in case of change
        mainCameraReference = MainCamera.Instance;
        
        if (mainCameraReference == null)
        {
            Debug.LogError($"{GetType().Name}: No MainCamera instance found in scene.");
        }
        
        
        // on spawn, getting most recent spawn point from player settings SO in case it was changed while despawned
        if (Pio.CurrentPlayerSettings != null)
        {
            if (Pio.CurrentPlayerSettings.UseEnvironmentSpawnPoint)
            {
                SceneEnvironment sceneEnvironment = FindAnyObjectByType<SceneEnvironment>();
                
                if (sceneEnvironment != null)
                {
                    targetSpawnPosition = sceneEnvironment.SpawnPoint.position;
                    
                    targetSpawnEulerRotation = sceneEnvironment.SpawnPoint.rotation.eulerAngles;
                }
                else
                {
                    Debug.LogError($"{GetType().Name}: No SceneEnvironment found in scene," +
                                   $" cannot use environment spawn point.");
                    
                    targetSpawnPosition = Pio.CurrentPlayerSettings.SpawnPosition;
            
                    targetSpawnEulerRotation = Pio.CurrentPlayerSettings.SpawnEulerRotation;
                }
            }
            else
            {
                targetSpawnPosition = Pio.CurrentPlayerSettings.SpawnPosition;
            
                targetSpawnEulerRotation = Pio.CurrentPlayerSettings.SpawnEulerRotation;
            }
        }
        
        // teleport to spawn point
        TpPlayerObject(targetSpawnPosition, targetSpawnEulerRotation, true);
        
        ResetDynamicSettings();
        
        // turn on player object
        playerObject.gameObject.SetActive(true);
    }

    /// <summary>
    /// Resets dynamic movement settings and deactivates player object.
    /// </summary>
    private void DeSpawn()
    {
        // teleport to spawn point
        TpPlayerObject(targetSpawnPosition, targetSpawnEulerRotation, true);
        
        ResetDynamicSettings();
        
        // turn off player object
        playerObject.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Used to reset all dynamic settings related to movement and state of the player object and go back to idle.
    /// DOES NOT reset spawn position/rotation.
    /// </summary>
    private void ResetDynamicSettings()
    {
        // arms should always start in the same position relative to the body on spawn
        targetCursorHitPosition = CurrentObjectPosition;
        
        // joint needs to be reset
        playerArmsJoint.targetPosition = Vector3.zero;
        playerArmsJoint.targetRotation = Quaternion.identity;
        playerArmsJoint.transform.localPosition = Vector3.zero;
        playerArmsJoint.transform.localRotation = Quaternion.identity;
        
        // rigidbody velocity should be reset
        playerObjectRigidbody.linearVelocity = Vector3.zero;
        playerObjectRigidbody.angularVelocity = Vector3.zero;
        
        extendArmsPressed = false;
        
        HandleChangeState(EPlayerObjectState.Idle);
    }
    
    private void FixedUpdate()
    {
        // cannot move if not initialized
        if (!Initialized) { return; }
    }

    private void Update()
    {
        if (!Initialized) { return; }

        ManageDetectionValues();
        
        ManageJointValues();
        
        ManageState();
        
        return;
        
        void ManageDetectionValues()
        {
            if (cursorPioComponentReference.CursorInstance != null)
            {
                // raycast from camera to cursor position to get hit point for arms to reach towards
                Ray ray = mainCameraReference.Camera.ScreenPointToRay(
                    cursorPioComponentReference.CursorInstance.transform.position);
                
                if (Physics.Raycast(ray, out RaycastHit hitInfo, 10000f, cursorDetectionLayers))
                {
                    targetCursorHitPosition = hitInfo.point;
                }
            }
            
            
        }
        
        void ManageJointValues()
        {
            playerArmsJoint.targetPosition = extendArmsPressed ? new Vector3(TargetArmXExtension, 0f, 0f) : Vector3.zero;
            
            playerArmsJoint.targetRotation = Quaternion.Euler(0f, 0f, TargetArmZRotation - 180);
        }
        
        void ManageState()
        {
            EPlayerObjectState previousState = currentState;

            EPlayerObjectState targetState = previousState;
            
            // change state if needed
            if (targetState != previousState)
            {
                HandleChangeState(targetState);
            }
        }
    }
    
    /// <summary>
    /// Handle logic switching from the current state to a new target state. Cannot switch to same state.
    /// Should not be used to force a state, only to handle logic when a state change is needed.
    /// </summary>
    private void HandleChangeState(EPlayerObjectState toState)
    {
        if (currentState == toState) { return; }
        
        if (debugMode)
        {
            Debug.Log($"{GetType().Name}: {Pio.gameObject.name} changing to state: {toState}");
        }
        
        EPlayerObjectState previousState = currentState;

        // logic for exiting these states
        switch (previousState)
        {
            case EPlayerObjectState.Inactive:
                break;
            case EPlayerObjectState.Idle:
                break;
            case EPlayerObjectState.Moving:
                // todo: cache animator hashes
                animator.SetBool("isMoving", false);
                break;
        }
        
        // logic for entering these states
        switch (toState)
        {
            case EPlayerObjectState.Inactive:
                break;
            case EPlayerObjectState.Idle:
                break;
            case EPlayerObjectState.Moving:
                animator.SetBool("isMoving", true);
                break;
        }
        
        currentState = toState;
    }
}