using System;
using UnityEngine;
using UnityEngine.InputSystem;


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
        Walking,
        Jumping,
        Falling
    }
    
    [Header("Inscribed References")]
    
    [Tooltip("The transform of the player object to be controlled by this component.")]
    [SerializeField] private Transform playerObject;
    
    [Tooltip("The Animator component attached to the player object for state based animations.")]
    [SerializeField] private Animator animator;
    
    [Header("Inscribed Settings")]
    
    [Tooltip("The speed and which player object will move when move controls are used.")]
    [SerializeField] private float walkSpeed = 5f;
    
    [Tooltip("The real height the player will jump dependant on Physics gravity settings.")]
    [SerializeField] private float jumpHeight = 2f;
    
    [Tooltip("The time after pressing jump that playerObject will still attempt to jump (for jump forgiveness in air)")]
    [SerializeField] private float jumpBufferDuration = 0.2f;
    
    [Tooltip("The time after exiting grounded that playerObject will still attempt to jump (for coyote time)")]
    [SerializeField] private float jumpCoyoteBufferDuration = 0.2f;
    
    [Tooltip("The speed at which the player object will rotate towards target move when in a non-fixed player camera.")]
    [SerializeField] [Range(0,1)] private float playerRotationEasing = 0.1f;
    
    [Header("Dynamic References - Don't Modify In Inspector")]
    
    [Tooltip("The CharacterController component from the player object.")]
    [SerializeField] private CharacterController characterController;
    
    [Tooltip("The PlayerCameraPioComponent attached to the PlayerInputObject.")] 
    [SerializeField] private PlayerCameraPioComponent playerCameraComponent;
    
    [Header("Dynamic Settings - Don't Modify In Inspector")]
    
    [Tooltip("The target spawn position for the player object.")]
    [SerializeField] private Vector3 targetSpawnPosition;

    [Tooltip("The target spawn euler rotation for the player object.")]
    [SerializeField] private Vector3 targetSpawnEulerRotation;
    
    [Tooltip("The raw move input received from players input regardless of view orientation.")]
    [SerializeField] private Vector3 rawMoveInput;
    
    [Tooltip("The move input oriented to the current CurrentLookOrientation.")]
    [SerializeField] private Vector3 orientedMoveInput;
    
    [Tooltip("The target world space move vector the player object will move towards.")]
    [SerializeField] private Vector3 targetMove;
    
    [Tooltip("The target euler rotation the player object will rotate towards.")]
    [SerializeField] private Vector3 targetEulerRotation;
    
    [Tooltip("The current state of the player object based on input/player conditions.")]
    [SerializeField] private EPlayerObjectState currentState;
    
    [Tooltip("Is the player object currently grounded according to CharacterController.")]
    [SerializeField] private bool isGrounded;

    [Tooltip("Was the player object grounded last frame according to CharacterController.")]
    [SerializeField] private bool wasGrounded;
    
    [Tooltip("The time left that the player will still attempt to jump.")]
    [SerializeField] private float jumpBufferTimer;
    
    [Tooltip("The time left that the player can still attempt to jump if not grounded.")]
    [SerializeField] private float jumpCoyoteBufferTimer;
    
    /// <summary>
    /// True if grounded and jump requested within jump buffer time,
    /// or if not grounded but jump requested within jump buffer time and
    /// jump coyote time is still active.
    /// </summary>
    private bool JumpRequested => isGrounded ? jumpBufferTimer > 0f : 
        (jumpBufferTimer > 0f && jumpCoyoteBufferTimer > 0f);
    
    /// <summary>
    /// The look orientation transform of current camera view per dynamically set playerCameraComponent.
    /// </summary>
    private Transform CurrentLookOrientation => playerCameraComponent.CurrentLookOrientation;
    
    /// <summary>
    /// The current world position of the player object.
    /// </summary>
    public Vector3 CurrentObjectPosition => playerObject.position;
    
    /// <summary>
    /// The current euler rotation of the player object.
    /// </summary>
    public Vector3 CurrentObjectEulerRotation => playerObject.rotation.eulerAngles;
    
    /// <summary>
    /// Teleports the player object to the target location. Optionally also rotates to match target Euler rotation.
    /// </summary>
    public void TpPlayerObject(Vector3 targetPosition, Vector3 targetEulerRotation, bool alsoRotate)
    {
        // if the character controller is enabled and active we have to use its move method
        if (characterController.enabled && characterController.gameObject.activeSelf)
        {
            Vector3 currentPosition = playerObject.position;
                    
            Vector3 moveVector = targetPosition - currentPosition;
            
            characterController.Move(moveVector);
        }
        // otherwise just set position of player object GameObject
        else
        {
            playerObject.position = targetPosition;
        }
        
        if (alsoRotate)
        {
            // only rotate on y axis
            targetEulerRotation = new Vector3(0f, targetEulerRotation.y, 0f);
            
            playerObject.rotation = Quaternion.Euler(targetEulerRotation);
        }
    }
    
    
    /// <summary>
    /// Message to be received from players clone of InputActions when move input is performed.
    /// </summary>
    public void OnMove(InputValue value)

    {
        Vector2 inputValue = value.Get<Vector2>();

        
        rawMoveInput = new Vector3(inputValue.x, 0f, inputValue.y);
    }

    /// <summary>
    /// Message to be received from players clone of InputActions when jump button is pressed.
    /// </summary>
    public void OnJump(InputValue buttonValue)
    {
        if (buttonValue.isPressed)
        {
            jumpBufferTimer = jumpBufferDuration;
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

                animator == null)
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
                // get character controller from player object
                characterController = playerObject.GetComponent<CharacterController>();
                
                // get player camera component from Pio
                playerCameraComponent = Pio.GetComponent<PlayerCameraPioComponent>();
                
                if (characterController == null ||
                    playerCameraComponent == null)



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

        ResetDynamicSettings();

        // teleport to spawn point
        TpPlayerObject(targetSpawnPosition, targetSpawnEulerRotation, true);
        
        // turn on player object
        playerObject.gameObject.SetActive(true);
    }

    /// <summary>
    /// Resets dynamic movement settings and deactivates player object.
    /// </summary>
    private void DeSpawn()
    {
        ResetDynamicSettings();
        
        // teleport to spawn point
        TpPlayerObject(targetSpawnPosition, targetSpawnEulerRotation, true);
        
        // turn off player object
        playerObject.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Used to reset all dynamic settings related to movement and state of the player object and go back to idle.
    /// DOES NOT reset spawn position/rotation.
    /// </summary>
    private void ResetDynamicSettings()
    {
        rawMoveInput = Vector3.zero;
            
        orientedMoveInput = Vector3.zero;
            
        targetMove = Vector3.zero;
            
        targetEulerRotation = Vector3.zero;
            
        isGrounded = false;
            
        jumpBufferTimer = 0f;
        
        jumpCoyoteBufferTimer = 0f;

        HandleChangeState(EPlayerObjectState.Idle);
    }
    
    private void FixedUpdate()
    {
        // cannot move if not initialized
        if (!Initialized) { return; }
        
        //move the character controller based on targetMove
        characterController.Move(Time.fixedDeltaTime * targetMove);
    }

    private void Update()
    {
        if (!Initialized) { return; }
        
        ManageCooldownsAndTimers();

        ManageGrounded();

        ManageOrientedInput();

        ManageTargetMove();

        ManageRotation();
        
        ManageState();
        
        return;
        
        void ManageCooldownsAndTimers()
        {
            // counting down jump cooldowns and timers
            if (jumpBufferTimer > 0f)
            {
                jumpBufferTimer -= Time.deltaTime;
            }
            
            if (jumpCoyoteBufferTimer > 0f)
            {
                jumpCoyoteBufferTimer -= Time.deltaTime;
            }
        }

        void ManageGrounded()
        {
            // sample from player controller
            isGrounded = characterController.isGrounded;
            
            // if just left grounded, start coyote timer
            if (!isGrounded && wasGrounded && targetMove.y < 0f)
            {
                jumpCoyoteBufferTimer = jumpCoyoteBufferDuration;
            }
            
            wasGrounded = isGrounded;
        }

        void ManageOrientedInput()
        {
            // oriented input is raw input relative to look orientation
            orientedMoveInput = CurrentLookOrientation.forward * rawMoveInput.z + CurrentLookOrientation.right * rawMoveInput.x;
        
            // don't allow vertical movement from move input
            orientedMoveInput.y = 0f;
        
            // normalize to prevent faster diagonal movement
            orientedMoveInput.Normalize();
        }

        void ManageTargetMove()
        {
            // lateral management
            targetMove.x = orientedMoveInput.x * walkSpeed;
            
            targetMove.z = orientedMoveInput.z * walkSpeed;
            
            // jump can be requested with variable conditions so check that first
            if (JumpRequested)
            {
                // resetting jump buffers (also resets JumpRequested)
                jumpBufferTimer = 0f;
                jumpCoyoteBufferTimer = 0f;
                // use grav to calc jump height
                targetMove.y = Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y); 
            }
            // if grounded logic
            else if (isGrounded)
            {
                // if targetMove isn't upward (just jumped), set to small downward force to keep grounded
                if (targetMove.y <= 0f)
                {
                    targetMove.y = -0.1f; //small downward force to keep grounded
                }
            }
            // if not grounded logic
            else
            {
                targetMove.y += Physics.gravity.y * Time.deltaTime; // add gravity when not grounded
            }
            
            // todo: hitting head logic... annoying, look to past
        }

        void ManageRotation()
        {
            // logic variable based on camera type and if player object should face move direction
            bool shouldPoFaceMoveDirection = 
                (playerCameraComponent.CurrentCameraType == PlayerCameraPioComponent.EPlayerCameraType.PlayerThirdOrbit ||
                 playerCameraComponent.CurrentCameraType == PlayerCameraPioComponent.EPlayerCameraType.MainCamera ||
                playerCameraComponent.CurrentCameraType == PlayerCameraPioComponent.EPlayerCameraType.PlayerFixed);

            // body faces move direction in certain camera types, otherwise faces look orientation.
            if (shouldPoFaceMoveDirection)
            {
                // don't rotate if no move input
                if (orientedMoveInput != Vector3.zero)
                {
                    //target rot is lerped from playerObject rot to oriented move input dir
                    targetEulerRotation = 
                        Quaternion.Lerp(playerObject.rotation, Quaternion.LookRotation(orientedMoveInput), 
                            playerRotationEasing).eulerAngles;
                    
                    
                    playerObject.rotation = Quaternion.Euler(0f, targetEulerRotation.y, 0f);
                }
            }
            else
            {
                // face look orientation dir
                targetEulerRotation = Quaternion.LookRotation(CurrentLookOrientation.forward).eulerAngles;
            
                // only rotate on y axis
                playerObject.rotation = Quaternion.Euler(0f, targetEulerRotation.y, 0f);
            }
        }
        
        void ManageState()
        {
            EPlayerObjectState previousState = currentState;

            EPlayerObjectState targetState;


            
            if (isGrounded)
            {
                // if grounded and trying to move walking
                if (orientedMoveInput.magnitude > 0f)
                {
                    targetState = EPlayerObjectState.Walking;
                }
                // otherwise idle
                else
                {
                    targetState = EPlayerObjectState.Idle;
                }
            }
            else
            {
                // if not grounded and target move is up, jumping
                if (targetMove.y > 0f)
                {
                    targetState = EPlayerObjectState.Jumping;
                }
                // otherwise must be falling
                else
                {
                    targetState = EPlayerObjectState.Falling;
                }
            }

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
            case EPlayerObjectState.Walking:
                // todo: cache animator hashes
                animator.SetBool("isMoving", false);
                break;
            case EPlayerObjectState.Jumping:
                break;
            case EPlayerObjectState.Falling:
                break;
        }
        
        // logic for entering these states
        switch (toState)
        {
            case EPlayerObjectState.Inactive:
                break;
            case EPlayerObjectState.Idle:
                break;
            case EPlayerObjectState.Walking:
                animator.SetBool("isMoving", true);
                break;
            case EPlayerObjectState.Jumping:
                break;
            case EPlayerObjectState.Falling:
                break;
        }
        
        currentState = toState;
    }
}