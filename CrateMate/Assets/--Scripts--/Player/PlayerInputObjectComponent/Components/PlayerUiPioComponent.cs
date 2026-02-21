using System;
using UnityEngine;

/// <summary>
/// The PlayerUiPioComponent class extends the PioComponent base class to manage the user interface (UI)
/// of the Pio's player input object. It handles the activation and deactivation of various UI canvases
/// based on the Pio's state and settings.
/// </summary>
public class PlayerUiPioComponent : PioComponent
{
    /// <summary>
    /// The different styles of Ui groups to target when toggling on and off based on Pio state and settings.
    /// </summary>
    private enum EToggleTargget
    {
        [Tooltip("Toggles all UI elements.")]
        All,
        [Tooltip("Toggles the player object world space canvas.")]
        PlayerCanvas,
        [Tooltip("Toggles the player object world space UI canvas.")]
        PlayerUICanvas,
        [Tooltip("Toggles both player object world space canvases.")]
        AllPlayer,
        [Tooltip("Toggles the scene screen space canvas.")]
        SceneUICanvas
    }
    
    [Header("Inscribed References")]

    [Tooltip("The canvas used for player object world space UI.")]
    [SerializeField] private Canvas playerObjectCanvas;
    
    [Tooltip("The canvas used for player object world space UI.")]
    [SerializeField] private Canvas playerObjectUiCanvas;
    
    [Tooltip("The canvas used for scene screen space UI.")]
    [SerializeField] private Canvas sceneUiCanvas;
    
    [Header("Dynamic Settings - Don't Modify In Inspector")]
    
    [Tooltip("If true, the player object canvas will persist below the player object ui canvas when in PlayerUi state," +
             " otherwise it will only be active in PlayerUi state.")]
    [SerializeField] private bool persistentPlayerCanvas = true;
    
    /// <summary>
    /// Returns the Pio player object world space canvas.
    /// </summary>
    public Canvas PlayerObjectUiCanvas => playerObjectUiCanvas;
    
    /// <summary>
    /// Returns the Pio scene screen space canvas.
    /// </summary>
    public Canvas SceneUiCanvas => sceneUiCanvas;
    
    protected override void Initialize()
    {
        if (!CheckInscribedReferences())
        {
            return;
        }
        
        // ensure UI starts off
        ToggleUI(EToggleTargget.AllPlayer, false);
        
        Initialized = true;
        
        return;
        
        // check inscribed references
        bool CheckInscribedReferences()
        {
            if (playerObjectCanvas == null ||
                playerObjectUiCanvas == null ||
                sceneUiCanvas == null)
            {
                Debug.LogError($"{GetType().Name}: Error checking inscribed references.");
                
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
                
                // if on, deactivate
                if (enabled)
                {
                    // toggle all off
                    ToggleUI(EToggleTargget.All, false);
                                    
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
                
                ToggleUI(EToggleTargget.All, false);
                
                // for player, just toggle player object canvas
                ToggleUI(EToggleTargget.PlayerCanvas, true);
            
                // if off, activate
                if (!enabled)
                {
                    enabled = true;
                }
                
                break;
            
            case PlayerInputObject.EPlayerInputObjectState.PlayerUi:
                
                ToggleUI(EToggleTargget.All, false);
                
                // for player ui, logic varies based on camera type
                bool isUsingMainCamera = Pio.CurrentPlayerSettings.CameraType == PlayerCameraPioComponent.EPlayerCameraType.MainCamera;
                
                // if using main camera, cursor will use scene ui, enable scene ui
                if (isUsingMainCamera)
                {
                    ToggleUI(EToggleTargget.SceneUICanvas, true);
                }
                // otherwise traditional player ui logic
                else
                {
                    // showing player canvas persistently?
                    if (persistentPlayerCanvas)
                    {
                        ToggleUI(EToggleTargget.AllPlayer, true);
                    }
                    else
                    {
                        ToggleUI(EToggleTargget.PlayerUICanvas, true);
                    }
                }

                // if off, activate
                if (!enabled)
                {
                    enabled = true;
                }
                
                break;
            
            case PlayerInputObject.EPlayerInputObjectState.PlayerSceneUi:
            case PlayerInputObject.EPlayerInputObjectState.SceneUi:
                
                ToggleUI(EToggleTargget.All, false);
                
                // for scene ui, just enable scene ui
                ToggleUI(EToggleTargget.SceneUICanvas, true);
                
                // if off, activate
                if (!enabled)
                {
                    enabled = true;
                }
                
                break;
                
        }
    }

    protected override void OnAfterPioSettingsChange(PlayerSettingsSO playerSettings)
    {
        try
        {
            // update persistent player canvas setting
            persistentPlayerCanvas = playerSettings.PersistentPlayerCanvas;
        }
        catch (Exception e)
        {
            Debug.LogError($"{GetType().Name}:{name}: Error applying PlayerSettingsSO changes: {e.Message}");
        }
        
    }
    
    /// <summary>
    /// Toggles a specific group of UI elements on or off.
    /// </summary>
    private void ToggleUI(EToggleTargget target, bool active)
    {
        switch (target)
        {
            case EToggleTargget.All:
                
                TogglePlayerCanvas(active);
                
                TogglePlayerUICanvas(active);
                
                ToggleApplicationUICanvas(active);
                
                break;
            
            case EToggleTargget.PlayerCanvas:
                
                TogglePlayerCanvas(active);
                
                break;
            
            case EToggleTargget.PlayerUICanvas:
                
                TogglePlayerUICanvas(active);
                
                break;
            
            case EToggleTargget.AllPlayer:
                
                TogglePlayerCanvas(active);
                
                TogglePlayerUICanvas(active);
                
                break;
            
            case EToggleTargget.SceneUICanvas:
                
                ToggleApplicationUICanvas(active);
                
                break;
                
        }
    }
    
    private void TogglePlayerCanvas(bool active)
    {
        playerObjectCanvas.gameObject.SetActive(active);
    }
    
    private void TogglePlayerUICanvas(bool active)
    {
        playerObjectUiCanvas.gameObject.SetActive(active);
    }
    
    private void ToggleApplicationUICanvas(bool active)
    {
        sceneUiCanvas.gameObject.SetActive(active);
    }
}
