using System;
using UnityEngine;

public class HazardZone : MonoBehaviour
{
    [Header("Inscribed Settings")]
    
    [SerializeField] private LayerMask interactionLayers;
    
    private void OnTriggerEnter(Collider other)
    {
        // if not in interaction layers, ignore
        if ((interactionLayers.value & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }
        
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState.State == GameManager.EGameState.Playing)
        {
            Package package = other.GetComponent<Package>();
            
            PlayerInputObject playerInputObject = other.GetComponentInParent<PlayerInputObject>();
            
            if (package != null)
            {
                package.OnHitHazardZone();
                
                GameManager.Instance.GameOver(false);
            }
            else if (playerInputObject != null)
            {
                playerInputObject.GetComponent<PlayerObjectPioComponent>().OnHitHazardZone();
                
                GameManager.Instance.GameOver(false);
            }
        }
    }
}
