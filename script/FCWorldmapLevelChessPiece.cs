using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FCWorldmapLevelChessPiece : MonoBehaviour, IRefreshable
{
    public string levelName;

    public UISprite flag;

    public UISprite shadow;

    public UISprite score;

    public UISprite bossWings;

    public UILabel titleLabel;

//    public UISprite titleBoard;

    public GameObject levelPath;

    private Dictionary<EnumLevelState, string> _levelStateMapping = new Dictionary<EnumLevelState, string>()
    {
        {EnumLevelState.S, "178"},
        {EnumLevelState.A, "179"},
        {EnumLevelState.B, "180"},
        {EnumLevelState.C, "181"},
        {EnumLevelState.D, "182"}
    };

    private float _pathSpendTime = 1.0f;

    private LevelData _levelData;
    public LevelData LevelData
    {
        get{return LevelManager.Singleton.LevelsConfig.GetLevelDataByLevelName(levelName);}
    }

    private List<FCWorldmapLevelPathPoint> _pathPointsList;
    public List<FCWorldmapLevelPathPoint> PathPointsList
    {
        get
        {
            if(null == _pathPointsList)
            {
                _pathPointsList = new List<FCWorldmapLevelPathPoint>();
                foreach (Transform tf in levelPath.transform)
                {
                    FCWorldmapLevelPathPoint pathPoint = tf.gameObject.GetComponent<FCWorldmapLevelPathPoint>();
                    _pathPointsList.Add(pathPoint);
                }
                _pathPointsList.Sort(SortByName);
            }
            return _pathPointsList;
        }
    }

    private EnumLevelState _levelState;
    public EnumLevelState LevelState
    {
        set { _levelState = value; Refresh(); }
        get { return _levelState; }
    }

    private Rect _rect;
    public Rect Rectangle 
    {
        get 
        {
            if (_rect.width == 0 || _rect.height == 0)
            {
                BoxCollider box = GetComponent<BoxCollider>();
                _rect = new Rect(transform.localPosition.x + box.center.x - box.size.x / 2,
                    transform.localPosition.y + box.center.y - box.size.y / 2,
                    box.size.x, box.size.y);
            }
            return _rect;
        }
    }

    private BoxCollider _bc;
    public BoxCollider BC
    {
        get
        {
            if(null == _bc)
            {
                _bc = GetComponent<BoxCollider>();
            }
            return _bc;
        }
    }

    void Start()
    {
        BC.enabled = false;
    }

    public void Refresh()
    {
        if (FCWorldmapLevelSelect.Instance == null)
            return;
        if (_levelState == EnumLevelState.LOCKED)
        {
            gameObject.SetActive(false);
            HidePathPoints();
        }
        else
        {
            gameObject.SetActive(true);
            if (_levelState == EnumLevelState.NEW_UNLOCK)
            {
                HidePathPoints();
                StartCoroutine(OnOpenNewLevel());
            }
            else
            {
                LevelData lastLD = FCWorldmapLevelSelect.Instance.CurrentLevelRoot.LastScoreLevelData;
                if (null == lastLD || lastLD.id != LevelData.id)
                {
                    ShowPathPoints();
                }
                else
                {
                    HidePathPoints();
                    OnOpenPathPointOneByOne();
                }

                //flag
                flag.gameObject.SetActive(true);
                if (_levelState == EnumLevelState.S)
                {
                    flag.spriteName = "174";
                }
                else
                {
                    flag.spriteName = "173";
                }
                //score
                score.gameObject.SetActive(true);
                score.spriteName = _levelStateMapping[FCWorldmapLevelSelect.Instance.CurrentLevelRoot.GetLevelState(LevelData.id)];
                //title
                //titleBoard.gameObject.SetActive(true);
                titleLabel.gameObject.SetActive(true);
                titleLabel.text = Localization.Localize(LevelData.levelNameIDS);
                //boss
                bossWings.gameObject.SetActive(LevelData.isBoss);
                
            }
        }
    }

    void OnOpenPathPointOneByOne()
    {
        StartCoroutine(StepToOpenPathPoint());
    }

    IEnumerator OnOpenNewLevel()
    {
        shadow.gameObject.SetActive(false);
        score.gameObject.SetActive(false);
        flag.gameObject.SetActive(false);
        bossWings.gameObject.SetActive(false);
        //titleBoard.gameObject.SetActive(false);
        titleLabel.gameObject.SetActive(false);
        GameObject fire = FCWorldmapLevelSelect.Instance.attackingEffect;
        fire.SetActive(false);
        if(LevelData.preLevelID != 0)
            yield return new WaitForSeconds(_pathSpendTime + 0.2f);
        fire.SetActive(true);
        fire.transform.parent = transform;
        fire.transform.localPosition = Vector3.zero;
        shadow.gameObject.SetActive(true);
        bossWings.gameObject.SetActive(LevelData.isBoss);
       // titleBoard.gameObject.SetActive(true);
        titleLabel.gameObject.SetActive(true);
        titleLabel.text = Localization.Localize(LevelData.levelNameIDS);
    }

    IEnumerator StepToOpenPathPoint()
    {
        HidePathPoints();
        float perTime = _pathSpendTime / PathPointsList.Count;
        foreach (FCWorldmapLevelPathPoint point in PathPointsList)
        {
            point.Born();
            yield return new WaitForSeconds(perTime);
        }
    }

    void HidePathPoints()
    {
        foreach (FCWorldmapLevelPathPoint point in PathPointsList)
        {
            point.gameObject.SetActive(false);
        }
    }

    void ShowPathPoints()
    {
        foreach (FCWorldmapLevelPathPoint point in PathPointsList)
        {
            point.gameObject.SetActive(true);
        }
    }

    void OnClick()
    {
        string levelName = LevelData.levelName;
        if (LevelManager.Singleton.CheckDownloadAndEnterLevel(levelName, PlayerInfo.Instance.difficultyLevel))
        {
            //UIManager.Instance.CloseUI("FCWorldmapLevelSelect");
            UIManager.Instance.OpenUI("FCEnterLevel", levelName);
        }
    }

    #region sort funcion
    static int SortByName(FCWorldmapLevelPathPoint a, FCWorldmapLevelPathPoint b)
    {
        return string.Compare(a.gameObject.name, b.gameObject.name);
    }
    #endregion
}
