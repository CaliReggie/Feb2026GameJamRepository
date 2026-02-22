using UnityEngine;

public class GameOverCaller : MonoBehaviour
{
    public void CallGameWon()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver(true);
        }
    }
    
    public void CallGameLost()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver(false);
        }
    }
    
    public void PlayCountDownPart1()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfxOneShot(AudioManager.Instance.WinSfxP1, .5f, transform.position);
        }
    }
    
    public void PlayCountDownPart2()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfxOneShot(AudioManager.Instance.WinSfxP2, .6f, transform.position);
        }
    }
    
    public void PlayCountDownPart3()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfxOneShot(AudioManager.Instance.WinSfxP3, .75f, transform.position);
        }
    }
    
    
    public void PlayCountDownPart4()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfxOneShot(AudioManager.Instance.WinSfxP4, .9f, transform.position);
        }
    }
}
