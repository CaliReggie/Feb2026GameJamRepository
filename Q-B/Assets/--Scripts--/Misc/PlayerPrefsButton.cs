using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PlayerPrefsButton : MonoBehaviour
{
    private enum EPlayerPrefsButtonType
    {
        Unassigned,
        UnlockLevels,
        LockLevels,
    }

    [Header("Inscribed References")]

    [Tooltip("The behaviour of this button when pressed.")]
    [SerializeField] private EPlayerPrefsButtonType buttonBehaviour = EPlayerPrefsButtonType.Unassigned;
    
    [Header("Inscribed Settings")]
    
    [SerializeField] private int lowestChronologicalIdToUnlock = 0;
    [SerializeField] private int highestChronologicalIdToUnlock = 999;
    
    [Space]
    
    [SerializeField] private int lowestChronologicalIdToLock = 0;
    [SerializeField] private int highestChronologicalIdToLock = 999;
    
    [Header("Dynamic References - Don't Modify In Inspector")]
    
    [Tooltip("The Button component on this GameObject.")]
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
            case EPlayerPrefsButtonType.UnlockLevels:
                for (int i = lowestChronologicalIdToUnlock; i <= highestChronologicalIdToUnlock; i++)
                {
                    PlayerPrefsManager.SetSceneBestScore(i, 0);
                }
                break;
            
            case EPlayerPrefsButtonType.LockLevels:
                for (int i = lowestChronologicalIdToLock; i <= highestChronologicalIdToLock; i++)
                {
                    PlayerPrefsManager.SetSceneBestScore(i, -1);
                }
                break;
        }
    }
}