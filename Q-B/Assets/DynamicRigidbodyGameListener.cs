using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DynamicRigidbodyGameListener : GameManagerListener
{
    
    [Header("Dynamic References - Don't Modify In Inspector")]
    
    [SerializeField] private Rigidbody rb;
    
    [SerializeField] private Vector3 initialPosition;
    
    [SerializeField] private Quaternion initialRotation;
    
    [SerializeField] private bool initiallyKinematic;

    protected override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        
        initialPosition = transform.position;
        
        initialRotation = transform.rotation;
        
        initiallyKinematic = rb.isKinematic;
        
        initialized = true;
    }

    protected override void OnBeforeGameStateChanged(GameManager.EGameState fromState)
    {
        // do nothing
    }

    protected override void OnAfterGameStateChanged(GameManager.EGameState toState)
    {
        switch (toState)
        {
            case GameManager.EGameState.Initialize:
                SetRbKinematic();
                break;
        }
        
        if (GameManager.Instance.PreviousState != null)
        {
            if (GameManager.Instance.PreviousState.State == GameManager.EGameState.Initialize &&
                toState == GameManager.EGameState.Playing)
            {
                SetRbDynamic();
            }
        }
    }

    private void SetRbKinematic()
    {
        if (initiallyKinematic) {return;}
        
        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        if (gameObject.activeInHierarchy)
        {
            rb.Move(initialPosition, initialRotation);
        }
        else
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }
        
        rb.isKinematic = true;
    }
    
    private void SetRbDynamic()
    {
        if (initiallyKinematic) {return;}
        
        rb.isKinematic = false;
    }
}
