using UnityEngine;

abstract public class GameManagerListener : MonoBehaviour
{
     [Header("Dynamic References - Don't Modify In Inspector")]
    
    [SerializeField] protected bool initialized;
    
    protected virtual void Awake()
    {
        if (!initialized)
        {
            Initialize();
        }
    }

    protected abstract void Initialize();

    protected virtual void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnAfterStateChange += OnAfterGameStateChanged;
            
            if (GameManager.Instance.Started)
            {
                OnAfterGameStateChanged(GameManager.Instance.CurrentState.State);
            }
            else
            {
                OnBeforeGameStateChanged(GameManager.EGameState.Initialize);
            }
        }
    }

    protected virtual void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnAfterStateChange -= OnAfterGameStateChanged;
        }
    }

    protected abstract void OnBeforeGameStateChanged(GameManager.EGameState fromState);
    
    // switch (fromState)
    // {
    //     //list all cases empty
    //     case GameManager.EGameState.Initialize:
    //         break;
    //     case GameManager.EGameState.Playing:
    //         break;
    //     case GameManager.EGameState.Paused:
    //         break;
    //     case GameManager.EGameState.GameOver:
    //         break;
    // }

    protected abstract void OnAfterGameStateChanged(GameManager.EGameState toState);
    
    // switch (toState)
    // {
    //     //list all cases empty
    //     case GameManager.EGameState.Initialize:
    //         break;
    //     case GameManager.EGameState.Playing:
    //         break;
    //     case GameManager.EGameState.Paused:
    //         break;
    //     case GameManager.EGameState.GameOver:
    //         break;
    // }

}
