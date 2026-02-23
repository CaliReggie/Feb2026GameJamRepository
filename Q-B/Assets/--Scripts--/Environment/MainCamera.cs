using System;
using System.Collections;
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
    
    private void Start()
    {
        // if PlayerManager exists subscribe to PlayerManager settings change event
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnAfterPlayerManagerSettingsChange += OnAfterPlayerManagerSettingsChange;
            
            PlayerManager.Instance.OnAfterStateChange += OnAfterPlayerManagerStateChange;
            
            if (PlayerManager.Instance.Started)
            {
                // trigger the event manually for instant correct state
                OnAfterPlayerManagerSettingsChange(PlayerManager.Instance.CurrentPlayerManagerSettings);
            }
        }
    }

    protected override void OnDestroy()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnAfterPlayerManagerSettingsChange -= OnAfterPlayerManagerSettingsChange;
            
            PlayerManager.Instance.OnAfterStateChange -= OnAfterPlayerManagerStateChange;
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
    
    private void OnAfterPlayerManagerStateChange(PlayerManager.EPlayerManagementState toState)
    {
        switch (toState)
        {
            case PlayerManager.EPlayerManagementState.SufficientPlayers:
                CheckPlayerManagerSettingsForCover(PlayerManager.Instance.CurrentPlayerManagerSettings);
                break;
        }
    }
    
    private void OnAfterPlayerManagerSettingsChange(PlayerManagerSettingsSO currentPlayerManagerSettings)
    {
        CheckPlayerManagerSettingsForCover(currentPlayerManagerSettings);
    }
    
    private void CheckPlayerManagerSettingsForCover(PlayerManagerSettingsSO currentPlayerManagerSettings)
    {
        StopAllCoroutines();
        StartCoroutine(DelayedCheckPlayerManagerSettingsForCover(0.05f, currentPlayerManagerSettings));
    }
    
    private IEnumerator DelayedCheckPlayerManagerSettingsForCover(float delay, PlayerManagerSettingsSO currentPlayerManagerSettings)
    {
        yield return new WaitForSeconds(delay);
        
        // if a single player needs to see from main camera, disable cover
        for (int i = 0; i < currentPlayerManagerSettings.PlayersSettings.Count; i++)
        {
            PlayerSettingsSO playerSettings = currentPlayerManagerSettings.PlayersSettings[i];

            if (playerSettings.NeedToSeeFromMainCamera)
            {
                ToggleCover(false);
                
                // return if found one
                yield break;
            }
        }
        
        // otherwise, enable cover
        ToggleCover(true);
    }
}
