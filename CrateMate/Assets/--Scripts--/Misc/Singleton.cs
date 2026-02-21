using UnityEngine;

/// <summary>
/// Static instance singleton. Similar to a singleton, but instead of destroying new instances, it overrides existing
/// ones to this.
/// </summary>
public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake() => Instance = this as T;
    
    protected virtual void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    protected virtual void OnApplicationQuit()
    {
        Instance = null;

        Destroy(gameObject);
    }
}
 /// <summary>
 /// Basic singleton. Destroys new instances created, and leaves the original.
 /// </summary>
public abstract class Singleton<T> : StaticInstance<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            
            return;
        }

        base.Awake();
    }
}

/// <summary>
/// Persistent singleton. Inherits all singleton behaviour, along with additionally not destroying on scene load.
/// </summary>
public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        
        DontDestroyOnLoad(gameObject);
    }
}


