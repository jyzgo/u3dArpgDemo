using UnityEngine;

using System;
using System.Collections.Generic;
using FaustComm;
using System.Collections;

public class TownHUD : MonoBehaviour
{
	//indicators
	public GameObject indicatorNewQuest;
	public UILabel indicatorCompletedQuestLabel;
	public GameObject indicatorCompletedQuest;
	public GameObject indicatorNewItems;
	public GameObject indicatorNewSkills;
	public GameObject indicatorNewTattooSlots;
	public GameObject indicatorNewMail;
    public UILabel indicatorUnreadMailCountLabel;

	public UIRewardHint rewardHint;  //outside of prefab

	private bool _started;

	private static TownHUD _instance;
    public static TownHUD Instance 
    {
        get
        {
            if (null != _instance)
                return _instance;
            else
            {
                Debug.LogError("TownHUD has not been initialized yet.");
                return null;
            }
        }
    }

    void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        rewardHint.gameObject.GetComponent<UIAnchor>().enabled = true;
        rewardHint.gameObject.SetActive(false);

		_started = true;
    }

	//called from OpenUI
    void OnInitialize()
    {
		this.UpdateQuestIndicators();
		this.UpdateSkillIndicator();
		this.UpdateTattooSlotIndicator();
		this.UpdateInventoryIndicators();

		StartCoroutine(WaitToStartTutorial());

        StartCoroutine(GetMailsAtIntervals());
    }

	private IEnumerator WaitToStartTutorial()
	{
		while (!_started)
		{
			yield return null;
		}
		TutorialTownManager.Instance.TryToStartTownTutorial(this.gameObject);
	}

    IEnumerator GetMailsAtIntervals()
    {
        while (true)
        {
            this.UpdateMailIndicators();
            yield return new WaitForSeconds(600.0f);
        }
    }

    /// <summary>
    /// SettingButton locate at FCTopBar/Offset/Setting(Button).
    /// </summary>
    public void OnClickSettingButton()
    {
        if (!TutorialTownManager.Instance.isInTutorial)
        {
            UIManager.Instance.OpenUI("UIGameSettings");
        }
    }

    public void TempHide()
    {
        UIManager.Instance.CloseUI("TownHome");
        CameraController.Instance.MainCamera.transform.gameObject.SetActive(false);
    }

    public void ResumeShow()
    {
        UIManager.Instance.OpenUI("TownHome");
		if (CameraController.Instance)
		{
			CameraController.Instance.MainCamera.transform.gameObject.SetActive(true);
		}
    }

	//display the overlay icons: finished quests, new quests
	public void UpdateQuestIndicators()
	{
		int finished = QuestManager.instance.GetCompletedQuestCount();

		int newCount = QuestManager.instance.GetNewQuestCount();

		if (finished > 0)
		{
			indicatorNewQuest.SetActive(false);
			indicatorCompletedQuest.SetActive(true);

			indicatorCompletedQuestLabel.text = finished.ToString();
		}
		else if (newCount > 0)
		{
			indicatorNewQuest.SetActive(true);
			indicatorCompletedQuest.SetActive(false);
		}
		else
		{
			indicatorNewQuest.SetActive(false);
			indicatorCompletedQuest.SetActive(false);
		}
	}

	private void UpdateSkillIndicator()
	{
		int newSkillCount = UISkillsHandler.GetNewSkillCount();

		indicatorNewSkills.SetActive(newSkillCount > 0);
	}

	private void UpdateTattooSlotIndicator()
	{
		int previousPlayerLevel = PlayerPrefs.GetInt(PrefsKey.PreviousPlayerLevel, 0);

		int playerLevel = PlayerInfo.Instance.CurrentLevel;

		int level1 = 0, level2 = 0;

		List<TattooUnlockData> tudList = DataManager.Instance.tattooUnlockDataList.dataList;
		foreach (TattooUnlockData tud in tudList)
		{
			if (tud.playerLevel < playerLevel)
			{
				level1 = tud.playerLevel;
			}
			
			if (tud.playerLevel < previousPlayerLevel)
			{
				level2 = tud.playerLevel;
			}
		}

		if (level1 > level2)	//a new grade is reached
		{
			indicatorNewTattooSlots.SetActive(true);
		}
		else
		{
			indicatorNewTattooSlots.SetActive(false);
		}
	}

    public void UpdateMailIndicators()
    {
        NetworkManager.Instance.MailsList(0, OnMailResponse);
    }

    void OnMailResponse(NetResponse response)
    {
        if (response.Succeeded)
        {
            MailListResponse mResponse = (MailListResponse)response;
            int unReadCount = mResponse.UnReadMailCount();
            indicatorNewMail.gameObject.SetActive(unReadCount > 0);
            indicatorUnreadMailCountLabel.text = unReadCount.ToString();
        }
        else
        { 
            //error
        }
    }

	void UpdateInventoryIndicators()
	{
        bool hasNew = PlayerInfo.Instance.PlayerInventory.HasNewItemInInventory();
        indicatorNewItems.gameObject.SetActive(hasNew);
	}
}
