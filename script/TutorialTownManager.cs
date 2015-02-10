using UnityEngine;
using System.Collections.Generic;

public class TutorialTownManager : MonoBehaviour
{
	public const string k_equip_item1 = "weapon_1_green_tutorial";
	public const string k_equip_item2 = "necklace_1_green_tutorial";

	public TownHUD uITownHome;

	public GameObject maskGO;

	public bool isInTutorial = false;

	public TutorialTownList tutorialDataList = null;

	private bool _needSaveFlag = false;

	private static TutorialTownManager _instance;
	public static TutorialTownManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType(typeof(TutorialTownManager)) as TutorialTownManager;
			}
			return _instance;
		}
	}

	void Awake()
	{
		_instance = this;
		isInTutorial = false;
		_needSaveFlag = false;
	}

	void OnDestroy()
	{
		_instance = null;
	}


	public TutorialTown GetTutorialTown(EnumTutorial tutorialId)
	{
		foreach (TutorialTown tutorialTown in tutorialDataList._dataList)
		{
			if (tutorialTown.id == tutorialId)
			{
				return tutorialTown;
			}
		}
		return null;
	}


	public void ResetActiveTutorial()
	{
		foreach (TutorialTown tutorialTown in tutorialDataList._dataList)
		{
			EnumTutorialState state = PlayerInfo.Instance.GetTutorialState(tutorialTown.id);
			if (state == EnumTutorialState.Active) //if this tutorial is active, set to inactive
			{
				PlayerInfo.Instance.ChangeTutorialState(tutorialTown.id, 0);
			}
		}
	}


	public void CheckIfStartTutorial()
	{
		if (true)
		{
			//return; //FIXME 
		}


		if (isInTutorial)
		{
			return;
		}


		foreach (TutorialTown tutorialTown in tutorialDataList._dataList)
		{
			if (TryStartTutorialTown(tutorialTown.id))
			{
				DoStartTutorialTown(tutorialTown.id);
				return;
			}
		}
	}


	public void DoStartTutorialTown(EnumTutorial tutorialId)
	{
		//_uITownHome.DoStartTutorialTown(tutorialId);	
	}

	public void DoFinishTutorialTown(EnumTutorial tutorialId)
	{
		//_uITownHome.DoFinishTutorialTown(tutorialId);	
	}


	public bool TryFinishTutorialTown(EnumTutorial tutorialId)
	{
		isInTutorial = false;
		EnumTutorialState state = PlayerInfo.Instance.GetTutorialState(tutorialId);
		if (state != EnumTutorialState.Finished)
		{
			PlayerInfo.Instance.ChangeTutorialState(tutorialId, EnumTutorialState.Finished);
			DoFinishTutorialTown(tutorialId);

			if (tutorialId == EnumTutorial.Town_Fusion)
			{
				GameManager._showRateApp = true;
			}
			TutorialTown tutorialTown = GetTutorialTown(tutorialId);
			if (tutorialTown.only_once)
			{
				PlayerInfo.Instance.ChangeTutorialState(tutorialId, EnumTutorialState.Finished);
			}

			_needSaveFlag = true;

			return true;
		}
		return false;
	}



	public void SaveAfterCompleteTutorial()
	{
		if (_needSaveFlag)
		{
			//PlayerInfo.CurrentPlayer.SaveGameData();	
			_needSaveFlag = false;
		}
	}


	public bool TryStartTutorialTown(EnumTutorial tutorialId)
	{
		if (tutorialId == EnumTutorial.Town_Equip)
		{
			if (PlayerInfo.Instance.IsEquipedItem(k_equip_item1))
			{
				PlayerInfo.Instance.ChangeTutorialState(tutorialId, EnumTutorialState.Finished);
				return false;
			}
		}

		EnumTutorialState state = PlayerInfo.Instance.GetTutorialState(tutorialId);
		if (state == 0) //if this tutorial is not active or finish, check if can start.
		{
			TutorialTown tutorialTown = GetTutorialTown(tutorialId);
			EnumTutorialState preState = EnumTutorialState.Finished;
			if (tutorialTown.preId != EnumTutorial.None)
			{
				preState = PlayerInfo.Instance.GetTutorialState(tutorialTown.preId);
			}

			if (preState == EnumTutorialState.Finished) //if pre turiral have finish 
			{
				EnumLevelState levelState = EnumLevelState.D;
				if (!string.IsNullOrEmpty(tutorialTown.level))
				{
					levelState = PlayerInfo.Instance.GetLevelState(tutorialTown.level);
				}

				if (levelState >= EnumLevelState.D)
				{
					PlayerInfo.Instance.ChangeTutorialState(tutorialId, EnumTutorialState.Active);
					isInTutorial = true;
					return true;
				}
			}
		}
		return false;
	}


	public void InitTownCompoment(TownHUD townHome)
	{
		isInTutorial = false;
		_needSaveFlag = false;
		uITownHome = townHome;
	}


	public void CheatFinishAllTownTutorial()
	{
		foreach (TutorialTown tutorialTown in tutorialDataList._dataList)
		{
			PlayerInfo.Instance.ChangeTutorialState(tutorialTown.id, EnumTutorialState.Finished);
		}
	}


	public bool IfAllTutorialHaveFinish()
	{
		foreach (TutorialTown tutorialTown in tutorialDataList._dataList)
		{
			if (PlayerInfo.Instance.GetTutorialState(tutorialTown.id) != EnumTutorialState.Finished)
			{
				return false;
			}
		}
		return true;
	}

	public void TryToStartTownTutorial(GameObject go)
	{
		TutorialTriggerTown trigger = go.GetComponent<TutorialTriggerTown>();
		if (trigger != null && trigger.enabled)
		{
			trigger.TryToStartTutorial();
		}
	}
}
