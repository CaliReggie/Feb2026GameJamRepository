using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        [Tooltip("Alternate between running and paused application states.")]
        AlternateState,
        [Tooltip("Load the next highest index SceneSettingsSo from the sceneToLoadPool compared to the current" +
                 "ApplicationManager active SceneSettingsSo." +
                 "If none found, chooses lowest index SceneSettingsSO in pool.")]
        LoadNextScene,
        [Tooltip("Quit the application.")]
        QuitApplication
    }
    
    [Header("Inscribed References")]
    
    [Tooltip("The behaviour of this button when pressed.")]
    [SerializeField] private ESceneManagementButtonType buttonBehaviour = ESceneManagementButtonType.QuitApplication;
    
    [Tooltip("The pool of SceneSettingsSO to choose from when loading a scene.")]
    [SerializeField] private SceneSettingsSO[] sceneToLoadPool;
    
    [Tooltip("The index of the SceneSettingsSo in sceneToLoadPool to load when button is pressed.")]
    [SerializeField] private int sceneToLoadId = -1;
    
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
                if (sceneSo.Id == sceneToLoadId)
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
                 Debug.LogWarning($"{GetType().Name}: Invalid {nameof(sceneToLoadId)} for " +
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
                    ApplicationManager.Instance.TryLoadScene(GetSceneFromId());
                }
                else
                {
                    Debug.LogWarning($"{GetType().Name}: No ApplicationManager instance found in scene. " +
                                   $"Attempting manual load.");
                    
                    if (SceneSettingsSO.IsValidScene(GetSceneFromId()))
                    {
                        SceneManager.LoadScene(GetSceneFromId().TryGetScenePathAsName());
                        
                    }
                    else
                    {
                        Debug.LogError($"Cannot manual load SceneSO: {GetSceneFromId().name}.");
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
            
            case ESceneManagementButtonType.LoadNextScene:
                
                if (ApplicationManager.Instance != null)
                {
                    try
                    {
                        int currentId = ApplicationManager.Instance.ActiveSceneSettings.Id;
                        
                        int targetId = int.MaxValue;
                        
                        // find next highest index in pool
                        foreach (SceneSettingsSO sceneSo in sceneToLoadPool)
                        {
                            if (sceneSo.Id > currentId && sceneSo.Id < targetId)
                            {
                                targetId = sceneSo.Id;
                            }
                        }
                        
                        // if no higher index found, load lowest index in pool
                        if (targetId == int.MaxValue)
                        {
                            // 0 is defaulted to main menu scene design side
                            targetId = 0;
                        }
                        
                        // load target scene
                        foreach (SceneSettingsSO sceneSo in sceneToLoadPool)
                        {
                            if (sceneSo.Id == targetId)
                            {
                                ApplicationManager.Instance.TryLoadScene(sceneSo);
                                return;
                            }
                        }
                        
                        // should not reach here
                        Debug.LogError($"{GetType().Name}: Failed to find target scene to load despite valid index. " +
                                       $"This should not happen.");
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
    
    private SceneSettingsSO GetSceneFromId()
    {
        if (!SceneToLoadPoolIsValid)
        {
            Debug.LogError($"{GetType().Name}: Invalid {nameof(sceneToLoadPool)}. " +
                           $"Cannot get scene to load.");
            
            return null;
        }

        if (!SceneToLoadIdIsValid)
        {
            Debug.LogError($"{GetType().Name}: Invalid {nameof(sceneToLoadId)}. " +
                           $"Cannot get scene to load.");
            
            return null;
        }

        foreach (SceneSettingsSO sceneSo in sceneToLoadPool)
        {
            if (sceneSo.Id == sceneToLoadId)
            {
                return sceneSo;
            }
        }
        
        Debug.LogError($"{GetType().Name}: Scene to load not found in pool despite valid index. " +
                       $"This should not happen.");
        
        return null;
    }
    
    private void Update()
    {
    }
}