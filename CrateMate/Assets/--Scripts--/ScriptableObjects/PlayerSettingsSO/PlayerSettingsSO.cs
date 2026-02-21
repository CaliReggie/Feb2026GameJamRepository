using System;
using UnityEngine;
using UnityEditor;

#region PlayerSettingsSOEditor

#if UNITY_EDITOR

/// <summary>
/// Custom editor for PlayerSettingsSO to handle settings references from a scene.
/// </summary>
[CustomEditor(typeof(PlayerSettingsSO))]
public class PlayerSettingsSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PlayerSettingsSO playerSettingsSo = (PlayerSettingsSO)target;

        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Referenced Settings - WRITE CHANGES", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Spawn Transform will not stay, watch written information!", EditorStyles.helpBox);
        
        playerSettingsSo.DefaultSpawn = (Transform)EditorGUILayout.ObjectField("Spawn Transform",
            playerSettingsSo.DefaultSpawn, typeof(Transform),
            true);
        
        DrawDefaultInspector();
    }
}

#endif

#endregion

#region PlayerSettingsSOClass

[CreateAssetMenu( fileName = "PlayerSettings", menuName = "ScriptableObjects/PlayerSettings")]
public class PlayerSettingsSO : ScriptableObject
{
    #region Declarations

    /// <summary>
    /// The type of player configuration currently active.
    /// </summary>
    public enum EPlayerConfigurationType
    {
        Off,
        Default,
        Alternate
    }
    
    [Header("Inscribed Settings - WRITE CHANGES")]
    
    [Space]
    
    [Tooltip("Press when done making changes to the SO")]
    [SerializeField] private bool writeChanges;
    
    [Space]
    
    [Header("Default Configuration Specific")]
    
    [Tooltip("The default player input object configuration for the Pio to be in.")]
    [SerializeField] private PioStateConfiguration defaultConfiguration;
    
    [Header("Alternate Configuration Specific")]
    
    [Tooltip("The alternate player input object configuration for the Pio to be in.")]
    [SerializeField] private PioStateConfiguration alternateConfiguration;
    
    [Header("Configuration General")]
    
    [Tooltip("Whether manual switching between configurations is allowed. " +
             "Effectively disables non-default configurations if false.")]
    [SerializeField] private bool allowManualSwitching = true;
    
    [Header("Player General")]
    
    [Tooltip("Whether to use the environment spawn point from an environment prefab in scene." +
             " (if false, uses spawn position and rotation below).")]
    [SerializeField] private bool useEnvironmentSpawnPoint = true;

    [Tooltip("The spawn position for the player object.")]
    [SerializeField] private Vector3 spawnPosition;
    
    [Tooltip("The spawn rotation (Euler angles) for the player object.")]
    [SerializeField] private Vector3 spawnEulerRotation;

    [Space]
    
    [Header("Player Camera General")] 
    
    [Tooltip("The type of camera the player object uses.")]
    [SerializeField] private PlayerCameraPioComponent.EPlayerCameraType cameraType =
        PlayerCameraPioComponent.EPlayerCameraType.PlayerThirdFixed;
      
    [Tooltip("The sensitivity multiplier of the player camera.")]
    [Range(0.01f, 1)] [SerializeField] private float cameraSensitivity = 1f;
    
    [Header("Player UI General")]
    
    [Tooltip("If true, the player object canvas will persist below the player object ui canvas when in" +
             "player ui state.")] 
    [SerializeField] private bool persistentPlayerCanvas = true;
    
    [Header("Player Cursor General")]
    
    [Tooltip("The sprite used for the player cursor.")]
    [SerializeField] private Sprite cursorSprite = null;
    
    [Header("Dynamic Settings - DON'T MODIFY IN INSPECTOR")]
    
    [Tooltip("The current player configuration type active.")]
    [SerializeField] private EPlayerConfigurationType currentConfigurationType = 
        EPlayerConfigurationType.Off;
    
    #if UNITY_EDITOR
            
    /// <summary>
    /// Dummy property to set spawn transform from editor.
    /// </summary>
    public Transform DefaultSpawn
    {
        get => null;
        set
        {
            if (value == null) return;
                    
            spawnPosition = value.position;
                    
            spawnEulerRotation = value.eulerAngles;
        }
    }
            
    #endif

    #endregion

    #region Properties
    
    /// <summary>
    /// Whether to use the environment spawn point from an environment prefab in scene.
    /// (if false, uses spawn position and rotation below).
    /// </summary>
    public bool UseEnvironmentSpawnPoint => useEnvironmentSpawnPoint;

    /// <summary>
    /// The spawn position for the player object.
    /// </summary>
    public Vector3 SpawnPosition => spawnPosition;
    
    /// <summary>
    /// The spawn rotation (Euler angles) for the player object.
    /// </summary>
    public Vector3 SpawnEulerRotation => spawnEulerRotation;
    
    /// <summary>
    /// The type of camera the player object uses.
    /// </summary>
    public PlayerCameraPioComponent.EPlayerCameraType CameraType => cameraType;
    
    /// <summary>
    /// The sensitivity multiplier of the player camera.
    /// </summary>
    public float CameraSensitivity => cameraSensitivity;
    
    /// <summary>
    /// The sprite used for the player cursor.
    /// </summary>
    public Sprite CursorSprite => cursorSprite;
    
    /// <summary>
    /// If true, the player object canvas will persist below the player object ui canvas when in
    /// player object ui state.
    /// </summary>
    public bool PersistentPlayerCanvas => persistentPlayerCanvas;
    
    /// <summary>
    /// The generic "Off" configuration for where the Pio is off with no game control/effect.
    /// </summary>
    public static PioStateConfiguration OffConfiguration => new PioStateConfiguration();
    
    /// <summary>
    /// The default player input object configuration for a Pio to be in.
    /// </summary>
    public PioStateConfiguration DefaultConfiguration => defaultConfiguration;
    
    /// <summary>
    /// The alternate player input object configuration for a Pio to be in.
    /// </summary>
    
    public PioStateConfiguration AlternateConfiguration => alternateConfiguration;
    
    /// <summary>
    /// Whether manual switching between configurations is allowed.
    /// </summary>
    public bool AllowManualSwitching => allowManualSwitching;
    
    /// <summary>
    /// The current player configuration type active.
    /// </summary>
    public EPlayerConfigurationType CurrentConfigurationType 
    {
        get => currentConfigurationType;
        set => currentConfigurationType = value;
    }

    /// <summary>
    /// The current player input object configuration based on the current configuration type.
    /// </summary>
    public PioStateConfiguration CurrentConfiguration 
    {
        get
        {
            switch (CurrentConfigurationType)
            {
                case EPlayerConfigurationType.Off:
                    return OffConfiguration;
                case EPlayerConfigurationType.Default:
                    return defaultConfiguration;
                case EPlayerConfigurationType.Alternate:
                    return alternateConfiguration;
                default:
                    Debug.LogWarning($"Unknown {nameof(EPlayerConfigurationType)}: {CurrentConfigurationType}. " +
                                     $"Returning Off configuration.");
                    return OffConfiguration;
            }
        }
    }
    
    /// <summary>
    /// True if the current configuration is Player state with Main Camera type.
    /// </summary>
    private bool IsPlayerWithMainCamera =>
        CurrentConfiguration.State == PlayerInputObject.EPlayerInputObjectState.Player
         && cameraType == PlayerCameraPioComponent.EPlayerCameraType.MainCamera;
    
    /// <summary>
    /// True if the current configuration is PlayerUi state with Main Camera type.
    /// </summary>
    private bool IsPlayerUiWithMainCamera =>
        CurrentConfiguration.State == PlayerInputObject.EPlayerInputObjectState.PlayerUi
        && cameraType == PlayerCameraPioComponent.EPlayerCameraType.MainCamera;
    
    /// <summary>
    /// True if the current configuration is PlayerSceneUi state with Main Camera type.
    /// </summary>
    private bool IsPlayerSceneUiWithMainCamera =>
        CurrentConfiguration.State == PlayerInputObject.EPlayerInputObjectState.PlayerSceneUi
        && cameraType == PlayerCameraPioComponent.EPlayerCameraType.MainCamera;
    
    /// <summary>
    /// True if the current configuration is PlayerSceneUi state.
    /// </summary>
    private bool IsPlayerSceneUi =>
        CurrentConfiguration.State == PlayerInputObject.EPlayerInputObjectState.PlayerSceneUi;

    /// <summary>
    /// True if the current configuration is SceneUi state.
    /// </summary>
    private bool IsSceneUi =>
        CurrentConfiguration.State == PlayerInputObject.EPlayerInputObjectState.SceneUi;
    
    /// <summary>
    /// True if the player needs to see from the main camera.
    /// </summary>
    public bool NeedToSeeFromMainCamera =>
        (IsPlayerWithMainCamera || IsPlayerUiWithMainCamera || IsPlayerSceneUiWithMainCamera) || IsSceneUi;
    
    /// <summary>
    /// True if the player needs to see the scene pause UI.
    /// </summary>
   public bool NeedToSeeScenePauseUi =>
   ( IsPlayerUiWithMainCamera || IsPlayerSceneUi || IsSceneUi) && CurrentConfiguration.PauseTime;

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
    }

    #endregion
}

#endregion

#region PioStateConfigurationEditor

# if UNITY_EDITOR

[CustomPropertyDrawer(typeof(PioStateConfiguration))]
public class PioStateConfigurationDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        float line = EditorGUIUtility.singleLineHeight;
        float space = EditorGUIUtility.standardVerticalSpacing;
        float y = position.y;
        
        position = EditorGUI.PrefixLabel(position, label);

        y += line + space;

        var stateProp = property.FindPropertyRelative("state");
        var pauseTimeProp = property.FindPropertyRelative("pauseTime");
        // var cameraTypeProp = property.FindPropertyRelative("cameraType");
        // var cursorSpriteProp = property.FindPropertyRelative("cursorSprite");
        var cursorConstrainedProp = property.FindPropertyRelative("cursorConstrained");
        // var persistentPlayerCanvasProp = property.FindPropertyRelative("persistentPlayerCanvas");
        
        DrawGeneralSettings();

        // Variable logic
        var stateEnum =
            (PlayerInputObject.EPlayerInputObjectState)stateProp.enumValueIndex;

        switch (stateEnum)
        {
            case PlayerInputObject.EPlayerInputObjectState.Player:
                DrawPlayerSettings();
                break;
            case PlayerInputObject.EPlayerInputObjectState.PlayerUi:
            case PlayerInputObject.EPlayerInputObjectState.PlayerSceneUi:
                DrawGeneralUISettings();
                DrawPlayerUISettings();
                break;
            case PlayerInputObject.EPlayerInputObjectState.SceneUi:
                DrawGeneralUISettings();
                DrawApplicationUISettings();
                break;
        }

        EditorGUI.EndProperty();
        
        return;
        
        void DrawGeneralSettings()
        {
            EditorGUI.PropertyField(
                new Rect(position.x, y, position.width, line),
                stateProp
            );
            y += line + space;
            EditorGUI.PropertyField(
                new Rect(position.x, y, position.width, line),
                pauseTimeProp,
                new GUIContent("Pause Time")
            );
            y += line + space;
        }
        
        void DrawGeneralUISettings()
        {
            // EditorGUI.PropertyField(
            //     new Rect(position.x, y, position.width, line),
            //     cursorSpriteProp,
            //     new GUIContent("Cursor Sprite")
            // );
            // y += line + space;
            EditorGUI.PropertyField(
                new Rect(position.x, y, position.width, line),
                cursorConstrainedProp,
                new GUIContent("Cursor Constrained")
            );
            y += line + space;
        }
        
        void DrawPlayerSettings()
        {
            // EditorGUI.PropertyField(
            //     new Rect(position.x, y, position.width, line),
            //     cameraTypeProp,
            //     new GUIContent("Camera Type")
            // );
            // y += line + space;
        }
        
        void DrawPlayerUISettings()
        {
            // EditorGUI.PropertyField(
            //     new Rect(position.x, y, position.width, line),
            //     persistentPlayerCanvasProp,
            //     new GUIContent("Persistent Player Canvas")
            // );
            // y += line + space;
        }
        
        void DrawApplicationUISettings()
        {
            // Nothing yet
        }
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;
        float space = EditorGUIUtility.standardVerticalSpacing;

        int lines = PioStateConfiguration.GeneralLines + 1; // includes label line

        var stateProp = property.FindPropertyRelative("state");
        
        var stateEnum =
            (PlayerInputObject.EPlayerInputObjectState)stateProp.enumValueIndex;
        
        switch (stateEnum)
        {
            case PlayerInputObject.EPlayerInputObjectState.Player:
                lines += PioStateConfiguration.PlayerLines;
                break;
            case PlayerInputObject.EPlayerInputObjectState.PlayerUi:
            case PlayerInputObject.EPlayerInputObjectState.PlayerSceneUi:
                lines += PioStateConfiguration.GeneralUILines;
                lines += PioStateConfiguration.PlayerUILines;
                break;
            case PlayerInputObject.EPlayerInputObjectState.SceneUi:
                lines += PioStateConfiguration.GeneralUILines;
                lines += PioStateConfiguration.ApplicationUILines;
                break;
        }
        
        return lines * line + (lines - 1) * space;
    }
}

# endif

#endregion

#region PioStateConfigurationClass

[Serializable]
public class PioStateConfiguration
{

    //General State Settings
    
    [Tooltip("The corresponding state of the player input object for this configuration.")]
    [SerializeField] private PlayerInputObject.EPlayerInputObjectState state = 
        PlayerInputObject.EPlayerInputObjectState.Off;
    
    [Tooltip("If true, time is desired to be paused for this player when in this state.")]
    [SerializeField] private bool pauseTime = false;
    
    public const int GeneralLines = 2;
    
    //General UI State Settings
    
    [Tooltip("If true, the cursor is constrained to player screen bounds when in this state. Otherwise whole screen.")]
    [SerializeField] private bool cursorConstrained = false;
    
    public const int GeneralUILines = 1;
    
    //Player State Settings (Currently none)
    
    public const int PlayerLines = 0;
    
    //PlayerUi State Settings (Currently none)
    
    public const int PlayerUILines = 0;
    
    // SceneUi State Settings (Currently none)
    
    public const int ApplicationUILines = 0;
        
    // Properties
        
    /// <summary>
    /// The corresponding state of the player input object for this configuration.
    /// </summary>
    public PlayerInputObject.EPlayerInputObjectState State => state;
        
    /// <summary>
    /// If true, time is desired to be paused for this player when in this state.
    /// </summary>
    public bool PauseTime => pauseTime;
    
    /// <summary>
    /// If true, the cursor is constrained to player screen bounds when in this state. otherwise whole screen.
    /// </summary>
    public bool CursorConstrained => cursorConstrained;
}

#endregion