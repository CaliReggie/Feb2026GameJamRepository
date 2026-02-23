using UnityEngine;

public class Package : MonoBehaviour
{
    public void OnHitHazardZone()
    {
        Destroy(gameObject);
    }
}
