using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AudioManagerButtonSfx : MonoBehaviour
{
    [Header("Dynamic References - Don't Modify In Inspector")]
    
    [SerializeField] private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }
    
    private void OnEnable()
    {
        button.onClick.AddListener(PlayButtonClickSfx);
    }
    
    private void OnDisable()
    {
        button.onClick.RemoveListener(PlayButtonClickSfx);
    }
    
    private void PlayButtonClickSfx()
    {
        AudioManager.Instance.PlaySfxOneShot(AudioManager.Instance.ButtonClickSfx);
    }
}
