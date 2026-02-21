using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

public class DeliveryZone : MonoBehaviour
{
    [Header("Dynamic Settings")]
    
    [SerializeField] private List<Package> deliveredPackages;
    
    private void Start()
    {
        if (deliveredPackages == null)
        {
            deliveredPackages = new List<Package>();
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
                if (deliveredPackages.Contains(package))
                {
                    deliveredPackages.Remove(package);
                }
            }
        }
    }

    private void ReceivePackage(Package package)
    {
        if (GameManager.Instance == null)
        {
            return;
        }
        
        if (!deliveredPackages.Contains(package))
        {
            deliveredPackages.Add(package);
            
            if (deliveredPackages.Count >= GameManager.Instance.NumNumPackagesToWin)
            {
                GameManager.Instance.GameOver(true);
            }
        }
    }
}
