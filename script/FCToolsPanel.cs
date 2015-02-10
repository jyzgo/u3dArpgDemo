using System;
using System.Collections;
using UnityEngine;

public class FCToolsPanel : MonoBehaviour
{
    public GameObject toolsContainer;

    public GameObject controllBallArrow;
    public GameObject controllBall;

    public GameObject inventoryButton;
    public GameObject questButton;
    public GameObject battleButton;
    public GameObject offeringButton;
	public GameObject skillButton;
	public GameObject tattooButton;
    public GameObject storeButton;


    private bool _isTweening;

    private bool _isToolsVisible;

    private float _tweenPassTime;

    private const float K_TweenTime = 0.2f;
    private int[] K_angles = new int[]{135, 315};

    void Awake()
    {
        _isToolsVisible = true;
        _isTweening = false;

        UIEventListener.Get(battleButton).onClick = OnClickHUDBattleButton;
        UIEventListener.Get(questButton).onClick = OnClickHUDQuestButton;
        UIEventListener.Get(inventoryButton).onClick = OnClickHUDInventoryButton;
        UIEventListener.Get(offeringButton).onClick = OnClickHUDOfferingButton;
		UIEventListener.Get(controllBall).onClick = OnClickHUDControllBall;
		UIEventListener.Get(skillButton).onClick = OnClickHUDSkillButton;
		UIEventListener.Get(tattooButton).onClick = OnClickHUDTattooButton;
        UIEventListener.Get(storeButton).onClick = OnClickStoreButton;
	}

    void Start()
    {
    }

    void OnClickHUDControllBall(GameObject go)
    {
        if (_isTweening)
            return;
        _isTweening = true;
		_tweenPassTime = 0;
        StartCoroutine(StepToSwitch(!_isToolsVisible, K_TweenTime));
        StartCoroutine(StepToRotateArrow(!_isToolsVisible, K_TweenTime));
    }

    IEnumerator StepToRotateArrow(bool visible, float totalTime)
    {
        while (_tweenPassTime < totalTime)
        {
            float percent = _tweenPassTime / totalTime;
            float angle = 0;
            if(visible)
                angle = iTween.easeOutElastic(0, 180, percent) + K_angles[0];
            else
                angle = -180 * percent + K_angles[1];
            controllBallArrow.transform.eulerAngles = new Vector3(0, 0, angle);
            yield return null;
        }
    }

    IEnumerator StepToSwitch(bool visible , float totalTime)
    {   
        while (_tweenPassTime < totalTime)
        {
            _tweenPassTime += Time.deltaTime;
            float percent = _tweenPassTime / totalTime;
            foreach (Transform ts in toolsContainer.transform)
            {
                FCToolsPanelFunctionButtonBase button = ts.GetComponent<FCToolsPanelFunctionButtonBase>();
                if(null != button)
                    button.StepToMoveAndScale(visible, percent);
            }
            yield return null;
        }
        _isTweening = false;
        _isToolsVisible = visible;
    }

    void OnClickHUDBattleButton(GameObject go = null)
    {
        UIManager.Instance.OpenUI("FCWorldmapLevelSelect", WorldmapRegion.Faluo);
        //UIManager.Instance.OpenUI("FCWorldmapRegionSelect");
        UIManager.Instance.CloseUI("TownHome");
    }

    void OnClickHUDQuestButton(GameObject go)
    {
        UIManager.Instance.OpenUI("UIQuest");
    }

    void OnClickHUDInventoryButton(GameObject go)
    {
		TutorialTriggerTown trigger = NGUITools.FindInParents<TutorialTriggerTown>(this.gameObject);
		if (trigger != null)
		{
			trigger.TryToCloseTutorial();
		}
        UIManager.Instance.OpenUI("FCUIInventory");
    }

	void OnClickHUDOfferingButton(GameObject go)
	{
		UIManager.Instance.OpenUI("FCUIOffering");
	}

	void OnClickHUDSkillButton(GameObject go)
	{
		UIManager.Instance.OpenUI("Skills");
	}

	void OnClickHUDTattooButton(GameObject go)
	{
		UIManager.Instance.OpenUI("Tattoo");
	}

    void OnClickStoreButton(GameObject go)
    {
        UIManager.Instance.OpenUI("Store");
    }
}