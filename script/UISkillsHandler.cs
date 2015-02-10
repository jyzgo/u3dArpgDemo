using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using InJoy.FCComm;
using InJoy.Utils;


public class UISkillsHandler : MonoBehaviour
{
	public UIGrid skillGrid;
	public UICenterOnChild centerChild;
	public GameObject skillCellPrefab;

	public UILabel labelSkillName;
	public UILabel labelSCCost;
	public UILabel labelHCCost;
	public UILabel labelSkillLevel;
	public UILabel labelSkillDesc;
	public UILabel labelConditionTip;

	public UIImageButton upgradeWithSC;

	public UIPanel gridPanel;

	private SkillDataList _skills;
	private UIDraggablePanel mDrag;

	private bool _updateNewTagPosition = false;

	private GameObject _selectedSkillGO;	//the skill that user clicked or just centered

	// Use this for initialization
	void Start()
	{
		if (mDrag == null)
		{
			mDrag = gameObject.GetComponentInChildren<UIDraggablePanel>();
		}

		DisplaySkillInfo();
	}

	void OnInitialize()
	{
		UIManager.Instance.CloseUI("TownHome");

		CameraController.Instance.MainCamera.gameObject.SetActive(false);

		if (mDrag == null)
		{
			mDrag = gameObject.GetComponentInChildren<UIDraggablePanel>();
		}

		ClearSkillList();

		FillSkillGrid();

		centerChild.onFinished = OnCenterCellChanged;

		_lastSelectedSkill = null;

		if (!_tutorialStarted)
		{
			TryStartTutorial();

			_tutorialStarted = true;
		}

		int skillNum = FindFirstNewSkill();
		if (skillNum >= 0)
		{
			SelectSkill(skillNum);
			_updateNewTagPosition = true;
		}
		else
		{
			SelectSkill(0);
			_updateNewTagPosition = false;
		}

		LevelManager.Singleton.StopActorMoving();

		centerChild.Recenter();
	}

	// Display all skills.
	void FillSkillGrid()
	{
		List<SkillData> skills = DataManager.Instance.GetAllSkill();
		int index = 0;
		foreach (SkillData skillData in skills)
		{
			AddSkillCell(skillData, index++);
		}
		skillGrid.Reposition();
	}

	// Clear skill list
	void ClearSkillList()
	{
		List<Transform> itemlist = new List<Transform>();

		foreach (Transform t in skillGrid.transform)
		{
			itemlist.Add(t);
		}

		foreach (Transform t in itemlist)
		{
			t.parent = null;
			NGUITools.Destroy(t.gameObject);
		}

		itemlist.Clear();
	}

	void OnDisable()
	{
		centerChild.onFinished = null;
	}

	void SetSkill(SkillData skillData, UITexture skillIcon)
	{
		if (skillData != null)
		{
			Texture2D tex = InJoy.AssetBundles.AssetBundles.Load(skillData.iconPath) as Texture2D;

			skillIcon.mainTexture = tex;
			skillIcon.gameObject.SetActive(true);
		}
		else
		{
			skillIcon.gameObject.SetActive(false);
		}
	}

	private SkillData _lastSelectedSkill;
	void OnCenterCellChanged()
	{
		SkillData skillData = GetCenteredSkill();

		if ((skillData == null) || (skillData == _lastSelectedSkill))
		{
			return;
		}

		_lastSelectedSkill = skillData;

		SoundManager.Instance.PlaySoundEffect("menu_scroll");

		SelectSkill(centerChild.centeredObject);
	}

	private SkillData GetCenteredSkill()
	{
		if (centerChild.centeredObject != null)
		{
			return centerChild.centeredObject.GetComponent<UISkillCell>().skillData;
		}
		return null;
	}

	void AddSkillCell(SkillData skillData, int index)
	{
		GameObject o = NGUITools.AddChild(skillGrid.gameObject, skillCellPrefab);
		o.name = index.ToString() + " " + skillData.skillID;

		UISkillCell skillcell = o.GetComponent<UISkillCell>();

		skillcell.SetData(skillData, this);
	}

	void UpdateAllSkillState()
	{
		foreach (Transform t in skillGrid.transform)
		{
			UISkillCell skillcell = t.GetComponent<UISkillCell>();

			int skillRank = PlayerInfo.Instance.GetSkillLevel(skillcell.skillData.skillID);
			skillcell.Unlocked = (skillRank == 0);
			skillcell.UpdateFlagState();
		}
	}

	public static bool IsNewSkill(SkillData skilldata)
	{
		int skillRank = PlayerInfo.Instance.GetSkillLevel(skilldata.skillID);
		SkillUpgradeData ranksdata = DataManager.Instance.GetSkillUpgradeData(skilldata.enemyID, skilldata.skillID);
		int nextRank = skillRank + 1;

		if (nextRank >= ranksdata.upgradeDataList.Count)
		{
			return false;
		}

		int unLockLevel = ranksdata.upgradeDataList[nextRank].unlockLevel;
		string preSkillId = ranksdata.upgradeDataList[nextRank].preSkillId;
		int preSkillLevelNeed = ranksdata.upgradeDataList[nextRank].preSkillLevel;
		int preSkillCurrentLevel = PlayerInfo.Instance.GetSkillLevel(preSkillId);

		if ((preSkillCurrentLevel >= preSkillLevelNeed)
			&& (PlayerInfo.Instance.CurrentLevel >= unLockLevel))
		{
			return true;
		}

		return false;
	}


	public static int GetNewSkillCount()
	{
		int newCount = 0;
		List<SkillData> skillList = DataManager.Instance.GetAllSkill();
		foreach (SkillData skilldata in skillList)
		{
			if (IsNewSkill(skilldata))
			{
				newCount += 1;
			}
		}
		return newCount;
	}

	public int FindFirstNewSkill()
	{
		int skillNum = -1;
		List<SkillData> skillList = DataManager.Instance.GetAllSkill();
		foreach (SkillData skilldata in skillList)
		{
			skillNum++;

			if (IsNewSkill(skilldata))
			{
				return skillNum;
			}
		}

		return -1;
	}

	void DisplaySkillInfo()
	{
		SkillData skillData = GetSelectedSkill();

		if (skillData != null)
		{
			string ids, name;

			// Skill name
			if (!skillData.isPassive)
			{
				ids = Localization.instance.Get("IDS_MESSAGE_SKILLS_ACTIVESKILL");
				name = Localization.instance.Get(skillData.nameIDS);
			}
			else
			{
				ids = Localization.instance.Get("IDS_MESSAGE_SKILLS_PASSIVESKILL");
				name = Localization.instance.Get(skillData.nameIDS);
			}

			int skillLevel = PlayerInfo.Instance.GetSkillLevel(skillData.skillID);
			labelSkillName.text = string.Format(ids, name);//[1,5]

			// Skill description
			SkillUpgradeData ranksdata = DataManager.Instance.GetSkillUpgradeData(skillData.enemyID, skillData.skillID);
			int maxRanks = ranksdata.upgradeDataList.Count - 1;

            SkillGrade skillGrade = ranksdata.GetSkillGradeBySkillLevel(skillLevel);
            labelSkillDesc.text = Localization.instance.Get((null == skillGrade) ? "" : skillGrade.descIDS);

			ids = Localization.instance.Get("IDS_MESSAGE_GLOBAL_LEVEL");
			labelSkillLevel.text = string.Format("{0}: {1}/{2}", ids, skillLevel, maxRanks);

			// Conditions
			//skillRank=[1,5]
			int nextRank = Mathf.Min(skillLevel + 1, maxRanks);
			int sc = ranksdata.upgradeDataList[nextRank].costSc;
			int hc = ranksdata.upgradeDataList[nextRank].costHc;
			int unLockLevel = ranksdata.upgradeDataList[nextRank].unlockLevel;
			string preSkillId = ranksdata.upgradeDataList[nextRank].preSkillId;
			int preSkillLevelNeed = ranksdata.upgradeDataList[nextRank].preSkillLevel;
			int preSkillCurrentLevel = PlayerInfo.Instance.GetSkillLevel(preSkillId);

			labelSCCost.text = sc.ToString();
			labelHCCost.text = hc.ToString();

			labelConditionTip.text = string.Empty;

			// Require level XXX to learn.
			if (unLockLevel > PlayerInfo.Instance.CurrentLevel)  //level requirement not met, show hc
			{
				ids = Localization.instance.Get("IDS_MESSAGE_SKILLS_NEED_LEVEL");
				labelConditionTip.text = string.Format(ids, unLockLevel);

				upgradeWithSC.isEnabled = false;
			}
			else
			{
				upgradeWithSC.isEnabled = true;
			}
		}
	}

	void SelectSkill(int index)
	{
		if (index < skillGrid.transform.childCount)
		{
			Transform t = skillGrid.transform.GetChild(index);

			SelectSkill(t.gameObject);
		}
	}

	public void SelectSkill(GameObject skillGO)
	{
		if (skillGO == null)
		{
			return;
		}

		_selectedSkillGO = skillGO;

		DisplaySkillInfo();

		// Calculate the panel's center in world coordinates
		Vector4 clip = mDrag.panel.clipRange;
		Transform dt = mDrag.panel.cachedTransform;
		Vector3 center = dt.localPosition;
		center.x += clip.x;
		center.y += clip.y;
		center = dt.parent.TransformPoint(center);

		// Figure out the difference between the chosen child and the panel's center in local coordinates
		Vector3 cp = dt.InverseTransformPoint(skillGO.transform.position);
		Vector3 cc = dt.InverseTransformPoint(center);
		Vector3 offset = cp - cc;

		// Offset shouldn't occur if blocked by a zeroed-out scale
		if (mDrag.scale.x == 0f) offset.x = 0f;
		if (mDrag.scale.y == 0f) offset.y = 0f;
		if (mDrag.scale.z == 0f) offset.z = 0f;

		// Spring the panel to this calculated position
		SpringPanel.Begin(mDrag.gameObject, dt.localPosition - offset * 0.995f, 8f).onFinished = OnCenterFinished;
	}

	void OnCenterFinished()
	{
		centerChild.Recenter();
	}

	private SkillData GetSelectedSkill()
	{
		if (_selectedSkillGO != null)
		{
			return _selectedSkillGO.GetComponent<UISkillCell>().skillData;
		}
		return null;
	}

	// Upgrade or Learn Skill
	void OnClickUpgradeWithHC()
	{
		int sc, hc;
		NeedUseHC(out sc, out hc);

		if (hc > PlayerInfo.Instance.HardCurrency)
		{
			UIMessageBoxManager.Instance.ShowErrorMessageBox(5001, "Skills");
		}
		else
		{
			int max = (int)DataManager.Instance.CurGlobalConfig.getConfig("hcWarningAmount");
			if (hc >= max)
			{
				UIMessageBoxManager.Instance.ShowMessageBox(
					string.Format(Localization.instance.Get("IDS_MESSAGE_TATTOO_CONFIRM_HCLEARNSKILL"), hc,
					Localization.instance.Get(GetSelectedSkill().nameIDS)),
					null, MB_TYPE.MB_OKCANCEL, OnClickUpgradeSkillCallBack);
			}
			else
			{
				UpgradeSkill();
			}
		}
	}

	void OnClickUpgradeWithSC()
	{
		int sc, hc;
		NeedUseHC(out sc, out hc);

		if (sc > PlayerInfo.Instance.SoftCurrency)
		{
			UIMessageBoxManager.Instance.ShowErrorMessageBox(5002, "Skills");
		}
		else
		{
			UpgradeSkill();
		}
	}

	public void OnClickUpgradeSkillCallBack(ID_BUTTON buttonID)
	{
		if (ID_BUTTON.ID_OK == buttonID)
		{
			UpgradeSkill();
		}
	}

	void UpgradeSkill()
	{
		ConnectionManager.Instance.RegisterHandler(LearnOrUpgradeOp, true);
	}

	bool NeedUseHC(out int sc, out int hc)
	{
		SkillData skilldata = GetSelectedSkill();

		SkillUpgradeData ranksdata = DataManager.Instance.GetSkillUpgradeData(skilldata.enemyID, skilldata.skillID);
		int skillRank = PlayerInfo.Instance.GetSkillLevel(skilldata.skillID);
		int maxRanks = ranksdata.upgradeDataList.Count - 1;
		int nextRank = Mathf.Min(skillRank + 1, maxRanks);
		sc = ranksdata.upgradeDataList[nextRank].costSc;
		hc = ranksdata.upgradeDataList[nextRank].costHc;
		int unLockLevel = ranksdata.upgradeDataList[nextRank].unlockLevel;
		return sc > PlayerInfo.Instance.SoftCurrency
			|| unLockLevel > PlayerInfo.Instance.CurrentLevel;
	}

	void LearnOrUpgradeOp()
	{
		SkillData skilldata = GetSelectedSkill();

		if (skilldata != null)
		{
			int sc, hc;
			if (NeedUseHC(out sc, out hc))
			{
				LearnWithHC(skilldata.skillID, hc);
			}
			else
			{
				LearnWithSC(skilldata.skillID);
			}

			UpdateAllSkillState();
		}
	}

	void OnUpgradeSuccessCallback(ID_BUTTON buttonId)
	{
		if (buttonId == ID_BUTTON.ID_OK)
		{
			BeginStep2();
		}
	}

	void LearnWithSC(string skillId)
	{
		SkillUpgradeRequest request = new SkillUpgradeRequest();

		request.skillID = skillId;

		request.useHC = 0;

		NetworkManager.Instance.SendCommand(request, OnUpgradeSkill);
	}

	void LearnWithHC(string skillId, int HC)
	{
		if (HC <= PlayerInfo.Instance.HardCurrency)
		{
			SkillUpgradeRequest request = new SkillUpgradeRequest();

			request.skillID = skillId;

			request.useHC = 1;

			NetworkManager.Instance.SendCommand(request, OnUpgradeSkill);
		}
		else
		{
			ConnectionManager.Instance.SendACK(LearnOrUpgradeOp, true);

			string text = Localization.instance.Get("IDS_MESSAGE_GLOBAL_NOTENOUGHHC");
			UIMessageBoxManager.Instance.ShowMessageBox(text, "", MB_TYPE.MB_OKCANCEL, OnMessageboxCallback);
		}
	}

	void OnUpgradeSkill(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			ConnectionManager.Instance.SendACK(LearnOrUpgradeOp, true);

			SkillData skillData = (msg as SkillUpgradeResponse).skillData;

			PlayerInfo.Instance.UpgradeSkill(skillData);

			(msg as SkillUpgradeResponse).updateData.Broadcast();

			//refresh UI
			UpdateAllSkillState();

			DisplaySkillInfo();

			string ids = Localization.instance.Get("IDS_MESSAGE_SKILLS_LEARNSUCCESSFUL");

			UIMessageBoxManager.Instance.ShowMessageBox(ids, null, MB_TYPE.MB_OK, OnUpgradeSuccessCallback);
		}
		else
		{
			ConnectionManager.Instance.SendACK(LearnOrUpgradeOp, true);

			UIMessageBoxManager.Instance.ShowMessageBox(Utils.GetErrorIDS(msg.errorCode), null, MB_TYPE.MB_OK, null);
		}
	}

	void OnMessageboxCallback(ID_BUTTON buttonId)
	{
		if (buttonId == ID_BUTTON.ID_OK)
		{
			//todo: go to store
		}
	}

	void OnBuyHCFinishCallback()
	{
		DisplaySkillInfo();
	}

	void OnClickBackToTown()
	{
		TryFinishTutorial();

		CameraController.Instance.MainCamera.gameObject.SetActive(true);

		UIManager.Instance.OpenUI("TownHome");
		UIManager.Instance.CloseUI("Skills");
	}


	#region Tutorial
	private bool _tutorialStarted = false;

	public GameObject[] _tutorialTips = new GameObject[2];
	public GameObject _tutorialColliderMask;
	public GameObject _upgradeArrow;
	public GameObject _backArrow;
	int _tutorialStep;

	public void CloseTutorial()
	{
		_tutorialStep = 0;

		foreach (GameObject go in _tutorialTips)
		{
			go.SetActive(false);
		}

		_tutorialColliderMask.SetActive(false);
		_upgradeArrow.SetActive(false);
		_backArrow.SetActive(false);

		Utils.ChangeColliderZPos(_upgradeArrow.transform.parent.gameObject, false, -1f, -30f);
		Utils.ChangeColliderZPos(_backArrow.transform.parent.gameObject, false, -1f, -30f);

		_tutorialColliderMask.transform.parent.gameObject.SetActive(false);
	}

	public void TryStartTutorial()
	{
		CloseTutorial();

		string thirdSkill;
		int role = (int)PlayerInfo.Instance.Role;
		if (role == 0)
		{
			thirdSkill = "LightningBall";
		}
		else if (role == 1)
		{
			thirdSkill = "Vortex";
		}
		else
		{
			thirdSkill = "Bash";
		}

		int rank = PlayerInfo.Instance.GetSkillLevel(thirdSkill);
		if (rank > 0)
		{
			return;
		}


		EnumTutorialState state = PlayerInfo.Instance.GetTutorialState(EnumTutorial.Town_Skill);
		if (state == EnumTutorialState.Active)
		{
			BeginStep1();
		}
	}

	public void BeginStep1()
	{
		_tutorialStep = 1;
		_tutorialColliderMask.transform.parent.gameObject.SetActive(true);
		_tutorialColliderMask.SetActive(true);

		_upgradeArrow.SetActive(true);
		Utils.ChangeColliderZPos(_upgradeArrow.transform.parent.gameObject, true, -1f, -30f);

		_tutorialTips[0].SetActive(true);

		string ids = Localization.instance.Get("IDS_SKILLS_TUTORIAL_STEP_1");
		//SkillData skillData = GetSelectedSkill();
		_tutorialTips[0].GetComponentInChildren<UILabel>().text = ids;

		SelectSkill(2);
	}

	void BeginStep1_1()
	{
		_upgradeArrow.SetActive(false);
		Utils.ChangeColliderZPos(_upgradeArrow.transform.parent.gameObject, false, -1f, -30f);

		_tutorialTips[0].SetActive(false);
	}

	public void BeginStep2()
	{
		if (_tutorialStep == 1)
		{
			_tutorialStep = 2;

			_tutorialTips[1].SetActive(true);
			_tutorialTips[1].GetComponentInChildren<UILabel>().text = Localization.instance.Get("IDS_TU_GOOD_JOB");

			_backArrow.SetActive(true);
			Utils.ChangeColliderZPos(_backArrow.transform.parent.gameObject, true, -1f, -30f);
		}
	}



	public void TryFinishTutorial()
	{
		if (_tutorialStep != 0)
		{
			TutorialTownManager.Instance.TryFinishTutorialTown(EnumTutorial.Town_Skill);
			CloseTutorial();
			TutorialTownManager.Instance.SaveAfterCompleteTutorial();
		}
	}
	#endregion
}