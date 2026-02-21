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
}
