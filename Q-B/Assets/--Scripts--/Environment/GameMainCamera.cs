using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameMainCamera : MainCamera
{
    [Header("Game Main Camera Inscribed References")]
    
    [SerializeField] private Transform cameraLookAtTarget;
    
    [Header("Game Main Camera Dynamic References - Don't Modify In Inspector")]
    
    [SerializeField] private Vector3 initialTargetFollowPosition;
    
    [SerializeField] private Vector3 targetFollowPosition;
    
    /// <summary>
    /// Dictionary mapping position tracking requestors (PlayerIDs) to the positions they want the camera to follow.
    /// The camera will follow the average of all tracked positions.
    /// If no positions are being tracked, it will follow the initial target follow position.
    /// </summary>
    private Dictionary<int, Vector3> trackedPositions = new ();
    
    public void UpdateTrackedPosition(int playerID, Vector3 followTargetPosition)
    {
        trackedPositions[playerID] = followTargetPosition;
        
        Vector3 averagePosition = Vector3.zero;
        
        foreach (Vector3 position in trackedPositions.Values)
        {
            averagePosition += position;
        }
        
        averagePosition /= trackedPositions.Count;
        
        targetFollowPosition = averagePosition;
    }
    
    public void RemoveTrackedPosition(int playerID)
    {
        trackedPositions.Remove(playerID);
        
        if (trackedPositions.Count == 0)
        {
            targetFollowPosition = initialTargetFollowPosition;
        }
        else
        {
            Vector3 averagePosition = Vector3.zero;
        
            foreach (Vector3 position in trackedPositions.Values)
            {
                averagePosition += position;
            }
        
            averagePosition /= trackedPositions.Count;
        
            targetFollowPosition = averagePosition;
        }
    }
    
    protected override void Awake()
    {
        base.Awake();
        
        if (cameraLookAtTarget == null)
        {
            Debug.LogError($"{GetType().Name}: Error checking inscribed references, disabling self.");
            
            enabled = false;
            
            return;
        }
    }
    
    private void Start()
    {
        if (cameraLookAtTarget != null)
        {
            targetFollowPosition = cameraLookAtTarget.position;
            
            initialTargetFollowPosition = cameraLookAtTarget.position;
        }
    }
    
    private void LateUpdate()
    {
        cameraLookAtTarget.position = targetFollowPosition;
    }
    
    private void RemoveAllFollowPositions()
    {
        trackedPositions.Clear();
        
        targetFollowPosition = initialTargetFollowPosition;
    }
}
