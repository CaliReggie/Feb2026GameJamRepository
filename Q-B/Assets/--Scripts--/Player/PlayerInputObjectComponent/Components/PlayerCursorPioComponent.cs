using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// The PlayerCursorPioComponent class extends PioComponent to manage a player-specific cursor GameObject
/// for UI interaction. Can handle both Keyboard&Mouse and Gamepad control schemes, but does not currently
/// support swapping between schemes at runtime or other scheme types.
/// </summary>
public class PlayerCursorPioComponent : PioComponent
{
    /// <summary>
    /// The string name of the Gamepad control scheme
    /// </summary>
    private const string GamepadScheme = "Gamepad";
    
    /// <summary>
    /// The string name of the Keyboard&Mouse control scheme
    /// </summary>
    private const string KeyboardScheme = "Keyboard&Mouse";
    
    [Header("Inscribed References")]
    
    [Tooltip("The prefab to spawn as player cursor")]
    [SerializeField] private GameObject cursorPrefab;
    
    [FormerlySerializedAs("forceRealMouseInBounds")]
    [Header("Inscribed Settings")]
    
    [Tooltip("If true, the hardware mouse will be forced to stay within bounds when cursor is active. " +
             "If false, the hardware mouse can move freely (meaning freely click elsewhere / off bounds), " +
             "but the cursor will still be clamped to player bounds.")]
    [SerializeField] private bool constrainRealMouseInBounds = true;
    
    [Header("Dynamic References - Don't Modify In Inspector")]
    
    [Tooltip("The PlayerInput component of the Pio")]
    [SerializeField] private PlayerInput pioPlayerInput;
    
    [Tooltip("The PlayerUiPioComponent of the Pio")]
    [SerializeField] private PlayerUiPioComponent playerUiPioComponent;
    
    [SerializeField] private PlayerObjectPioComponent playerObjectPioComponent;
    
    [Tooltip("The canvas the cursor will be used on for player ui")]
    [SerializeField] private Canvas playerUiCanvas;
    
    [Tooltip(" The rect transform of the canvas the cursor will be used on for player ui")]
    [SerializeField] private RectTransform playerUiCanvasRectTransform;
    
    [Tooltip("The camera used to draw cursor on the player ui")]
    [SerializeField] private Camera playerUiDrawCam;
    
    [Tooltip("The canvas the cursor will be used on for scene ui")]
    [SerializeField] private Canvas sceneUiCanvas;
    
    [Tooltip(" The rect transform of the canvas the cursor will be used on for scene ui")]
    [SerializeField] private RectTransform sceneUiCanvasRectTransform;
    
    /// <summary>
    /// Returns the current canvas the cursor should be used on based on Pio state. Null if in a state
    /// not set up for cursor use.
    /// </summary>
    private Canvas CurrentCursorCanvas
    {
        get
        {
            // cannot get current canvas if no current state
            if (Pio.CurrentState == null) return null;
            
            // if in player ui state, logic varies
            if (Pio.CurrentState.State == PlayerInputObject.EPlayerInputObjectState.PlayerUi)
            {
                // if player using main camera, use scene ui canvas
                if (Pio.CurrentPlayerSettings.CameraType == PlayerCameraPioComponent.EPlayerCameraType.MainCamera)
                {
                    return sceneUiCanvas;
                }
                // otherwise use player ui canvas
                else
                {
                    return playerUiCanvas;
                }
                
            }
            // if in scene ui or player scene ui state, use scene ui canvas
            else if (Pio.CurrentState.State == PlayerInputObject.EPlayerInputObjectState.SceneUi ||
                     Pio.CurrentState.State == PlayerInputObject.EPlayerInputObjectState.PlayerSceneUi)
            {
                return sceneUiCanvas;
            }
            // otherwise currently no cursor logic setup in another state
            else
            {
                return null;
            }
        }
    }
    
    /// <summary>
    /// Returns the current canvas rect transform the cursor should be used on based on Pio state. Null if in a state
    /// not set up for cursor use.
    /// </summary>
    private RectTransform CurrentCursorCanvasRectTransform
    {
        get
        {
            // cannot get current canvas if no current state
            if (Pio.CurrentState == null) return null;
            
            // if in player ui state, logic varies
            if (Pio.CurrentState.State == PlayerInputObject.EPlayerInputObjectState.PlayerUi)
            {
                // if player using main camera, use scene ui canvas rect transform
                if (Pio.CurrentPlayerSettings.CameraType == PlayerCameraPioComponent.EPlayerCameraType.MainCamera)
                {
                    return sceneUiCanvasRectTransform;
                }
                // otherwise use player ui canvas rect transform
                else
                {
                    return playerUiCanvasRectTransform;
                }
            }
            // if in scene ui or player scene ui state, use scene ui canvas rect transform
            else if (Pio.CurrentState.State == PlayerInputObject.EPlayerInputObjectState.SceneUi ||
                     Pio.CurrentState.State == PlayerInputObject.EPlayerInputObjectState.PlayerSceneUi)
            {
                return sceneUiCanvasRectTransform;
            }
            // otherwise currently no cursor logic setup in another state
            else
            {
                return null;
            }
        }
    }
    
    [Tooltip("The currently managed RectTransform references associated with the cursor GameObject")]
    [field: SerializeField] public RectTransform CursorInstance {get; private set;}

    [Tooltip("The mouse being managed. Either the real mouse of k&m player or virtual mouse of gamepad player")]
    private Mouse _mouse;

    [Header("Dynamic Settings - Don't Modify In Inspector")]
    
    [Tooltip("The players specific min and max Vector2 screen bounds")]
    [SerializeField]
    private Vector2[] playerScreenBounds;
    
    [Tooltip("The main canvas min and max Vector2 screen bounds")]
    [SerializeField]
    private Vector2[] mainScreenBounds;
    
    [Tooltip("If true, cursor movement is constrained to player screen bounds")]
    [SerializeField] private bool cursorConstrained;
    
    [Tooltip("The most recently updated position of real mouse scheme")]
    [SerializeField]
    private Vector2 realMousePos;
    
    [Tooltip("The most recently updated direction of gamepad movement")]
    [SerializeField]
    private Vector2 gamepadDir;
    
    [Tooltip("The most recently updated bool if either control scheme click is pressed")]
    [SerializeField]
    private bool cursorPressed;
    
    /// <summary>
    /// Way to set the cursor sprite from other scripts if needed,
    /// passing in null will set it to the current player settings sprite.
    /// </summary>
    public void SetOverrideCursor(Sprite newCursorSprite)
    {
        try
        {
            // try to get cursor image and assign new sprite
            Image cursorImage = CursorInstance.GetComponent<Image>();
                
            if (newCursorSprite == null)
            {
                // assign player settings sprite if null
                cursorImage.sprite = Pio.CurrentPlayerSettings.CursorSprite;
            }
            else
            {
                cursorImage.sprite = newCursorSprite;
            }
        }
        catch (Exception)
        {
            return;
        }
    }
    
    /// <summary>
    /// Message to be received from PlayerInput Message Broadcast when real mouse moves on Keyboard&Mouse scheme.
    /// </summary>
    public void OnPoint(InputValue pointValue)
    {
        realMousePos = pointValue.Get<Vector2>();
    }
    
    /// <summary>
    /// Message to be received from PlayerInput Message Broadcast for moving cursor on Gamepad scheme.
    /// </summary>
    public void OnGamepadPoint(InputValue inputVector)
    {
        gamepadDir = inputVector.Get<Vector2>();
    }
    
    /// <summary>
    /// Message to be received from PlayerInput Message Broadcast for clicking on any scheme.
    /// </summary>
    public void OnClick(InputValue buttonValue)
    {
        cursorPressed = buttonValue.isPressed;
        
        //hide real mouse when using cursor (if constraining hardware mouse in bounds)
        if (cursorPressed && enabled && Initialized && constrainRealMouseInBounds)
        {
            Cursor.visible = false;
        }
    }
    
    /// <summary>
    /// Message to be received from InputManager Message Broadcast when a player joins.
    /// </summary>
    public void OnPlayerJoined(PlayerInput joinedPlayerInput)
    {
        // player bounds will likely change, update
        UpdateScreenBounds();
    }
    
    /// <summary>
    /// Message to be received from InputManager Message Broadcast when a player leaves.
    /// </summary>
    public void OnPlayerLeft(PlayerInput joinedPlayerInput)
    {
        // player bounds will likely change, update
        UpdateScreenBounds();
    }
    
    protected override void Initialize()
    {
        // make sure inscribed refs are good
        if (!CheckInscribedReferences()) return;
        
        // ensure dynamic refs set
        if (!SetDynamicReferences()) return;
        
        // get mouse ref or add if gamepad
        ConfigureMouseReference();
        
        // ensure starts off
        ToggleCursorInstance(false);
        
        // configure cursor defaults
        ConfigureCursorSettings(CursorLockMode.Locked, false);
        
        Initialized = true;
        
        // update screen bounds
        UpdateScreenBounds();
        
        return;
        
        // checks if all inscribed references are set
        bool CheckInscribedReferences()
        {
            if (cursorPrefab == null)
            {
                Debug.LogError($"{GetType().Name}: Inscribed references not set.");
                
                return false;
            }
            else
            {
                return true;
            }
        }
        
        // sets and checks dynamic references
        bool SetDynamicReferences()
        {
            try
            {
                // getting PlayerInput ref from Pio
                pioPlayerInput = GetComponent<PlayerInput>();
                
                playerObjectPioComponent = GetComponent<PlayerObjectPioComponent>();
                
                // getting PlayerUiPioComponent ref from Pio
                playerUiPioComponent = GetComponent<PlayerUiPioComponent>();
                
                // getting player ui canvas ref
                playerUiCanvas = playerUiPioComponent.PlayerObjectUiCanvas;
                
                // getting player ui canvas rect transform ref
                playerUiCanvasRectTransform = playerUiPioComponent.PlayerObjectUiCanvas.GetComponent<RectTransform>();
                
                // getting player ui draw camera ref
                playerUiDrawCam = playerUiPioComponent.PlayerObjectUiCanvas.worldCamera;
                
                // getting scene ui canvas ref
                sceneUiCanvas = playerUiPioComponent.SceneUiCanvas;
                
                // getting scene ui canvas rect transform ref
                sceneUiCanvasRectTransform = sceneUiCanvas.GetComponent<RectTransform>();

                // getting cursor constrained setting from player settings
                cursorConstrained = Pio.CurrentPlayerSettings.CurrentConfiguration.CursorConstrained;
                
                // spawning cursor from prefab if null, starting with gameObject inactive.
                if (CursorInstance == null)
                {
                    GameObject cursorGO = Instantiate(cursorPrefab, transform);
                    
                    //assigning to CursorInstance
                    CursorInstance = cursorGO.GetComponent<RectTransform>();
                    
                    //configuring name
                    CursorInstance.name = "Player" + Pio.VisualIndex + "Cursor";
                    
                    //getting cursor image and assigning sprite from player settings if exists
                    Image cursorImage = CursorInstance.GetComponent<Image>();

                    if (Pio.CurrentPlayerSettings != null &&
                        Pio.CurrentPlayerSettings.CursorSprite != null)
                    {
                        cursorImage.sprite = Pio.CurrentPlayerSettings.CursorSprite;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("PlayerCursor: Exception getting dynamic references: " + e);

                return false;
            }
        }
        
        // getting mouse ref or adding if gamepad
        void ConfigureMouseReference()
        {
            //if player is on keyboard
            if (pioPlayerInput.currentControlScheme == KeyboardScheme)
            {
                // assigning real mouse if not already present
                if (_mouse == null)
                {
                    // find the mouse among player input devices
                    foreach (var device in pioPlayerInput.devices)
                    {
                        if (device is Mouse)
                        {
                            // found it, assign and break
                            _mouse = device as Mouse;
                            
                            break;
                        }
                    }
                }
                // adding device if not already added
                else if (!_mouse.added)
                {
                    InputSystem.AddDevice(_mouse);
                }
                
            }
            //if player is on gamepad
            else if (pioPlayerInput.currentControlScheme == GamepadScheme)
            {
                //adding virtual mouse if not already present
                if (_mouse == null)
                {
                    _mouse = (Mouse) InputSystem.AddDevice("VirtualMouse");
                }
                // adding device if not already added
                else if (!_mouse.added)
                {
                    InputSystem.AddDevice(_mouse);
                }
                
                //pairing virtual mouse to player
                InputUser.PerformPairingWithDevice(_mouse, pioPlayerInput.user);
            }
            // unhandled control scheme
            else
            {
                Debug.LogError("PlayerCursor: Unhandled control scheme: " + pioPlayerInput.currentControlScheme);
            }
        }
    }
    
    protected override void OnDestroy()
    {
        // unsub to input system update
        InputSystem.onAfterUpdate -= UpdateCursor;

        // if gamepad player remove virtual mouse
        if (pioPlayerInput.currentControlScheme == GamepadScheme)
        {
            InputSystem.RemoveDevice(_mouse);
        }
        
        //if cursor instance exists, destroy it
        if (CursorInstance != null)
        {
            Destroy(CursorInstance.gameObject);
        }
        
        base.OnDestroy();
    }
    
    protected override void OnBeforePioStateChange(PlayerInputObject.EPlayerInputObjectState toState)
    {
        switch (toState)
        {   
            case PlayerInputObject.EPlayerInputObjectState.Off:
            case PlayerInputObject.EPlayerInputObjectState.Player:
                
                // ensure locked cursor if not in Ui state
                ConfigureCursorSettings(CursorLockMode.Locked, false);
                    
                // re-parent to this transform
                CursorInstance.SetParent(transform, false);
            
                //turn off cursor Go
                if (CursorInstance != null)
                {
                    ToggleCursorInstance(false);
                }
                
                //disable hardware cursor control
                InputSystem.onAfterUpdate -= UpdateCursor;
                
                break;
        }
    }
    
    protected override void OnAfterPioStateChange(PlayerInputObject.EPlayerInputObjectState toState)
    {
        switch (toState)
        {   
            case PlayerInputObject.EPlayerInputObjectState.PlayerUi:
            case PlayerInputObject.EPlayerInputObjectState.PlayerSceneUi:
            case PlayerInputObject.EPlayerInputObjectState.SceneUi:
                
                // ensure confined (semi unlocked) cursor in Ui states
                ConfigureCursorSettings(CursorLockMode.Confined, false);
                
                // re-parent to current canvas
                CursorInstance.SetParent(CurrentCursorCanvas.transform, false);
                
                // ensure on top of canvas
                CursorInstance.SetAsLastSibling();
                
                // ensuring click is false on enable
                cursorPressed = false;
                
                //turn on cursor Go
                ToggleCursorInstance(true);
                
                MoveCursorToCenter();
                
                //enable hardware cursor control
                InputSystem.onAfterUpdate += UpdateCursor;
                
                break;
        }
        
        // enable component if not already
        if (!enabled) { enabled = true;}
    }
    
    protected override void OnAfterPioSettingsChange(PlayerSettingsSO playerSettings)
    {
        try
        {
            // try to get cursor image and assign new sprite from player settings
            Image cursorImage = CursorInstance.GetComponent<Image>();
                
            cursorImage.sprite = playerSettings.CursorSprite;
            
            cursorConstrained = playerSettings.CurrentConfiguration.CursorConstrained;
        }
        catch (Exception e)
        {
            Debug.LogError($"{GetType().Name}:{name}: " +
                           $"Exception updating cursor sprite on player settings change: " + e);
        }
    }
    
    /// <summary>
    /// Toggles the visible cursor gameObject on or off
    /// </summary>
    private void ToggleCursorInstance(bool active)
    {
        if (CursorInstance == null)
        {
            Debug.LogError("PlayerCursor: Cursor Instance null on toggle.");
            
            return;
        }
        
        CursorInstance.gameObject.SetActive(active);
    }
    
    /// <summary>
    /// Configures the cursor settings
    /// </summary>
    /// <param name="hardwareLockMode"></param> The real mouse lock mode to set
    /// <param name="hardwareCursorVisible"></param> If true, real mouse cursor is visible
    private void ConfigureCursorSettings(CursorLockMode hardwareLockMode, bool hardwareCursorVisible)
    {
        // If managed by player manager, will be handled there
        if (Pio.IsPlayerManager) {return; }
        
        // if not constraining, don't change hardware cursor settings since player can
        // move freely and click off bounds if they want, otherwise apply settings
        if (!constrainRealMouseInBounds) { return; }
        
        Cursor.visible = hardwareCursorVisible;
        
        Cursor.lockState = hardwareLockMode;
    }
    
    /// <summary>
    /// Call to update the screen bounds for this player initially or after their camera space changes. 
    /// </summary>
    private void UpdateScreenBounds()
    {
        // cannot update if not initialized
        if (!Initialized)
        {
            return;
        }
        
        // sampling main bounds for whole cursor space
        Rect mainAreaRect;
        
        // use CurrentCursorCanvasRectTransform to sample if exists
        if (CurrentCursorCanvasRectTransform != null)
        {
            mainAreaRect = CurrentCursorCanvasRectTransform.rect;
        }
        // if not use whole display
        else
        {
            float width = Display.main.renderingWidth;
            
            float height = Display.main.renderingHeight;
            
            mainAreaRect = new Rect(0, 0, width, height);
        }
        
        //setting main bounds
        mainScreenBounds = new Vector2[2];

        mainScreenBounds[0] = Vector2.zero;

        mainScreenBounds[1] = new Vector2(mainAreaRect.width, mainAreaRect.height);
        
        //setting player bounds
        playerScreenBounds = new Vector2[2];
        
        // if player ui draw camera exists, sample its rect
        if (playerUiDrawCam != null)
        {
            Rect camRect = playerUiDrawCam.rect;
            
            playerScreenBounds[0] = new Vector2(camRect.xMin * mainAreaRect.width,
                camRect.yMin * mainAreaRect.height); // minimum position
            
            playerScreenBounds[1] = new Vector2(camRect.xMax * mainAreaRect.width,
                camRect.yMax * mainAreaRect.height); // maximum position
        }
        // if not, use main bounds for player bounds
        else
        {
            playerScreenBounds[0] = mainScreenBounds[0]; // min pos

            playerScreenBounds[1] = mainScreenBounds[1]; // max pos
        }
    }
    
    /// <summary>
    /// Takes a V2 position, min, and max and clamps within bounds
    /// </summary>
    private Vector2 ClampedByBounds(Vector2 pos, Vector2 min, Vector2 max)
    {
        // extended logic, using the clamped world position of player object if in relevant state
        Vector2 loc = new Vector2(Mathf.Clamp(pos.x, min.x, max.x), Mathf.Clamp(pos.y, min.y, max.y));
        
        return loc;
    }
    
    /// <summary>
    /// Takes a target pos and moves the cursor GO anchor to that place on current canvas
    /// </summary>
    private void AnchorCursor(Vector2 newPos)
    {
        Vector2 anchoredPos;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(CurrentCursorCanvasRectTransform, newPos, 
            CurrentCursorCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : playerUiDrawCam , out anchoredPos);
        
        CursorInstance.anchoredPosition = anchoredPos;
    }
    
    /// <summary>
    /// Moves the cursor to the center of the player screen bounds and matches the real
    /// or virtual mouse to that position.
    /// </summary>
    private void MoveCursorToCenter()
    {
        //placing in center of screen
        Vector2 startingPos = (playerScreenBounds[0] + playerScreenBounds[1]) / 2;
        
        if (CursorInstance.gameObject.activeInHierarchy)
        {
            AnchorCursor(startingPos);
        }
        
        // if k&m
        if (pioPlayerInput.currentControlScheme == KeyboardScheme)
        {
            //matching mouse to cursor
            _mouse.WarpCursorPosition(startingPos);

            realMousePos = startingPos;
        }
        // if Gp
        else if (pioPlayerInput.currentControlScheme == GamepadScheme)
        {
            //matching virtual mouse to cursor
            InputState.Change(_mouse.position, startingPos);

            gamepadDir = Vector2.zero;
        }
    }
    
    /// <summary>
    /// Subscribed with input system update, cursor will be moved and managed depending on control scheme type
    /// </summary>
    private void UpdateCursor()
    {
        // cannot update if not initialized or not enabled
        if (!Initialized || !enabled)
        {
            return;
        }
        
        //if player in on keyboard
        if (pioPlayerInput.currentControlScheme == KeyboardScheme)
        {
            //clamping mouse pos based on desired player cursor space (player portion or whole screen)
            Vector2 clampedMousePos;
            
            if (cursorConstrained)
            {
                clampedMousePos = ClampedByBounds(realMousePos, playerScreenBounds[0], playerScreenBounds[1]);
            }
            else
            {
                clampedMousePos = ClampedByBounds(realMousePos, mainScreenBounds[0], mainScreenBounds[1]);
            }
            
            //updating cursor position
            AnchorCursor(clampedMousePos);
            
            if (constrainRealMouseInBounds)
            {
                // did real mouse go too far?
                bool mouseOffBounds = realMousePos != clampedMousePos;
                
                if (mouseOffBounds)
                {
                    // how far is it off
                    Vector2 diff = clampedMousePos - realMousePos;
                    
                    // bring it back that much plus a little extra to avoid getting stuck
                    Vector2 warpedCursorPos = clampedMousePos + diff.normalized * 10;
                    
                    //matching mouse to cursor
                    _mouse.WarpCursorPosition(warpedCursorPos);
                    
                    realMousePos = warpedCursorPos;
                }
            }
        }
        //if player is on gamepad
        else if (pioPlayerInput.currentControlScheme == GamepadScheme)
        {
            // cannot proceed if mouse is null
            if (_mouse == null)
            {
                Debug.LogError("PlayerCursor: Mouse is null while on GamepadScheme. Disabling PlayerCursor.");
                
                enabled = false;
                
                return;
            }
            
            //reading current virtual mouse position
            Vector2 gamepadMousePos = _mouse.position.ReadValue();
            
            //calculating new position based on current position and input direction
            Vector2 targetGpMousePos = gamepadMousePos + gamepadDir * Time.unscaledDeltaTime;
            
            //clamping new pos based on desired player cursor space (player portion or whole screen)
            Vector2 clampedTargetGpPos;
            
            if (cursorConstrained)
            {
                clampedTargetGpPos = ClampedByBounds(targetGpMousePos, playerScreenBounds[0], playerScreenBounds[1]);
            }
            else
            {
                clampedTargetGpPos = ClampedByBounds(targetGpMousePos, mainScreenBounds[0], mainScreenBounds[1]);
            }
            //
            // if (GameManager.Instance != null && MainCamera.Instance != null)
            // {
            //     if (GameManager.Instance.CurrentState.State == GameManager.EGameState.Playing)
            //     {
            //         Vector3 unclampedCursorWorldPos = playerObjectPioComponent.TargetCursorHitWorldPosition;
            //         
            //         Vector3 clampedCursorWorldPos = playerObjectPioComponent.ClampedTargetCursorHitWorldPosition;
            //         
            //         if (Vector3.Distance(unclampedCursorWorldPos, clampedCursorWorldPos) > 0.01f)
            //         {
            //             Vector3 playerObjectPos = playerObjectPioComponent.CurrentObjectPosition;
            //             
            //             Vector3 direction = (clampedCursorWorldPos - playerObjectPos).normalized;
            //             
            //             float distance = Vector3.Distance(playerObjectPos, clampedCursorWorldPos);
            //             
            //             Vector3 correctedLocation = playerObjectPos + direction * distance;
            //             
            //             clampedTargetGpPos = MainCamera.Instance.Camera.WorldToScreenPoint( correctedLocation);
            //         }
            //     }
            // }
            
            //updating virtual mouse position
            InputState.Change(_mouse.position, clampedTargetGpPos);
            
            // calculating delta for potential use
            Vector2 delta = clampedTargetGpPos - gamepadMousePos;
            
            //using delta to update virtual mouse delta
            InputState.Change(_mouse.delta, delta);
            
            //updating cursor position
            AnchorCursor(clampedTargetGpPos);
            
            //handling gamepad click state
            _mouse.CopyState<MouseState>(out var gamepadMouseState);
            
            //setting left button state based on cursorPressed
            gamepadMouseState = cursorPressed
                ? gamepadMouseState.WithButton(MouseButton.Left)
                : gamepadMouseState.WithButton(MouseButton.Left, false);
            
            //applying state change
            InputState.Change(_mouse, gamepadMouseState);
        }
        // unhandled control scheme
        else
        {
            Debug.LogError("PlayerCursor: Unhandled control scheme: " + pioPlayerInput.currentControlScheme);
        }
    }
}
    