using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SceneEnvironment : Singleton<SceneEnvironment>
{
    [Header("Inscribed References")] 
    
    [SerializeField]
    private List<Transform> spawnPoints;
    
    /// <summary>
    /// Get a spawn point transform (1-4) if spawn points are valid.
    /// </summary>
    /// <returns></returns>
    public Transform GetSpawnPoint(int oneBasedIndex)
    {
        if (!SpawnPointsValid())
        {
            Debug.LogError($"{GetType().Name}: Cannot get spawn point because the spawn points list is invalid. " +
                           $"It must be non-null, contain exactly 4 entries, and contain no null entries. " +
                           $"They can be the same Transform or different ones, but they must all be assigned.");
            return null;
        }
        
        oneBasedIndex = Mathf.Clamp(oneBasedIndex, 1, 4);
        
        return spawnPoints[oneBasedIndex - 1];
    }
    
    private void OnValidate()
    {
        if (!SpawnPointsValid())
        {
            Debug.LogWarning($"{GetType().Name}: Spawn points list is invalid. It must be non-null," +
                             $" contain exactly 4 entries, and contain no null entries. " +
                             $"They can be the same Transform or different ones, but they must all be assigned.");
        }
    }

    protected override void Awake()
    {
        base.Awake();
        
        if (!SpawnPointsValid())
        {
            Debug.LogError($"{GetType().Name}: Error checking inscribed references, disabling self.");
            
            enabled = false;
            
            return;
        }
    }
    
    /// <summary>
    /// Returns true if the spawn points is not null, 4 long, and contains no null entries.
    /// </summary>
    /// <returns></returns>
    private bool SpawnPointsValid()
    {
        if (spawnPoints == null)
        {
            return false;
        }
        
        if (spawnPoints.Count != 4)
        {
            return false;
        }

        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint == null)
            {
                return false;
            }
        }

        return true;
    }
}
