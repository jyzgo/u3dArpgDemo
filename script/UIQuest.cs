using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIQuest : MonoBehaviour
{
    public GameObject questItemPrefab;

    public GameObject _scrollView;

    public GameObject mainPanel;

    #region Tutorial
    public GameObject tutorialColliderMask;
    public GameObject[] tutorialTips;
    public GameObject arrowClaimFight;
    public GameObject arrowBack;
    #endregion

    //Grid
    private UIGrid _grid;

    public GameObject gridGO;

    private float _scrollViewX; //due to scroll view bug, this X could be changed. We need to remember its original value.

    private List<UIQuestItem> _itemList = new List<UIQuestItem>();

    public List<UIQuestItem> ItemList { get { return _itemList; } }

    //============= Of Current Quest Item ==================
    public UILabel nameLabel;

    public UIImageButton buttonFight;

    public UILabel buttonLabel;  //fight or claim reward

    public UILabel labelDescription;

    //rewards
    public UIRewardItem[] rewards1;     //hc sc exp
    public UICommonDisplayItem[] rewards2;     //items

    //from atlas common_ui
    public const string k_hc_sprite_name = "12";
    public const string k_sc_sprite_name = "13";
    public const string k_exp_sprite_name = "224";       //icon missing

    [Serializable]
    public class UIRewardItem
    {
        public GameObject root;
        public UILabel amount;
        public UISprite iconBG;
        public UITexture icon;
    }
    public UISprite rewardIconHC;
    public UISprite rewardIconSC;
    public UISprite rewardIconExp;

    public UILabel rewardLabelHC;
    public UILabel rewardLabelSC;
    public UILabel rewardLabelExp;

    public GameObject claimEffectRoot;
    //end rewards

    //target
    [Serializable]
    public class TmpUIQuestTarget
    {
        public GameObject targetRoot;
        public UILabel targetLabel;
        public GameObject tick;
        public GameObject circle;
    }

    public TmpUIQuestTarget[] targets;

    public UILabel labelQuestID;     //for debug
    //end target

    private UIQuestItem _selectedItem;
    //=============end current quest item===========

    public Color completeColor = new Color(0, 1, 0);

    public Color uncompleteColor = new Color(1, 0, 1);

    void Awake()
    {
        _grid = gridGO.GetComponent<UIGrid>();

        _scrollViewX = _scrollView.transform.localPosition.x;

        //_uiTownHome = transform.parent.FindChild("TownHome").GetComponent<UITownHome>();
    }

    public void DisplayQuest(QuestProgress qp)
    {
        QuestData qd = QuestManager.instance.CurrentQuestList.FindQuestDataByID(qp.quest_id);

        //fill UI fields
        nameLabel.text = Localization.instance.Get(qd.quest_name);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        labelQuestID.text = "Quest id: " + qd.quest_id.ToString();
#endif
        labelDescription.text = Localization.instance.Get(qd.description);

        EnumRole role = PlayerInfo.Instance.Role;

        //rewards

        //hc, sc, exp. Do not show 0 values
        int activeItemCount = 0;
        if (qd.reward_exp > 0) activeItemCount++;
        if (qd.reward_sc > 0) activeItemCount++;
        if (qd.reward_hc > 0) activeItemCount++;

        //left aligned
        switch (activeItemCount)
        {
            case 0: //hide all
                foreach (UIRewardItem item in rewards1)
                {
                    item.root.SetActive(false);
                }
                break;

            case 1: //show 1
                rewards1[0].root.SetActive(true);
                rewards1[1].root.SetActive(false);
                rewards1[2].root.SetActive(false);

                if (qd.reward_exp > 0)
                {
                    rewards1[0].iconBG.spriteName = k_exp_sprite_name;
                    rewards1[0].amount.text = "x" + qd.reward_exp.ToString();
                }
                else if (qd.reward_sc > 0)
                {
                    rewards1[0].iconBG.spriteName = k_sc_sprite_name;
                    rewards1[0].amount.text = "x" + qd.reward_sc.ToString();
                }
                else if (qd.reward_hc > 0)
                {
                    rewards1[0].iconBG.spriteName = k_hc_sprite_name;
                    rewards1[0].amount.text = "x" + qd.reward_hc.ToString();
                }
                break;

            case 2: //show 2
                rewards1[0].root.SetActive(true);
                rewards1[1].root.SetActive(true);
                rewards1[2].root.SetActive(false);

                if (qd.reward_hc <= 0)
                {
                    //show exp, sc
                    rewards1[0].iconBG.spriteName = k_exp_sprite_name;
                    rewards1[0].amount.text = "x" + qd.reward_exp.ToString();

                    rewards1[1].iconBG.spriteName = k_sc_sprite_name;
                    rewards1[1].amount.text = "x" + qd.reward_sc.ToString();
                }
                if (qd.reward_sc <= 0)
                {
                    //show exp, hc
                    rewards1[0].iconBG.spriteName = k_exp_sprite_name;
                    rewards1[0].amount.text = "x" + qd.reward_exp.ToString();

                    rewards1[1].iconBG.spriteName = k_hc_sprite_name;
                    rewards1[1].amount.text = "x" + qd.reward_hc.ToString();
                }
                if (qd.reward_exp <= 0)
                {
                    //show hc and sc
                    rewards1[0].iconBG.spriteName = k_sc_sprite_name;
                    rewards1[0].amount.text = "x" + qd.reward_sc.ToString();

                    rewards1[1].iconBG.spriteName = k_hc_sprite_name;
                    rewards1[1].amount.text = "x" + qd.reward_hc.ToString();
                }
                break;

            case 3: //show all 3
                foreach (UIRewardItem item in rewards1)
                {
                    item.root.SetActive(true);
                }
                rewards1[0].iconBG.spriteName = k_exp_sprite_name;
                rewards1[0].amount.text = "x" + qd.reward_exp.ToString();

                rewards1[1].iconBG.spriteName = k_sc_sprite_name;
                rewards1[1].amount.text = "x" + qd.reward_sc.ToString();

                rewards1[2].iconBG.spriteName = k_hc_sprite_name;
                rewards1[2].amount.text = "x" + qd.reward_hc.ToString();
                break;
        }

        //reward items
        int index = 0;
        foreach (QuestRewardItem rewardItem in qd.reward_item_list)
        {
            if (rewardItem.reward_role == role || rewardItem.reward_role < 0)
            {
                rewards2[index].gameObject.SetActive(true);

				rewards2[index].SetData(rewardItem.reward_item_id, rewardItem.reward_item_count);

                index++;
            }
        }

        while (index < 4)
        {
            rewards2[index].gameObject.SetActive(false);
            index++;
        }

        //targets
        index = 0;
        for (int i = 0; i < qp.target_progress_list.Count; i++)
        {
            QuestTargetProgress qtp = qp.target_progress_list[i];

            targets[i].targetRoot.SetActive(true);

            targets[i].targetLabel.text = GetQuestTargetString(qtp, qd);

            if (qtp.actual_amount == qtp.required_amount)
            {
				targets[i].tick.SetActive(true);
				targets[i].circle.SetActive(false);
				//change color to green
                targets[i].targetLabel.color = completeColor;
            }
            else
            {
				targets[i].tick.SetActive(false);
				targets[i].circle.SetActive(true);
				targets[i].targetLabel.color = uncompleteColor;
            }
        }

        //hide other targets
        for (int i = qp.target_progress_list.Count; i < 2; i++)
        {
            targets[i].targetRoot.SetActive(false);
        }

        if (qp.isCompleted)
        {
            buttonLabel.text = Localization.instance.Get("IDS_MESSAGE_QUEST_ACCEPTREWARD");
            buttonFight.gameObject.SetActive(true);
        }
        else// if (!qp.isCompleted)
        {
			buttonLabel.text = Localization.instance.Get("IDS_BUTTON_HUD_COMBAT");
            buttonFight.gameObject.SetActive(!string.IsNullOrEmpty(qd.recommended_level_name));
        }

        //claim effect
        claimEffectRoot.SetActive(false);
    }

    private string GetQuestTargetString(QuestTargetProgress qtp, QuestData qd)
    {
        string resultStr = string.Empty;

        string typeName = Localization.instance.Get(QuestTarget.typeNames[(int)qtp.target_type]);

        string levelName;

		switch (qtp.target_type)
		{
			case QuestTargetType.complete_level:
				levelName = Localization.instance.Get(LevelManager.Singleton.LevelsConfig.GetLevelData(qtp.target_id).levelNameIDS);
				if (qtp.required_amount == qtp.actual_amount) //target achieved, use same color for level
				{
					levelName = "[-]" + levelName;
				}
				resultStr = string.Format(typeName, levelName);
				break;

			case QuestTargetType.kill_monster:
				AcData ad = DataManager.Instance.CurAcDataList.Find(qtp.target_id);
				string monsterName = ad == null ? "Error" : Localization.instance.Get(ad.nameIds);

				levelName = Localization.instance.Get(LevelManager.Singleton.LevelsConfig.GetLevelData(qd.recommended_level_name).levelNameIDS);
				if (qtp.required_amount == qtp.actual_amount) //target achieved, use same color for level
				{
					levelName = "[-]" + levelName;
				}

				resultStr = string.Format(typeName, levelName, monsterName);
				break;

			case QuestTargetType.find_loot:
			case QuestTargetType.purchase_item:
				string itemName = Localization.instance.Get(DataManager.Instance.GetItemData(qtp.target_id).nameIds);
				resultStr = string.Format(typeName, itemName, qtp.required_amount);
				break;

			case QuestTargetType.level_up:
				resultStr = string.Format(typeName, qtp.required_amount);
				break;

			case QuestTargetType.fusion_up:
				int targetLevel = -1;       //-1: up to any level,  1~3: reach level 1~5

				Int32.TryParse(qtp.target_id, out targetLevel);

				if (targetLevel < 0)
				{
					resultStr = string.Format(typeName, qtp.required_amount);
				}
				else
				{
					typeName = Localization.instance.Get("IDS_MESSAGE_QUEST_TARGET_FUSIONTIME");

					resultStr = string.Format(typeName, targetLevel, qtp.required_amount); //upgrade 10 items to level X
				}
				break;
		}

        resultStr += "  " + qtp.actual_amount.ToString() + "/" + qtp.required_amount;
        return resultStr;
    }

    private void OnButtonFightClick()
    {
        StopCoroutine("ShowCompleteEffect");
		
        if (_selectedItem.questProgress.isCompleted) //claim
        {
            BeginStep2();

            buttonFight.isEnabled = false;

            StartCoroutine(ClientClaimReward());
        }
        else  //fight
        {
			TryFinishTutorial_41();

            UIManager.Instance.OpenUI("TownHome");		
			///////////////////////////////////////////////////////////////////////////////
			
			bool allDownloaded = LevelManager.Singleton.CheckDownloadByLevel(_selectedItem.questData.recommended_level_name);

	        // All bundles are in local device.
			if (allDownloaded)
			{
				//start the desired level
				this.gameObject.SetActive(false);

				WorldMapController.LevelName = _selectedItem.questData.recommended_level_name;

				//WorldMapController.DifficultyLevel = _selectedItem.questData.recommended_level_difficulty;

				UIManager.Instance.OpenUI("FCEnterLevel", WorldMapController.LevelName);
			}
			else
			{
				FCDownloadManagerView.Instance.ShowDownloadProgress(true);
			}
		}
    }

    //called automatically when openUI
    public void OnInitialize()
    {
		UIManager.Instance.CloseUI("TownHome");

		CameraController.Instance.MainCamera.gameObject.SetActive(false);

        TryStartTutorial();

        claimEffectRoot.SetActive(false);

        StartCoroutine(RefreshItems(false));
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
	//for cheatManager only
    public void CheatRefreshItems()
    {
		StartCoroutine(CheatRefreshItems2());
    }

	private IEnumerator CheatRefreshItems2()
	{
		yield return new WaitForSeconds(0.5f);

		yield return StartCoroutine(RefreshItems(true));
	}
#endif

    private IEnumerator RefreshItems(bool forceUpdate)
    {
        if (_itemList.Count == 0)
        {
            mainPanel.SetActive(false); //nothing to show
        }

        buttonFight.isEnabled = false;

        const float k_timeout = 5;

        float elapsedTime = 0;
        
        if (!QuestManager.instance.HasSyncedWithServer || forceUpdate)
        {
            QuestManager.instance.CheckToActivateQuests();

            while (!QuestManager.instance.HasSyncedWithServer && elapsedTime < k_timeout)
            {
                elapsedTime += Time.deltaTime;

                yield return null;
            }
        }

        if (elapsedTime < k_timeout)
        {
            ClearItems();

            foreach (QuestProgress qp in QuestManager.instance.UserQuestList)
            {
                AddQuestItem(qp, this);
            }
        }
        else
        {
            Debug.LogError("[UI Quest] Failed to retrieve latest quests.");
        }

        if (_itemList.Count > 0)
        {
            this.SetFocusItem(_itemList[0]);

            mainPanel.SetActive(true);
        }
        else
        {
            mainPanel.SetActive(false); //nothing to show
        }

        yield return null;

        _grid.Reposition();

        _scrollView.transform.localPosition = new Vector3(_scrollViewX, _scrollView.transform.localPosition.y, _scrollView.transform.localPosition.z);

        buttonFight.isEnabled = true;
    }

    private void ClearItems()
    {
        List<GameObject> goList = new List<GameObject>();

        foreach (Transform t in _grid.transform)
        {
            goList.Add(t.gameObject);
        }

        foreach (GameObject go in goList)
        {
            Destroy(go);
        }

        _itemList.Clear();

        _selectedItem = null;
    }

    private void AddQuestItem(QuestProgress qp, UIQuest uiQuest)
    {
        GameObject go = GameObject.Instantiate(questItemPrefab) as GameObject;

        go.transform.parent = gridGO.transform;

        go.transform.localPosition = Vector3.zero;

        go.transform.localScale = Vector3.one;

        UIQuestItem item = go.GetComponent<UIQuestItem>();

        item.Init(qp, uiQuest);

        _itemList.Add(item);
    }

	void OnClickCloseButton()
    {
        TryFinishTutorial_40();

        this.gameObject.SetActive(false);
		
		TutorialTownManager.Instance.CheckIfStartTutorial();
		
        //close the level up effect any way
        Transform t = transform.parent.FindChild("LevelupUINotification");

        if (t != null)
		{
			t.gameObject.SetActive(false);
		}

        //update town home UI
		UIManager.Instance.OpenUI("TownHome");

		CameraController.Instance.MainCamera.gameObject.SetActive(true);
    }

    public void RepositionItems(UIQuestItem changeItem)
    {
        _grid.Reposition(); //do it right now
    }

	private const float k_claim_effect_full_size = 1;

    private const float k_claim_effect_appear_time = 0.25f;

    private const float k_claim_effect_disappear_time = 0.2f;

    private IEnumerator ClientClaimReward()
    {
        int oldLevel = PlayerInfo.Instance.CurrentLevel;
		
		int questid = _selectedItem.questData.quest_id;
		
		GameManager.Instance.CommStatus = FCCommStatus.Busy;

		NetworkManager.Instance.SendCommand(new ClaimQuestRewardRequest(_selectedItem.questData.quest_id), OnServerClaimReward);
		
		//wait until result is returned
		while (GameManager.Instance.CommStatus == FCCommStatus.Busy)
        {
            yield return null;
        }

		if (GameManager.Instance.CommStatus == FCCommStatus.ResultError)
        {
			GameManager.Instance.CommStatus = FCCommStatus.Idle;

            buttonFight.isEnabled = true;

            //refresh quest list when there is a network error
            yield return StartCoroutine(RefreshItems(true));

            yield break;
        }
		
        //ok now
		GameManager.Instance.CommStatus = FCCommStatus.Idle;

        yield return StartCoroutine(ShowCompleteEffect());

        yield return StartCoroutine(RefreshItems(true));

        int newLevel = PlayerInfo.Instance.CurrentLevel;

        if (newLevel > oldLevel)
        {
            Transform t = transform.parent.FindChild("LevelupUINotification");

            if (t != null)
            {
                t.gameObject.SetActive(true);

                yield return new WaitForSeconds(1.5f);

                t.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("Level up effect not found!");
            }
        }
		//magic num of quest id from GD
		if(questid == 2006)
		{
			GameManager.Instance.ShowRateApp(GameManager.RateAppPoint.AT_FINIST_QUEST);
		}
        BeginStep3();
    }

	private void OnServerClaimReward(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			GameManager.Instance.CommStatus = FCCommStatus.ResultOK;

			List<int> questIDList = (msg as ClaimQuestRewardResponse).questIDList;

			QuestManager.instance.ApplyActiveQuestIdList(questIDList);

			(msg as ClaimQuestRewardResponse).updateData.Broadcast();
		}
		else
		{
			UIMessageBoxManager.Instance.ShowMessageBox(Utils.GetErrorIDS(msg.errorCode), null, MB_TYPE.MB_OK, null);
		}
	}
	
	private IEnumerator ShowCompleteEffect()
    {
        SoundManager.Instance.PlaySoundEffect("quest_finish");

        claimEffectRoot.SetActive(true);

		yield return new WaitForSeconds(2.5f);

        claimEffectRoot.SetActive(false);
    }

    public void SetFocusItem(UIQuestItem item)
    {
        if (item != _selectedItem)
        {
            if (_selectedItem != null)
            {
                _selectedItem.OnLoseFocus();
            }

            _selectedItem = item;
            _selectedItem.OnSetFocus();

            this.DisplayQuest(_selectedItem.questProgress);
        }
    }

    #region Tutorial
    private int _tutorialStep = 0;  //none = 0 claim = 1, fight = 2
    public void TryStartTutorial()
    {
        CloseTutorial();

        //tutorial: claim and back
		EnumTutorialState state = PlayerInfo.Instance.GetTutorialState(EnumTutorial.Town_Quest1);
        //state = 1;  //open this for debug
		if (state == EnumTutorialState.Active)
        {
            BeginStep1();
            return;
        }

        //tutorial: fight
		state = PlayerInfo.Instance.GetTutorialState(EnumTutorial.Town_Quest2);
        //state = 1; //open this for debug
        if (state == EnumTutorialState.Active)
        {
            BeginStep6();
        }
    }

    private void BeginStep1()
    {
        _tutorialStep = 1;

        tutorialColliderMask.transform.parent.gameObject.SetActive(true);

        tutorialTips[0].SetActive(true);

        tutorialTips[0].GetComponentInChildren<UILabel>().text = Localization.instance.Get("IDS_TU_TOWN_QUEST_CLAIM");

        arrowClaimFight.SetActive(true);

        Utils.ChangeColliderZPos(arrowClaimFight.transform.parent.gameObject, true, -6, -30);
    }

    private void BeginStep2()
    {
        if (_tutorialStep > 0)
        {
            tutorialTips[0].SetActive(false);

            arrowClaimFight.SetActive(false);

            Utils.ChangeColliderZPos(arrowClaimFight.transform.parent.gameObject, false, -6, -30);
        }
    }

    private void BeginStep3()
    {
        if (_tutorialStep > 0)
        {
            CloseTutorial();
			_tutorialStep = 3;

            tutorialColliderMask.transform.parent.gameObject.SetActive(true);

            Utils.ChangeColliderZPos(arrowBack.transform.parent.gameObject, true, -6, -30);

            arrowBack.SetActive(true);
        }
    }

    private void BeginStep6()
    {

        CloseTutorial();
		_tutorialStep = 6;

        tutorialColliderMask.transform.parent.gameObject.SetActive(true);

        tutorialTips[0].SetActive(true);

        tutorialTips[0].GetComponentInChildren<UILabel>().text = Localization.instance.Get("IDS_TU_TOWN_QUEST_FIGHT");

        arrowClaimFight.SetActive(true);

        Utils.ChangeColliderZPos(arrowClaimFight.transform.parent.gameObject, true, -6, -30);
    }

    public void TryFinishTutorial_40()
    {
        if (_tutorialStep != 0)
        {
			TutorialTownManager.Instance.TryFinishTutorialTown(EnumTutorial.Town_Quest1);
            CloseTutorial();
        }
    }

    public void TryFinishTutorial_41()
    {
        if (_tutorialStep != 0)
        {
			TutorialTownManager.Instance.TryFinishTutorialTown(EnumTutorial.Town_Quest2);
            CloseTutorial();
        }
    }

    public void CloseTutorial()
    {
        _tutorialStep = 0;

        Utils.ChangeColliderZPos(arrowClaimFight.transform.parent.gameObject, false, -6, -30);

        Utils.ChangeColliderZPos(arrowBack.transform.parent.gameObject, false, -6, -30);

        foreach (GameObject go in tutorialTips)
        {
            go.SetActive(false);
        }

        arrowBack.SetActive(false);

        arrowClaimFight.SetActive(false);

        tutorialColliderMask.transform.parent.gameObject.SetActive(false);
    }
    #endregion
}