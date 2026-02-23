using System;
using UnityEngine;

public class ColliderAudioManagerSfx : MonoBehaviour
{
    public enum ESfxType
    {
        None,
        BodyImpact,
        PackageImpact
    }
    
    [Header("Inscribed Referfences")]
    
    [SerializeField] private ESfxType sfxType = ESfxType.None;
    
    [Tooltip("The range of velocities that will trigger the SFX. The SFX will be played at full volume at and above the upper end of the range, and will be played at zero below the lower end of the range.")]
    [SerializeField] private Vector2 velocityRange = new (0.1f, 10f);

    private void OnCollisionEnter(Collision other)
    {
        if (sfxType == ESfxType.None || AudioManager.Instance == null)
        {
            return;
        }
        
        float velocity = other.relativeVelocity.magnitude;

        if (velocity < velocityRange.x)
        {
            return;
        }

        float volume = Mathf.InverseLerp(velocityRange.x, velocityRange.y, velocity);
        
        Vector3 collisionPoint = other.GetContact(0).point;

        switch (sfxType)
        {
            case ESfxType.BodyImpact:
                AudioManager.Instance.PlaySfxOneShot(AudioManager.Instance.BodyImpactSfx, volume, collisionPoint);
                break;
            case ESfxType.PackageImpact:
                AudioManager.Instance.PlaySfxOneShot(AudioManager.Instance.PackageImpactSfx, volume, collisionPoint);
                break;
        }
    }
}
