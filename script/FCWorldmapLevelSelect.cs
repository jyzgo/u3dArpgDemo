using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FCWorldmapLevelSelect : MonoBehaviour
{
	public UIFont textFont;

	public GameObject container;

	public GameObject closeButton;

	public GameObject LevelFaluo;

	public GameObject attackingEffect;

	public FCWorldmapLevelRoot CurrentLevelRoot
	{
		get
		{
			return LevelFaluo.GetComponent<FCWorldmapLevelRoot>();
		}
	}

	void Awake()
	{
		_instance = this;
        attackingEffect.SetActive(false);
		UIEventListener.Get(closeButton).onClick = OnClickCloseButton;
	}

	void Start()
	{
	}

#if false
	void OnGUI()
	{
		if (GUI.Button(new Rect(Screen.width - 200, 0, 200, 50),
			new GUIContent("refresh"))
			)
		{
			LevelFaluo.GetComponent<FCWorldmapLevelRoot>().Refresh();
		}
		if (GUI.Button(new Rect(Screen.width - 200, 50, 200, 50),
			new GUIContent("focus"))
		)
		{
            Vector3 focus = Vector3.zero;
            if (null != CurrentLevelRoot.AttackingLevelData)
            {
                focus = CurrentLevelRoot.GetLocationByLevel(
                    CurrentLevelRoot.AttackingLevelData.levelName);
                LevelFaluo.GetComponent<BoundLimitedDrag>().FocusOn(focus);
            }
            else
            {
                focus = CurrentLevelRoot.GetLocationByLevel(LevelManager.Singleton.LastScoreLevelData.levelName);
                LevelFaluo.GetComponent<BoundLimitedDrag>().FocusOn(focus);
            }
		}
	}
#endif

	void OnInitializeWithCaller(WorldmapRegion region)
	{
		if (WorldmapRegion.Faluo == region)
		{
			LevelFaluo.transform.localPosition = Vector3.zero;
		}
        Vector3 focus;
        if (null != CurrentLevelRoot.AttackingLevelData)
        {
            focus = CurrentLevelRoot.GetLocationByLevel(
                CurrentLevelRoot.AttackingLevelData.levelName);
            LevelFaluo.GetComponent<BoundLimitedDrag>().SetPosition(focus);
        }
        else
        {
            focus = CurrentLevelRoot.GetLocationByLevel(LevelManager.Singleton.LastScoreLevelData.levelName);
            LevelFaluo.GetComponent<BoundLimitedDrag>().FocusOn(focus);
        }

        TownHUD.Instance.TempHide();
	}

    void OnDisable()
    {
        TownHUD.Instance.ResumeShow();
    }

	void OnClickCloseButton(GameObject gameobject)
	{
		UIManager.Instance.CloseUI("FCWorldmapLevelSelect");
		//UIManager.Instance.OpenUI("FCWorldmapRegionSelect");
	}

	#region singleton
	private static FCWorldmapLevelSelect _instance;
	public static FCWorldmapLevelSelect Instance
	{
		get { return _instance; }
	}
	#endregion
}
