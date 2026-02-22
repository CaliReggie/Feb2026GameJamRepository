using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ApplicationManagementButton : MonoBehaviour
{
    private enum ESceneManagementButtonType
    {
        [Tooltip("Load a scene from the assigned sceneToLoadPool.")]
        LoadScene,
        [Tooltip("Reload the current active scene.")]
        ReloadCurrentScene,
        [Tooltip("Load the next highest chronologicalId of a SceneSettingsSo in the SceneToLoadPool." +
                 "If none found, chooses lowest chronologicalId of a SceneSettingsSo in the SceneToLoadPool.")]
        LoadNextScene,
        [Tooltip("Load the next lowest chronologicalId of a SceneSettingsSo in the SceneToLoadPool." +
                 "If none found, does nothing.")]
        LoadPreviousScene,
        [Tooltip("Alternate between running and paused application states.")]
        AlternateState,
        [Tooltip("Quit the application.")]
        QuitApplication
    }
    
    [Header("Inscribed References")]
    
    [Tooltip("The behaviour of this button when pressed.")]
    [SerializeField] private ESceneManagementButtonType buttonBehaviour = ESceneManagementButtonType.QuitApplication;
    
    [Tooltip("The pool of SceneSettingsSO to choose from when loading a scene.")]
    [SerializeField] private SceneSettingsSO[] sceneToLoadPool;
    
    [Tooltip("The corresponding chronologicalId of a SceneSettingsSo from the SceneToLoadPool to load to.")]
    [SerializeField] private int targetChronologicalId = 0;
    
    [Header("Dynamic References - Don't Modify In Inspector")]
    
    [Tooltip("The Button component on this GameObject.")]
    [SerializeField] private Button button;

    /// <summary>
    /// Returns true if conditions are met: pool not null, length > 0,
    /// no null entries, and all entries are valid scenes.
    /// </summary>
    /// <returns></returns>
    private bool SceneToLoadPoolIsValid
    {
        get
        {
            if (sceneToLoadPool == null || sceneToLoadPool.Length == 0) return false;

            foreach (SceneSettingsSO sceneSO in sceneToLoadPool)
            {
                if (sceneSO == null || !SceneSettingsSO.IsValidScene(sceneSO))
                {
                    return false;
                }
            }

            return true;
        }
    }
    
    private bool SceneToLoadIdIsValid
    {
        get
        {
            if (!SceneToLoadPoolIsValid) return false;

            foreach (SceneSettingsSO sceneSo in sceneToLoadPool)
            {
                if (sceneSo.ChronologicalId == targetChronologicalId)
                {
                    return true;
                }
            }
            
            return false;
        }
    }
    
    private void OnValidate()
    {
        if (buttonBehaviour == ESceneManagementButtonType.LoadScene)
        {
            if (!SceneToLoadPoolIsValid)
            {
                Debug.LogWarning($"{GetType().Name}: Invalid {nameof(sceneToLoadPool)} for " +
                                 $"{buttonBehaviour} button behaviour. " +
                                 $"Ensure {nameof(sceneToLoadPool)} is filled with valid entries." );
                
                return;
            }
            
            if (!SceneToLoadIdIsValid)
            {
                 Debug.LogWarning($"{GetType().Name}: Invalid {nameof(targetChronologicalId)} for " +
                                 $"{buttonBehaviour} button behaviour. " +
                                 $"Ensure SceneToLoadIndex correlates to the index of a SceneSettingsSO in " +
                                 $"the sceneToLoadPool" );
                
                return;
            }
        }
    }

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        
        if (buttonBehaviour == ESceneManagementButtonType.LoadScene &&
            (!SceneToLoadPoolIsValid))
        {
            Debug.LogError($"{GetType().Name}: Invalid {nameof(sceneToLoadPool)} for " +
                                 $"{buttonBehaviour} button behaviour. " +
                                 $"Ensure {nameof(sceneToLoadPool)} is filled with valid entries." +
                                 $"Disabling self" );
            
            button.interactable = false;
            
            this.enabled = false;
            
            return;
        }
    }
    
    private void Start()
    {
        button.onClick.AddListener(OnButtonPressed);
    }
    
    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonPressed);
        }
    }
    
    private void OnButtonPressed()
    {
        switch (buttonBehaviour)
        {
            case ESceneManagementButtonType.LoadScene:
                
                if (ApplicationManager.Instance != null)
                {
                    ApplicationManager.Instance.TryLoadScene(GetTargetSceneToLoad());
                }
                else
                {
                    Debug.LogWarning($"{GetType().Name}: No ApplicationManager instance found in scene. " +
                                   $"Attempting manual load.");
                    
                    if (SceneSettingsSO.IsValidScene(GetTargetSceneToLoad()))
                    {
                        SceneManager.LoadScene(GetTargetSceneToLoad().TryGetScenePathAsName());
                        
                    }
                    else
                    {
                        Debug.LogError($"Cannot manual load SceneSO: {GetTargetSceneToLoad().name}.");
                    }
                }
                
                break;
            
            case ESceneManagementButtonType.ReloadCurrentScene:
                
                if (ApplicationManager.Instance != null)
                {
                    ApplicationManager.Instance.TryLoadScene(ApplicationManager.Instance.ActiveSceneSettings);
                }
                else
                {
                    Debug.LogWarning($"{GetType().Name}: No ApplicationManager instance found in scene. " +
                                   $"Attempting manual reload of current scene.");
                    
                    Scene currentScene = SceneManager.GetActiveScene();
                    
                    SceneManager.LoadScene(currentScene.name);
                }
                
                break;
            
            case ESceneManagementButtonType.LoadNextScene:
                
                if (ApplicationManager.Instance != null)
                {
                    try
                    {
                        ApplicationManager.Instance.TryLoadScene(GetNextTargetSceneToLoad());
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"{GetType().Name}: Error loading next scene: {e.Message}");
                    }
                }
                else
                {
                    // log warning can't load next scene without ApplicationManager
                    Debug.LogWarning($"{GetType().Name}: No ApplicationManager instance found in scene. " +
                                   $"Cannot load next scene without ApplicationManager.");
                    
                }
                
                break;
            
            case ESceneManagementButtonType.LoadPreviousScene:
                
                if (ApplicationManager.Instance != null)
                {
                    try
                    {
                        ApplicationManager.Instance.TryLoadScene(GetPreviousTargetSceneToLoad());
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"{GetType().Name}: Error loading previous scene: {e.Message}");
                    }
                }
                else
                {
                    // log warning can't load previous scene without ApplicationManager
                    Debug.LogWarning($"{GetType().Name}: No ApplicationManager instance found in scene. " +
                                   $"Cannot load previous scene without ApplicationManager.");
                }
                
                break;
            
            case ESceneManagementButtonType.AlternateState:
                
                if (ApplicationManager.Instance != null)
                {
                    ApplicationManager.Instance.ToggleRunningOrPausedState();
                }
                else
                {
                    Debug.LogWarning($"{GetType().Name}: No ApplicationManager instance found in scene. " +
                                   $"Attempting manual state swap with standalone Player Input Object.");
                    try
                    {
                        PlayerInputObject pio = FindAnyObjectByType<PlayerInputObject>();

                        pio.TogglePlayerSettingsConfigurationType();

                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"{GetType().Name}: Error in manual state swap: {e.Message}");
                        throw;
                    }
                }
                break;
            
            case ESceneManagementButtonType.QuitApplication:
            
                if (ApplicationManager.Instance != null)
                {
                    ApplicationManager.Instance.Quit();
                }
                else
                {
                    Debug.LogWarning($"{GetType().Name}: No ApplicationManager instance found in scene. " +
                                   $"Attempting manual quit.");
                    
                    #if UNITY_EDITOR
                    
                    EditorApplication.isPlaying = false;
                    
                    #else 
                    
                    Application.Quit();

                    #endif
                }
            
                break;
        }
    }
    
    private SceneSettingsSO GetTargetSceneToLoad()
    {
        if (!SceneToLoadPoolIsValid)
        {
            Debug.LogError($"{GetType().Name}: Invalid {nameof(sceneToLoadPool)}. " +
                           $"Cannot get scene to load.");
            
            return null;
        }

        if (!SceneToLoadIdIsValid)
        {
            Debug.LogError($"{GetType().Name}: Invalid {nameof(targetChronologicalId)}. " +
                           $"Cannot get scene to load.");
            
            return null;
        }

        foreach (SceneSettingsSO sceneSo in sceneToLoadPool)
        {
            if (sceneSo.ChronologicalId == targetChronologicalId)
            {
                return sceneSo;
            }
        }
        
        Debug.LogError($"{GetType().Name}: Scene to load not found in pool despite valid index. " +
                       $"This should not happen.");
        
        return null;
    }
    
    private SceneSettingsSO GetNextTargetSceneToLoad()
    {
        if (!SceneToLoadPoolIsValid)
        {
            Debug.LogError($"{GetType().Name}: Invalid {nameof(sceneToLoadPool)}. " +
                           $"Cannot get next scene to load.");
            
            return null;
        }

        if (ApplicationManager.Instance == null)
        {
            Debug.LogError($"{GetType().Name}: No ApplicationManager instance found in scene. " +
                           $"Cannot get next scene to load without ApplicationManager.");
            
            return null;
        }

        int currentChronologicalId = ApplicationManager.Instance.ActiveSceneSettings.ChronologicalId;

        int currentTargetChronologicalId = int.MaxValue;

        // find next highest index in pool
        foreach (SceneSettingsSO sceneSo in sceneToLoadPool)
        {
            if (sceneSo.ChronologicalId > currentChronologicalId && sceneSo.ChronologicalId < currentTargetChronologicalId)
            {
                currentTargetChronologicalId = sceneSo.ChronologicalId;
            }
        }

        // if no higher index found, load lowest index in pool
        if (currentTargetChronologicalId == int.MaxValue)
        {
            // 0 is defaulted to main menu scene design side
            currentTargetChronologicalId = 0;
        }

        // load target scene
        foreach (SceneSettingsSO sceneSo in sceneToLoadPool)
        {
            if (sceneSo.ChronologicalId == currentTargetChronologicalId)
            {
                return sceneSo;
            }
        }
        
        Debug.LogError($"{GetType().Name}: Failed to find next target scene to load despite valid pool. " +
                       $"This should not happen.");
        
        return null;
    }
    
    private SceneSettingsSO GetPreviousTargetSceneToLoad()
    {
        if (!SceneToLoadPoolIsValid)
        {
            Debug.LogError($"{GetType().Name}: Invalid {nameof(sceneToLoadPool)}. " +
                           $"Cannot get previous scene to load.");
            
            return null;
        }

        if (ApplicationManager.Instance == null)
        {
            Debug.LogError($"{GetType().Name}: No ApplicationManager instance found in scene. " +
                           $"Cannot get previous scene to load without ApplicationManager.");
            
            return null;
        }

        int currentChronologicalId = ApplicationManager.Instance.ActiveSceneSettings.ChronologicalId;

        int currentTargetChronologicalId = int.MinValue;

        // find next lowest index in pool
        foreach (SceneSettingsSO sceneSo in sceneToLoadPool)
        {
            if (sceneSo.ChronologicalId < currentChronologicalId && sceneSo.ChronologicalId > currentTargetChronologicalId)
            {
                currentTargetChronologicalId = sceneSo.ChronologicalId;
            }
        }

        // if no lower index found, do nothing
        if (currentTargetChronologicalId == int.MinValue)
        {
            Debug.LogWarning($"{GetType().Name}: No previous scene to load found in pool. " +
                           $"Staying on current scene.");
            
            return null;
        }

        // load target scene
        foreach (SceneSettingsSO sceneSo in sceneToLoadPool)
        {
            if (sceneSo.ChronologicalId == currentTargetChronologicalId)
            {
                return sceneSo;
            }
        }
        
        Debug.LogError($"{GetType().Name}: Failed to find previous target scene to load despite valid pool. " +
                       $"This should not happen.");
        
        return null;
    }

    private void OnEnable()
    {
        if (buttonBehaviour == ESceneManagementButtonType.LoadScene)
        {
            button.interactable = PlayerPrefsManager.IsSceneUnlocked(targetChronologicalId);
        }
    }
}