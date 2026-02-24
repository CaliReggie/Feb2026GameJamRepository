using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

/// <summary>
/// The PlayerCameraPioComponent class is responsible for managing the Pios camera system,
/// using various view types, controls and settings to do so. Leverages CineMachine for camera control.
/// </summary>
public class PlayerCameraPioComponent : PioComponent
{
    /// <summary>
    /// The different types of player cameras available to the player to use as their main view.
    /// </summary>
    public enum EPlayerCameraType
    {
        [InspectorName(null)] // Don't want to show as setting in inspector
        PlayerFixed, // playerObjectCamera + firstPersonVirtual, static position and rotation set by target position
        [Tooltip("Player Object First Person Camera View")]
        PlayerFirstPerson, // playerObjectCamera + firstPersonVirtual, dynamic position and rotation
        [Tooltip("Player Object Third Person Orbit Camera View. Player can rotate/move independent of look direction.")]
        PlayerThirdOrbit, // playerObjectCamera + thirdPersonOrbitVirtual, dynamic position and rotation
        [Tooltip("Player Object Third Person Fixed Camera View. Player faces/moves corresponding to look direction.")]
        PlayerThirdFixed, // playerObjectCamera + thirdPersonFixedVirtual, dynamic position and rotation
        [Tooltip("Player Object Main Camera View. Uses the Main Camera in the scene. " +
                 "Player can rotate/move independent of look direction.")]
        MainCamera // MainCamera in scene, static position and rotation set by target position / MainCamera location
    }
    
    [Header("Inscribed References")]
    
    [Tooltip("The default position target for camera type. Null will lead to no follow.")]
    [SerializeField] private Transform playerObjectCameraPosition;
    
    [Tooltip("The default look orientation for the camera in types directly associated with the player object.")]
    [SerializeField] private Transform playerObjectLookOrientation;
    
    [Tooltip("The real Camera component to be used in camera types directly associated with the player object.")]
    [SerializeField] private Camera playerObjectCamera;
    
    [Tooltip("CineMachine Virtual Camera to use for PlayerFirstPerson type")]
    [SerializeField] private CinemachineCamera firstPersonVirtual;
    
    [Tooltip("CineMachine Virtual Camera to use for PlayerThirdOrbit type")]
    [SerializeField] private CinemachineCamera thirdPersonOrbitVirtual;
    
    [Tooltip("CineMachine Virtual Camera to use for PlayerThirdFixed type")]
    [SerializeField] private CinemachineCamera thirdPersonFixedVirtual;
    
    [Header("Inscribed Settings")]
    
    [Tooltip("Vertical rotation range clamp for the camera orientation")]
    [SerializeField] private Vector2 verticalRotRange = new (-89, 89);
    
    [Tooltip("Layers rendered in First Person view")]
    [SerializeField] private LayerMask firstPersonCullingMask = -1;
    
    [Tooltip("Layers rendered in Third Person views")]
    [SerializeField] private LayerMask thirdPersonCullingMask = -1;
    
    [Header("Dynamic References - Don't Modify In Inspector")]
    
    [Tooltip("The PlayerInput component attached to Pio.")]
    [SerializeField] private PlayerInput playerInputComponent;
    
    [Tooltip("The CineMachineBrain component attached to playerObjectCamera.")]
    [SerializeField] private CinemachineBrain brainComponent;
    
    [Tooltip("The CineMachineOrbitalFollow component attached to thirdPersonOrbitVirtual.")]
    [SerializeField] private CinemachineOrbitalFollow thirdOrbitalComponent;
    
    [Tooltip("The PlayerObjectPioComponent attached to Pio.")]
    [SerializeField] private PlayerObjectPioComponent playerObjectComponent;
    
    [Tooltip("Current Virtual Camera if the current type supports it, null otherwise.")]
    [SerializeField] private CinemachineCamera currentVirtualCam;
    
    [Tooltip("Current position target for the camera type. If null will not move/follow.")]
    [SerializeField] private Transform currentTargetPosition;
    
    [Tooltip("The MainCamera script instance in the scene.")]
    [SerializeField] private MainCamera currentMainCameraScript;
    
    [Header("Dynamic Settings - Don't Modify In Inspector")]
    
    [Tooltip("Current look input from player.")]
    [SerializeField] private Vector2 currentLookInput;
    
    [Tooltip("Current target rotation for the camera type in EulerAngles.")]
    [SerializeField] private Vector3 targetEulerRotation;
    
    [Tooltip("The current camera type being used.")]
    [field: SerializeField] public EPlayerCameraType CurrentCameraType  { get; private set; }
    
    [Tooltip("The sensitivity multiplier for camera look input")]
    [Range(0.01f, 1)] [SerializeField] private float currentCameraSensitivity = 1f;

    /// <summary>
    /// The current look orientation (transform) based on the current camera type. Can be used to know
    /// exactly what orientation the player is seeing at.
    /// </summary>
    public Transform CurrentLookOrientation => CurrentCameraType == EPlayerCameraType.MainCamera ? 
        currentMainCameraScript.Camera.transform : playerObjectLookOrientation;
    
    /// <summary>
    /// Ensures certain settings are correct
    /// </summary>
    private void OnValidate()
    {
        // ensure vertical rot range is valid (cannot be more than 89 degrees up or down)
        if (verticalRotRange.x < -89)
        {
            verticalRotRange.x = -89;
        }
        
        if (verticalRotRange.y > 89)
        {
            verticalRotRange.y = 89;
        }
    }
    
    /// <summary>
    /// Received by message sent from PlayerInput component on Pio when Look input action is performed.
    /// </summary>
    public void OnLook(InputValue value)
    {
        // get and store input vector
        Vector2 inputVector = value.Get<Vector2>();
        
        currentLookInput = inputVector;
    }
    
    protected override void Initialize()
    {
        if (!CheckInscribedReferences())
        {
            return;
        }
        
        if (!SetDynamicReferences())
        {
            return;
        }
        
        InitCameras();
        
        Initialized = true;
        
        return;
        
        // Checks inscribed refs and logs error if any are missing
        bool CheckInscribedReferences()
        {
            if (playerObjectLookOrientation == null ||
                playerObjectCamera == null ||
                firstPersonVirtual == null ||
                thirdPersonOrbitVirtual == null ||
                thirdPersonFixedVirtual == null)
            {
                Debug.LogError($"{ GetType().Name}:" +
                               $" One or more inscribed references are not set in the Inspector on {gameObject.name}.");
                return false;
            }
            
            return true;
        }
        
        // Sets dynamic refs and logs error if any are missing
        bool SetDynamicReferences()
        {
            try
            {
                // get third person orbital component from virtual camera
                thirdOrbitalComponent = thirdPersonOrbitVirtual.GetComponent<CinemachineOrbitalFollow>();
                
                // get PlayerInput component from Pio
                playerInputComponent = Pio.GetComponent<PlayerInput>();
                
                // get CineMachineBrain component from playerObjectCamera
                brainComponent = playerObjectCamera.GetComponent<CinemachineBrain>();
                
                // get PlayerObjectPioComponent from Pio
                playerObjectComponent = Pio.GetComponent<PlayerObjectPioComponent>();
                
                // get MainCamera instance in scene
                currentMainCameraScript = MainCamera.Instance;
                
                if (thirdOrbitalComponent == null ||
                    playerInputComponent == null ||
                    brainComponent == null ||
                    playerObjectComponent == null ||
                    currentMainCameraScript == null)
                {
                    Debug.LogError($"{ GetType().Name}: Error setting dynamic references.");
                    
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"{ GetType().Name}: Error setting dynamic references: {e.Message}");
                
                return false;
            }
            
            return true;
        }
        
        // initializes all default camera position, tracking, and channel settings
        void InitCameras()
        { 
            // set default target position
            currentTargetPosition = playerObjectCameraPosition;
            
            // ensure playerObjectCamera starts off disabled
            playerObjectCamera.enabled = false;
            playerObjectCamera.gameObject.SetActive(false);
            
            // look at is how CineMachine cameras rotate/orient themselves
            firstPersonVirtual.LookAt = playerObjectLookOrientation;
            // ensure start off
            firstPersonVirtual.enabled = false;
            firstPersonVirtual.gameObject.SetActive(false);
        
            thirdPersonOrbitVirtual.LookAt = playerObjectLookOrientation;
            thirdPersonOrbitVirtual.enabled = false;
            thirdPersonOrbitVirtual.gameObject.SetActive(false);
            
            thirdPersonFixedVirtual.LookAt = playerObjectLookOrientation;
            thirdPersonFixedVirtual.enabled = false;
            thirdPersonFixedVirtual.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Takes a CineMachine Virtual Camera and pairs its output channel with the attached CineMachineBrain
    /// </summary>
    private void PairCameraChannels(CinemachineCamera virtualCam)
    {
        //cannot pair if not initialized
        if (!Initialized) return;
        
        //cannot pair null camera
        if (virtualCam == null) return;
        
        // set brain channel mask based on Pio visual index
        brainComponent.ChannelMask = (OutputChannels)(1 << Pio.VisualIndex); // 1,2,4,8 for channels 0-3
        
        // set virtual cam output channel to match brain
        virtualCam.OutputChannel = brainComponent.ChannelMask;
    }
    
    /// <summary>
    /// Takes a CineMachine Virtual Camera and unpairs its output channel from the attached CineMachineBrain
    /// </summary>
    private void UnpairCameraChannels(CinemachineCamera virtualCam)
    {
        //cannot unpair if not initialized
        if (!Initialized) return;
        
        //cannot unpair null camera
        if (virtualCam == null) return;

        // reset brain channel mask to default
        virtualCam.OutputChannel = OutputChannels.Default;
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
                    // deactivate Main Player Camera if it exists
                    if (playerObjectCamera != null)
                    {
                        playerObjectCamera.enabled = false;
                        playerObjectCamera.gameObject.SetActive(false);
                    }
        
                    // deactivate the current virtual camera if it exists
                    if (currentVirtualCam != null)
                    {
                        currentVirtualCam.enabled = false;
                        currentVirtualCam.gameObject.SetActive(false);
                        UnpairCameraChannels(currentVirtualCam);
                        currentVirtualCam = null;
                    }
                    
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

                // if off, activate from defaults
                if (!enabled)
                {
                    // logic varies if player using MainCamera type
                    bool targetTypeIsMainCamera = Pio.CurrentPlayerSettings.CameraType == EPlayerCameraType.MainCamera;
                
                    if (targetTypeIsMainCamera)
                    {
                        // on switches to main camera, update and validate reference to currentMainCameraScript
                        currentMainCameraScript = MainCamera.Instance;
                        
                        // cannot switch if no MainCamera in scene
                        if (currentMainCameraScript == null)
                        {
                            Debug.LogError($"{ GetType().Name}: Cannot switch to Main Camera because" +
                                           $" it does not exist in the scene.");
                            
                            return;
                        }
                        
                        // configure to use MainCamera type and its transform as target position
                        ConfigureCamera(EPlayerCameraType.MainCamera, currentMainCameraScript.Camera.transform);
                        
                        // todo: in future, could set logic to move MainCamera
                        //  to a target position instead of using its current position
                    }
                    else
                    {
                        // configure to use player object camera type and its default target position
                        ConfigureCamera(Pio.CurrentPlayerSettings.CameraType, playerObjectCameraPosition);
                    }
                    
                    // set initial target rotation based on current player object euler rotation
                    targetEulerRotation = new Vector3( 0, playerObjectComponent.ObjectEulerRotation.y, 0);
                    
                    enabled = true;
                }
                
                break;
        }
    }

    protected override void OnAfterPioSettingsChange(PlayerSettingsSO playerSettings)
    {
        try
        {
            // if enabled, configure active camera settings
            if (enabled)
            {
                // logic varies if player using MainCamera type
                bool targetTypeIsMainCamera = playerSettings.CameraType == EPlayerCameraType.MainCamera;
                
                if (targetTypeIsMainCamera)
                {
                    // on switches to main camera, update and validate reference to currentMainCameraScript
                    currentMainCameraScript = MainCamera.Instance;
                    
                    // cannot switch if no MainCamera in scene
                    if (currentMainCameraScript == null)
                    {
                        Debug.LogError($"{ GetType().Name}: Cannot switch to Main Camera because" +
                                       $" it does not exist in the scene.");
                        
                        return;
                    }
                    
                    // configure to use MainCamera type and its transform as target position
                    ConfigureCamera(EPlayerCameraType.MainCamera, currentMainCameraScript.Camera.transform);
                    
                    // todo: in future, could set logic to move MainCamera
                    //  to a target position instead of using its current position
                }
                else
                {
                    // configure to use player object camera type and its default target position
                    ConfigureCamera(playerSettings.CameraType, playerObjectCameraPosition);
                }
            }
            
            // apply sensitivity setting
            currentCameraSensitivity = playerSettings.CameraSensitivity;
        }
        catch (Exception e)
        {
            Debug.LogError($"{ GetType().Name}:{name}: Error applying new player settings: {e.Message}");
        }
    }
    
    /// <summary>
    /// Configures player camera with specified type and position target. Must be initialized.
    /// </summary>
    private void ConfigureCamera(EPlayerCameraType targetType, Transform targetPos)
    {
        // cannot configure if not initialized
        if (!Initialized)
        {
            Debug.LogError($"{ GetType().Name}: Cannot configure camera before initialization.");
            
            return;
        }
        
        //set the position target. null (no follow) by default is allowed.
        currentTargetPosition = targetPos;
        
        // Set the player cam type to the target type.
        CurrentCameraType = targetType;
        
        // deactivate the current virtual camera if it exists
        if (currentVirtualCam != null)
        {
            currentVirtualCam.enabled = false;
            currentVirtualCam.gameObject.SetActive(false);
            UnpairCameraChannels(currentVirtualCam);
            currentVirtualCam = null;
        }
        
        // logic based on camera type
        switch (CurrentCameraType)
        {
            // for fixed, set position and rotation directly and that's it
            case EPlayerCameraType.PlayerFixed:
                
                //set cam position and rotation if target assigned
                if (currentTargetPosition != null)
                {
                    playerObjectCamera.transform.
                        SetPositionAndRotation(currentTargetPosition.position, currentTargetPosition.rotation);
                }
                
                break;
            // for player camera types, assign the correct virtual cam and culling mask
            case EPlayerCameraType.PlayerFirstPerson:
                
                currentVirtualCam = firstPersonVirtual;
                
                playerObjectCamera.cullingMask = firstPersonCullingMask;
                
                break;
            case EPlayerCameraType.PlayerThirdOrbit:
                
                currentVirtualCam = thirdPersonOrbitVirtual;
                
                playerObjectCamera.cullingMask = thirdPersonCullingMask;
                
                break;
            case EPlayerCameraType.PlayerThirdFixed:
                
                currentVirtualCam = thirdPersonFixedVirtual;
                
                playerObjectCamera.cullingMask = thirdPersonCullingMask;
                
                break;
            // for main camera types, set position and rotation of main camera to target position if assigned
            case EPlayerCameraType.MainCamera:
                
                if (currentTargetPosition != null)
                {
                    currentMainCameraScript.Camera.transform.
                        SetPositionAndRotation(currentTargetPosition.position, currentTargetPosition.rotation);
                }
                
                break;
                
        }

        // If we have a valid virtualCam, configure it
        if (currentVirtualCam != null)
        { 
            PairCameraChannels(currentVirtualCam);
            
            //turn on virtual cam
            currentVirtualCam.enabled = true;
            currentVirtualCam.gameObject.SetActive(true);
        }
        
        // ensure MainPlayerCam is active if not already
        if (!playerObjectCamera.enabled)
        {
            playerObjectCamera.enabled = true;
            playerObjectCamera.gameObject.SetActive(true);
        }
        
        // logic to enable/disable MainPlayerCam varies if MainCamera type or not
        bool isMainCameraType = CurrentCameraType == EPlayerCameraType.MainCamera;
        
        // if not MainCamera, ensure MainPlayerCam is active if not already
        if (!isMainCameraType)
        {
            playerObjectCamera.enabled = true;
            playerObjectCamera.gameObject.SetActive(true);
        }
        // if is MainCamera, ensure MainPlayerCam is off
        else
        {
            playerObjectCamera.enabled = false;
            playerObjectCamera.gameObject.SetActive(false);
        }
    }
    private void Update()
    {
        // skip update if not initialized or using non-controllable camera type
        bool isNonControllableCameraType = CurrentCameraType == EPlayerCameraType.PlayerFixed || 
                                       CurrentCameraType == EPlayerCameraType.MainCamera;
        
        if (!Initialized || isNonControllableCameraType) {return;}
        
        UpdateOrientationPosition();
        
        UpdateOrientationRotation();
        
        return; 
        
        // following position target if assigned
        void UpdateOrientationPosition()
        {
            if (currentTargetPosition != null)
            {
                playerObjectLookOrientation.position = currentTargetPosition.transform.position;
            }
        }
        
        // updating orientation rotation based on look input
        void UpdateOrientationRotation()
        {
            targetEulerRotation.y += currentLookInput.x * currentCameraSensitivity * Time.deltaTime;
            
            targetEulerRotation.x -= currentLookInput.y * currentCameraSensitivity * Time.deltaTime;
            
            targetEulerRotation.x = Mathf.Clamp(targetEulerRotation.x, verticalRotRange.x, verticalRotRange.y);
            
            targetEulerRotation.y = Mathf.Repeat(targetEulerRotation.y, 360);
            
            playerObjectLookOrientation.rotation = Quaternion.Euler(targetEulerRotation.x, targetEulerRotation.y, 0);
            
            // drive third person orbital component based on orientation if active
            bool thirdOrbitActive = CurrentCameraType == EPlayerCameraType.PlayerThirdOrbit &&
                                    thirdOrbitalComponent != null;
        
            if (thirdOrbitActive)
            {
                thirdOrbitalComponent.HorizontalAxis.Value = targetEulerRotation.y;
                
                thirdOrbitalComponent.VerticalAxis.Value = targetEulerRotation.x;
            }
        }
    }
}
