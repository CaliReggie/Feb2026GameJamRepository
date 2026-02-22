using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

public class DeliveryZone : MonoBehaviour
{
    [Header("Inscribed Settings")]

    [SerializeField] private int numPackagesToWin;

    [SerializeField] private LayerMask interactionLayers;
    
    [Header("Dynamic Settings - Don't Modify In Inspector")]
    
    [SerializeField] private List<Package> receivedPackages;
    
    [SerializeField] private List<PlayerObjectPioComponent> receivedPlayerObjects;
    
    private void Start()
    {
        if (receivedPackages == null)
        {
            receivedPackages = new List<Package>();
        }
        
        if (receivedPlayerObjects == null)
        {
            receivedPlayerObjects = new List<PlayerObjectPioComponent>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // if not interaction layers, ignore
        if ((interactionLayers.value & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }
        
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState.State == GameManager.EGameState.Playing)
        {
            Package package = other.GetComponent<Package>();
            
            if (package != null)
            {
                ReceivePackage(package);
            }
            
            PlayerObjectPioComponent playerObject = other.GetComponentInParent<PlayerObjectPioComponent>();
            
            if (playerObject != null)
            {
                ReceivePlayerObject(playerObject);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // if not interaction layers, ignore
        if ((interactionLayers.value & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }
        
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState.State == GameManager.EGameState.Playing)
        {
            Package package = other.GetComponent<Package>();
            
            if (package != null)
            {
                ReceivePackage(package);
            }
            
            PlayerObjectPioComponent playerObject = other.GetComponentInParent<PlayerObjectPioComponent>();
            
            if (playerObject != null)
            {
                ReceivePlayerObject(playerObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // if not interaction layers, ignore
        if ((interactionLayers.value & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }
        
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState.State == GameManager.EGameState.Playing)
        {
            Package package = other.GetComponent<Package>();
            
            if (package != null)
            {
                RemovePackage(package);
            }
            
            PlayerObjectPioComponent playerObject = other.GetComponentInParent<PlayerObjectPioComponent>();
            
            if (playerObject != null)
            {
                RemovePlayerObject(playerObject);
            }
        }
    }

    private void ReceivePackage(Package package)
    {
        if (GameManager.Instance == null)
        {
            return;
        }
        
        if (!receivedPackages.Contains(package))
        {
            receivedPackages.Add(package);
            
            ManageWinCondition();
        }
    }
    
    private void RemovePackage(Package package)
    {
        if (GameManager.Instance == null)
        {
            return;
        }
        
        if (receivedPackages.Contains(package))
        {
            receivedPackages.Remove(package);
            
            ManageWinCondition();
        }
    }
    
    private void ReceivePlayerObject(PlayerObjectPioComponent playerObject)
    {
        if (GameManager.Instance == null)
        {
            return;
        }
        
        if (!receivedPlayerObjects.Contains(playerObject))
        {
            receivedPlayerObjects.Add(playerObject);
            
            ManageWinCondition();
        }
    }
    
    private void RemovePlayerObject(PlayerObjectPioComponent playerObject)
    {
        if (GameManager.Instance == null)
        {
            return;
        }
        
        if (receivedPlayerObjects.Contains(playerObject))
        {
            receivedPlayerObjects.Remove(playerObject);
            
            ManageWinCondition();
        }
    }
    
    private void ManageWinCondition()
    {
        if (GameManager.Instance == null)
        {
            return;
        }
        
        if (GameManager.Instance.CurrentState.State != GameManager.EGameState.Playing)
        {
            return;
        }

        int targetNumPlayersToWin = 1;
        
        if (ApplicationManager.Instance != null)
        {
            targetNumPlayersToWin = ApplicationManager.Instance.ActiveSceneSettings.PlayerManagerSettings.TargetPlayers;
        }
        
        bool enoughPackagesToWin = receivedPackages.Count >= numPackagesToWin;
        
        bool enoughPlayersToWin = receivedPlayerObjects.Count >= targetNumPlayersToWin;
        
        if (enoughPackagesToWin && enoughPlayersToWin)
        {
            GameManager.Instance.ToggleWinCountdown(true);
        }
        else
        {
            GameManager.Instance.ToggleWinCountdown(false);
        }
    }
}
