using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(CinemachineBrain))]
public class MainCamera : StaticInstance<MainCamera>
{
    [Header("Inscribed References")]

    [Tooltip("The Cinemachine Brain component attached to this camera.")]
    [SerializeField] private CinemachineBrain cinBrain;
    
    /// <summary>
    /// The Camera component attached to this GameObject.
    /// </summary>
    [field: SerializeField] public Camera Camera { get; private set; }
    
    [Tooltip("The GameObject that covers the camera view when no player needs to see from it.")]
    [SerializeField] private GameObject cameraCover;

    
    protected override void Awake()
    {
        if (!CheckInscribedReferences())
        {
            ToggleCamera(false);
            
            return;
        }
        
        // subscribe to PlayerManager settings change event
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnAfterPlayerManagerSettingsChanged += OnAfterPlayerManagerSettingsChanged;
        }

        ToggleCover(false);
        
        base.Awake();
        
        return;
        
        bool CheckInscribedReferences()
        {
            if (cinBrain == null ||
                Camera == null  ||
                cameraCover == null) 
            {
                Debug.LogError($"{GetType().Name}: Error checking inscribed references.");
                
                return false;
            }
            
            return true;
        }
    }

    protected override void OnDestroy()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnAfterPlayerManagerSettingsChanged -= OnAfterPlayerManagerSettingsChanged;
        }
        
        base.OnDestroy();
    }

    private void ToggleCamera(bool active)
    {
        if (Camera != null)
        {
            Camera.enabled = active;
        }
        
        if (cinBrain != null)
        {
            cinBrain.enabled = active;
        }
        
        gameObject.SetActive(active);
    }
    
    private void ToggleCover(bool active)
    {
        if (cameraCover != null)
        {
            cameraCover.SetActive(active);
        }
    }
    
    private void OnAfterPlayerManagerSettingsChanged(PlayerManagerSettingsSO currentPlayerManagerSettings)
    {
        // if a single player needs to see from main camera, disable cover
        for (int i = 0; i < currentPlayerManagerSettings.PlayersSettings.Count; i++)
        {
            PlayerSettingsSO playerSettings = currentPlayerManagerSettings.PlayersSettings[i];

            if (playerSettings.NeedToSeeFromMainCamera)
            {
                ToggleCover(false);
                
                // return if found one
                return;
            }
        }
        
        // otherwise, enable cover
        ToggleCover(true);
    }
}
