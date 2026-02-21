using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

public class DeliveryZone : MonoBehaviour
{
    [FormerlySerializedAs("deliveredPackages")]
    [Header("Dynamic Settings")]
    
    [SerializeField] private List<Package> receivedPackages;
    
    private void Start()
    {
        if (receivedPackages == null)
        {
            receivedPackages = new List<Package>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState.State == GameManager.EGameState.Playing)
        {
            Package package = other.GetComponent<Package>();
            
            if (package != null)
            {
                ReceivePackage(package);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState.State == GameManager.EGameState.Playing)
        {
            Package package = other.GetComponent<Package>();
            
            if (package != null)
            {
                ReceivePackage(package);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState.State == GameManager.EGameState.Playing)
        {
            Package package = other.GetComponent<Package>();
            
            if (package != null)
            {
                RemovePackage(package);
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
            
            if (receivedPackages.Count >= GameManager.Instance.NumNumPackagesToWin)
            {
                GameManager.Instance.ToggleWinCountdown(true);
            }
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
            
            if (receivedPackages.Count <= 0 || receivedPackages.Count < GameManager.Instance.NumNumPackagesToWin)
            {
                GameManager.Instance.ToggleWinCountdown(false);
            }
        }
    }
}
