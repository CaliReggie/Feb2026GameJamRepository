using UnityEngine;
using UnityEngine.Serialization;

public class SceneEnvironment : Singleton<SceneEnvironment>
{
    [Header("Inscribed References")] 
    
    [SerializeField] private Transform spawnPoint;
    
    public Transform SpawnPoint => spawnPoint;

    protected override void Awake()
    {
        base.Awake();
        
        if (spawnPoint == null)
        {
            Debug.LogError($"{GetType().Name}: Error checking inscribed references, disabling self.");
            
            enabled = false;
            
            return;
        }
    }
}
