using System;
using UnityEngine;

public class HazardZone : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState.State == GameManager.EGameState.Playing)
        {
            Package package = other.collider.GetComponent<Package>();
            
            PlayerInputObject playerInputObject = other.collider.GetComponentInParent<PlayerInputObject>();
            
            if (package != null)
            {
                // package.OnHitHazardZone();
                
                GameManager.Instance.GameOver(false);
            }
            else if (playerInputObject != null)
            {
                GameManager.Instance.GameOver(false);
            }
        }
    }
}
