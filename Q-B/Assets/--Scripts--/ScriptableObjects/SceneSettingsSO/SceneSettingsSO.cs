using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

#region SceneSettingsSoEditor

#if UNITY_EDITOR

[CustomEditor(typeof(SceneSettingsSO))]
public class SceneSettingsSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SceneSettingsSO sceneSettingsSo = (SceneSettingsSO)target;
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Referenced Settings - WRITE CHANGES", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Make sure to include Scene Asset in build settings!", EditorStyles.helpBox);
        
        sceneSettingsSo.SceneAsset = (SceneAsset)EditorGUILayout.ObjectField("Scene Asset",
            sceneSettingsSo.SceneAsset, typeof(SceneAsset),
            false);
        
        DrawDefaultInspector();
    }
}

#endif

#endregion

#region SceneSettingsSOClass

[CreateAssetMenu(fileName = "NewSceneSettingsSO", menuName = "ScriptableObjects/SceneSettingsSO")]
public class SceneSettingsSO : ScriptableObject
{
    #region Declarations

    [Header("Scene Path - WRITE CHANGES")]
    
    [Tooltip("The raw path to the scene asset. " +
             "This is set automatically when assigning a SceneAsset in the inspector.")]
    [SerializeField] private string scenePath;

    [Tooltip("Press when done making changes to the SO")]
    [SerializeField] private bool writeChanges;
    
    [Header("Player Manager Settings")] 
    
    [Tooltip("The player manager settings for this scene.")]
    [SerializeField] private PlayerManagerSettingsSO playerManagerSettings;
    
    [Header("Level Settings")] 
    
    [Tooltip("The id of the scene to be set on creation, must be different that any other existing constantId, " +
             "and then NEVER CHANGED. This is for application structure purposes and does not affect loading.")]
    [SerializeField] private int constantId = -1;
    
    [Tooltip("The id of the scene in terms of desired game progression. 0 is default and reserved for main menu," +
             " 1 for first level, etc. Negative values can be implemented for special scenes/levels.")]
    [SerializeField] private int chronologicalId = -1;
    
    [Tooltip("The name of the scene. If left empty, the chronologicalId will be used as the name." +
             " This is for display purposes and does not affect loading.")]
    [SerializeField] private string nameId;
    
    [Header("Transition Settings")]
    
    [Tooltip("The style of transition to use for scene transitions.")]
    public ApplicationManager.ApplicationManagerContext.ETransitionStyle transitionStyle =
        ApplicationManager.ApplicationManagerContext.ETransitionStyle.ColorTransition;
    
    [Tooltip("The animation curve used for transitioning in the scene.")]
    public AnimationCurve transitionInCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    [Tooltip("The duration of the transition in the scene.")]
    public float transitionInDuration = 0.75f;
    
    [Tooltip("The color of the transition image when the screen is uncovered.")]
    public Color uncoveredColor = Color.clear;
    
    [Tooltip("The animation curve used for transitioning out of the scene.")]
    public AnimationCurve transitionOutCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    [Tooltip("The duration of the transition out of the scene.")]
    public float transitionOutDuration = 0.75f;
    
    [Tooltip("The color of the transition image when the screen is covered.")]
    public Color coveredColor = Color.black;
    
    [Header("General Settings")]
    [Tooltip("The target frame rate for this scene." +
             " This is set on scene load and can be used to optimize performance for different scenes.")]
    [SerializeField] private int targetFrameRate = 90;
    
    #if UNITY_EDITOR
    public SceneAsset SceneAsset
    { 
        get // Gets the SceneAsset from the scenePath if previously set
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogWarning("Scene path is not set.");
                
                return null;
            }
            
            try { return AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath); }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to find SceneAsset from path '{scenePath}': {e.Message}");
                
                return null;
            }
        }
        set
        {
            // Sets the scenePath from the SceneAsset
            scenePath = AssetDatabase.GetAssetPath(value);
        }
    }
            
    #endif

    #endregion

    #region Properties

    /// <summary>
    /// The player manager settings for this scene.
    /// </summary>
    public PlayerManagerSettingsSO PlayerManagerSettings => playerManagerSettings;
    
    /// <summary>
    /// The constant id of the scene, which should be unique from existing ConstantIds and never changed after creation.
    /// This is for application structure purposes and does not affect loading.
    /// </summary>
    public int ConstantId => constantId;
    
    /// <summary>
    /// The chronological id of the scene, which should be set according to desired game progression.
    /// 0 is default and reserved for main menu, 1 for first level, etc.
    /// Negative values can be implemented for special scenes/levels.
    /// </summary>
    public int ChronologicalId => chronologicalId;
    
    /// <summary>
    /// The name of the scene, which is for display purposes and does not affect loading.
    /// If left empty, the chronologicalId will be used as the name.
    /// </summary>
    public string Name => string.IsNullOrEmpty(nameId) ? $"{constantId}" : nameId;
    
    /// <summary>
    ///  The style of transition to use for scene transitions.
    /// </summary>
    public ApplicationManager.ApplicationManagerContext.ETransitionStyle TransitionStyle => transitionStyle;
    
    /// <summary>
    ///  The animation curve used for transitioning in the scene.
    /// </summary>
    public AnimationCurve TransitionInCurve => transitionInCurve;
    
    /// <summary>
    ///  The duration of the transition in the scene.
    /// </summary>
    public float TransitionInDuration => transitionInDuration;
    
    /// <summary>
    ///  The color of the transition image when the screen is uncovered.
    /// </summary>
    public Color UncoveredColor => uncoveredColor;
    
    /// <summary>
    ///  The animation curve used for transitioning out of the scene.
    /// </summary>
    public AnimationCurve TransitionOutCurve => transitionOutCurve;
    
    /// <summary>
    ///  The duration of the transition out of the scene.
    /// </summary>
    public float TransitionOutDuration => transitionOutDuration;
    
    /// <summary>
    ///  The color of the transition image when the screen is covered.
    /// </summary>
    public Color CoveredColor => coveredColor;
    
    /// <summary>
    /// The target frame rate for this scene.
    /// This is set on scene load and can be used to optimize performance for different scenes.
    /// </summary>
    public int TargetFrameRate => targetFrameRate;

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
            
            Debug.Log($"Wrote changes to {GetType().Name}: {((Object)this).name}");
        }
    }

    /// <summary>
    /// Takes the raw path of a scene asset and converts it to a name to load from and returns it, or nothing
    /// </summary>
    public String TryGetScenePathAsName()
    {
        // starts after last '/' and ends before last '.'
        int startIndex = scenePath.LastIndexOf('/') + 1;
        
        int endIndex = scenePath.LastIndexOf('.');
        
        // return the substring if valid indices found
        if (startIndex >= 0 || endIndex > startIndex)
        {
            return scenePath.Substring(startIndex, endIndex - startIndex);
        }
        else
        {
            Debug.LogWarning($"Got invalid scene path: {scenePath}");

            return null;
        }
    }
    
    /// <summary>
    /// Ensures a path is set and valid in a target SceneSettingsSO.
    /// </summary>
    public static bool IsValidScene (SceneSettingsSO sceneSettingsSo)
    {
        try
        {
            string scenePathAsName = sceneSettingsSo.TryGetScenePathAsName();
            
            // true if the scene path isn't null or empty
            if (!String.IsNullOrEmpty(scenePathAsName))
            {
                return true;
            }
            else
            {
                Debug.LogWarning($"SceneSettingSO '{((Object)sceneSettingsSo).name}' is invalid for runtime use. Check it's path or inclusion" +
                                 $"in build settings.");
                
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to validate SceneSettingsSO: {e.Message}");
            
            return false;
        }
        
    }
    
    /// <summary>
    /// True if the current active scene matches the one set in a passed SceneSettingsSO.
    /// </summary>
    public static bool IsActiveScene(SceneSettingsSO sceneSettingsSo)
    {
        return SceneManager.GetActiveScene().name == sceneSettingsSo.TryGetScenePathAsName();
    }
    
    #endregion
}

#endregion