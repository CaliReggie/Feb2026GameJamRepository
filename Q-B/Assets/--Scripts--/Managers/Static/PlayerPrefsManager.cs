using UnityEngine;

public static class PlayerPrefsManager
{
    const string ScoreKeyPrefix = "LevelBestScore_";
    
    static string GetScoreKey(int levelIndex) => ScoreKeyPrefix + levelIndex;
    
    public static int GetSceneBestScore(int chronologicalId)
    {
        return PlayerPrefs.GetInt(GetScoreKey(chronologicalId), -1);
    }
    
    public static void SetSceneBestScore(int chronologicalId, int score)
    {
        PlayerPrefs.SetInt(GetScoreKey(chronologicalId), score);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// A scene is unlocked if there is a score >= 0 for the chronologicalId of the scene one before it.
    /// Scene 0 and 1 are always unlocked (main menu and first playable scene),
    /// 2 is unlocked if 1 has a score >= 0, etc.
    /// </summary>
    public static bool IsSceneUnlocked(int chronologicalId)
    {
        if (chronologicalId == 0 || chronologicalId == 1)
        {
            return true; // First two scenes are always unlocked
        }
        
        // adding something else for co op, where i'm starting those at 201
        
        if (chronologicalId == 201)
        {
            return true; // First co op scene is always unlocked
        }
        
        int previousSceneScore = GetSceneBestScore(chronologicalId - 1);
        
        return previousSceneScore >= 0;
    }
}

