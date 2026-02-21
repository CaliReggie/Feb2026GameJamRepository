using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManagementButton : MonoBehaviour
{
    private enum EPlayerManagementButtonType
    {
        AlternateState
    }
    
    [Header("Inscribed References")]
    
    [Tooltip("The behaviour this button will perform when pressed. MUST have a PlayerInputObject in a parent object.")]
    [SerializeField] private EPlayerManagementButtonType buttonBehaviour = EPlayerManagementButtonType.AlternateState;
    
    
    [Header("Dynamic References - Don't Modify In Inspector")]
    
    [Tooltip("The button component on this GameObject.")]
    [SerializeField] private Button button;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
    }
    
    private void Start()
    {
        button.onClick.AddListener(OnButtonPressed);
    }
    
    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonPressed);
        }
    }
    
    private void OnButtonPressed()
    {
        switch (buttonBehaviour)
        {
            case EPlayerManagementButtonType.AlternateState:
                
                try
                {
                    GetComponentInParent<PlayerInputObject>().TogglePlayerSettingsConfigurationType();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error alternating PlayerInputObject state: {e.Message}.");
                }
                
                break;
        }
    }
}
