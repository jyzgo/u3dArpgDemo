using System;
using System.Collections.Generic;
using UnityEngine;

public class FCWorldmapLevelRoot : MonoBehaviour, IRefreshable
{
    public bool useDebugData;

    public FCWorldmapLevelChessPiece[] levels;

    public EnumLevelState[] levelScores;

    public GameObject container;

    void Start()
    {
        foreach (FCWorldmapLevelChessPiece chesspiece in levels)
        {
            chesspiece.LevelState = EnumLevelState.LOCKED;
        }
        Refresh();
    }

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        foreach (FCWorldmapLevelChessPiece chesspiece in levels)
        {
            EnumLevelState levelState = GetLevelState(chesspiece.levelName);
            chesspiece.LevelState = levelState;
        }
    }

    public Vector3 GetLocationByLevel(string levelName)
    {
        foreach (FCWorldmapLevelChessPiece chesspiece in levels)
        {
            if (chesspiece.LevelData.levelName == levelName)
                return chesspiece.transform.localPosition;
        }
        return Vector3.zero;
    }

    void OnClickWithoutDragging()
    {
		Vector3 vec = UICamera.currentCamera.ScreenToWorldPoint(
			new Vector3( UICamera.lastTouchPosition.x, UICamera.lastTouchPosition.y, UICamera.currentCamera.nearClipPlane ) );
		Vector3 vec2 = container.transform.InverseTransformPoint( vec );
		//Vector3 vec2 = container.transform.worldToLocalMatrix.MultiplyPoint( vec );

        foreach (FCWorldmapLevelChessPiece chesspiece in levels)
        {
			Rect rect = chesspiece.Rectangle;
            if (chesspiece.gameObject.activeInHierarchy && rect.Contains(new Vector2(vec2.x, vec2.y)))
            {
                UIManager.Instance.OpenUI("FCEnterLevel", chesspiece.LevelData.levelName);
				break;
            }
        }
    }

    #region level states

    public EnumLevelState GetLevelState(int levelId)
    {
        if (!useDebugData)
        {
            return PlayerInfo.Instance.GetLevelState(levelId);
        }
        else
        {
            LevelData lvData = LevelManager.Singleton.LevelsConfig.GetLevelDataByID(levelId);
            return GetLevelState(lvData.levelName);
        }
     }

    public EnumLevelState GetLevelState(string level)
    {
        if (!useDebugData)
        {
            return PlayerInfo.Instance.GetLevelState(level, PlayerInfo.Instance.difficultyLevel);
        }
        else
        {
            for (int i = 0, count = levels.Length; i < count; i++)
            {
                FCWorldmapLevelChessPiece chesspiece = levels[i];
                if (chesspiece.LevelData.levelName == level)
                {
                    if (levelScores.Length == i)
                        return EnumLevelState.NEW_UNLOCK;
                    else if (levelScores.Length < i)
                        return EnumLevelState.LOCKED;
                    else
                        return levelScores[i];
                }
            }
            return EnumLevelState.LOCKED;
        }
    }

    public LevelData LastScoreLevelData
    {
        get
        {
            if (!useDebugData)
            {
                return LevelManager.Singleton.LastScoreLevelData;
            }
            else
            {
                FCWorldmapLevelChessPiece lastLevel = null;
                for (int i = levels.Length - 1; i >= 0; i--)
                {
                    FCWorldmapLevelChessPiece chesspiece = levels[i];
                    if (levelScores.Length > i &&
                        levelScores[i] != EnumLevelState.LOCKED &&
                        levelScores[i] != EnumLevelState.NEW_UNLOCK
                        )
                    {
                        return chesspiece.LevelData;
                    }
                    lastLevel = chesspiece;
                }
                return lastLevel.LevelData;
            }
        }
    }

    public LevelData AttackingLevelData
    {
        get
        {
            if (!useDebugData)
            {
                return LevelManager.Singleton.NewUnlockLevelData;
            }
            else
            {
                for (int i = 0, count = levels.Length; i < count; i++)
                {
                    FCWorldmapLevelChessPiece chesspiece = levels[i];
                    if (chesspiece.LevelData.preLevelID == LastScoreLevelData.id)
                    {
                        return chesspiece.LevelData;
                    }
                }
                return null;
            }
        }
    }

    #endregion
}
