using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerManagerSettings", menuName = "ScriptableObjects/PlayerManagerSettings")]
public class PlayerManagerSettingsSO : ScriptableObject
{
    #region Declarations

    [Header("Inscribed Settings - WRITE CHANGES")]
    
    [Space]
    
    [Tooltip("Press when done making changes to the SO")]
    [SerializeField] private bool writeChanges;
    
    [Space]
        
    [Tooltip("Number of players for this configuration")]
    [SerializeField] [Range(1,4)]  private int targetPlayers = 1;
    
    [Space]

    [Tooltip("Settings for each player to use in this configuration, should be unique per player")]
    [SerializeField] private List<PlayerSettingsSO> playersSettings = new ();

    [Space]
    
    [Tooltip("If true, all players need to be requesting time pause for time to be paused")]
    [SerializeField] private bool allNeededToPauseTime = true;
    
    [Tooltip("If true, players' configuration types must match the target configuration type of PlayerManager")]
    [SerializeField] private bool forceConfigurationTypeMatch = true;
    
    [Header("Dynamic Settings - DON'T MODIFY IN INSPECTOR")]
    
    [Tooltip("The current target player configuration type for this PlayerManager configuration")]
    [SerializeField] private PlayerSettingsSO.EPlayerConfigurationType targetPlayerConfigurationType =
        PlayerSettingsSO.EPlayerConfigurationType.Off;
    

    #endregion

    #region Properties

    /// <summary>
    /// Target number of players for this configuration
    /// </summary>
    public int TargetPlayers => targetPlayers;
    
    /// <summary>
    /// The settings for each player in this configuration
    /// </summary>
    public List<PlayerSettingsSO> PlayersSettings => playersSettings;
    
    /// <summary>
    /// If true, all players need to be requesting time pause for time to be paused
    /// </summary>
    public bool AllNeededToPauseTime => allNeededToPauseTime;
    
    /// <summary>
    /// If true, players' configuration types must match the target configuration type of PlayerManager
    /// </summary>
    public bool ForceConfigurationTypeMatch => forceConfigurationTypeMatch;

    /// <summary>
    /// The current target player configuration type for this PlayerManager configuration
    /// </summary>
    public PlayerSettingsSO.EPlayerConfigurationType TargetPlayerConfigurationType
    {
        get => targetPlayerConfigurationType;
        set => targetPlayerConfigurationType = value;
    }

    #endregion

    #region Methods

    /// <summary>
    /// For some reason, changing values elsewhere is required to save values set by Editor extensions.
    /// Hence, the "writeChanges" boolean is used to trigger this method.
    /// </summary>
    private void OnValidate()
    {
        if (writeChanges)
        {
            writeChanges = false;
            
            Debug.Log($"Wrote changes to {GetType().Name}: {name}");
        }
        
        // if playersSettings list size is different then targetPlayers, resize it
        if (playersSettings.Count != targetPlayers)
        {
            if (playersSettings.Count < targetPlayers)
            {
                // add more elements
                int elementsToAdd = targetPlayers - playersSettings.Count;
                for (int i = 0; i < elementsToAdd; i++)
                {
                    playersSettings.Add(null);
                }
            }
            else
            {
                // remove excess elements
                playersSettings.RemoveRange(targetPlayers, playersSettings.Count - targetPlayers);
            }
        }
        
        // if player settings list contains null, reminding to set
        for (int i = 0; i < playersSettings.Count; i++)
        {
            if (playersSettings[i] == null)
            {
                Debug.LogWarning($"{GetType().Name}: {name}: PlayerSettings at index {i + 1} is null." +
                               $" Please assign it.");
            }
        }
    }

    #endregion
}
