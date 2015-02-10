using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class ChestInfo
{
	public string _id;
	public string _transformName;
	public int _count;
	public string _prefabPath;
}

/// <summary>
/// Level config.
/// </summary>
[System.Serializable]
public class LevelData 
{
	public int id;
	public string levelName; // Scene name of the level.
	public string sceneName;
	public string levelNameIDS; // The name of the level.
	public string levelDescIDS; // Description of the level.
	public bool available = true;
    public bool isBoss;
	public string levelLoadingBGTexture; //texture while loading
	public int unlockPlayerLevel = 1;
	public int suggestedPlayerLevel = 1;
	public int preLevelID = 0;

	public string worldName;	//for checking if the bundlle pack has been downloaded
	public string lootList;		//a ";" delimetered string

	public int vitalityCost;
	
	public CameraController.CameraMode cameraMode = CameraController.CameraMode.Standard;
	
	public int minTime; // Second
	public int maxTime; // Second
}


/// <summary>
/// Levels config.
/// </summary>
public class LevelConfig : ScriptableObject
{
    // All levels of our game.
    public LevelData[] levels;

    public string GetLevelLoadingBGTexture(string levelName)
    {
        foreach (LevelData level in levels)
        {
            if (levelName == level.levelName)
            {
                return level.levelLoadingBGTexture;
            }
        }

        Assertion.Check(false);
        return null;
    }

    public LevelData GetLevelData(string levelName)
    {
        foreach (LevelData level in levels)
        {
            if (levelName == level.levelName)
            {
                return level;
            }
        }

        Debug.LogError("[LevelConfig] Level not found in config: " + levelName);
        Assertion.Check(false);
        return null;
    }

	public LevelData GetLevelDataByID(int levelID)
	{
		foreach (LevelData level in levels)
		{
			if (levelID == level.id)
			{
				return level;
			}
		}
		
        //Debug.LogError("[LevelConfig] Level ID not found in config: " + levelID);
        //Assertion.Check(false);
		return null;
	}

    public LevelData GetLevelDataByLevelName(string lvName)
    {
        foreach (LevelData ld in levels)
        {
            if (lvName == ld.levelName)
            {
                return ld;
            }
        }

        Debug.LogError("[LevelConfig] Level name not found in config: " + lvName);
        Assertion.Check(false);
        return null;
    }


    public LevelData MaxLevelData
    {
        get
        {
            LevelData maxLevelData = null;
            foreach (LevelData ld in levels)
            {
                if (null == maxLevelData) maxLevelData = ld;
                maxLevelData = ld.id > maxLevelData.id ? ld : maxLevelData;
            }
            return maxLevelData;
        }
    }
}