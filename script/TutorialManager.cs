using UnityEngine;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
	public bool InTutorialFlag
	{
		set { _inTutorialFlag = value; }
		get { return _inTutorialFlag; }
	}
	private bool _inTutorialFlag = false;
	public TutorialLevelList tutorialLevelList = null;


	public bool isTutorialHpOrEnergy = false;

	private static TutorialManager _instance;
	public static TutorialManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType(typeof(TutorialManager)) as TutorialManager;
			}
			return _instance;
		}
	}

	void Awake()
	{
		_instance = this;
		_inTutorialFlag = false;
		isTutorialHpOrEnergy = false;
	}

	void OnDestroy()
	{
		_instance = null;
	}

	public bool IsValidString(string text)
	{
		return text != null && text.Length > 0;
	}


	public TutorialLevel GetTutorialLevel(EnumTutorial tutorialId)
	{
		foreach (TutorialLevel tutorialLevel in tutorialLevelList._dataList)
		{
			if (tutorialLevel.id == tutorialId)
			{
				return tutorialLevel;
			}
		}
		return null;
	}


	[ContextMenu("StartTutorialHp")]
	public void StartTutorialHp()
	{
		EnableTopBar(false);

		_skillHandler.ChangeState(0, false);
		_skillHandler.ChangeState(1, false);
		_skillHandler.ChangeState(2, false);
		_skillHandler.ChangeState(3, false);
		_skillHandler.ChangeState(4, false);
		_skillHandler._MP.GetComponent<UIButton>().isEnabled = false;
		GameManager.Instance.GamePaused = true;
	}

	[ContextMenu("StartTutorialEnergy")]
	public void StartTutorialEnergy()
	{
		EnableTopBar(false);
		_skillHandler.ChangeState(0, false);
		_skillHandler.ChangeState(1, false);
		_skillHandler.ChangeState(2, false);
		_skillHandler.ChangeState(3, false);
		_skillHandler.ChangeState(4, false);
		_skillHandler._HP.GetComponent<UIButton>().isEnabled = false;
		GameManager.Instance.GamePaused = true;
	}

	private void EnableTopBar(bool enabled)
	{
		BoxCollider[] colliders = _topBar.GetComponentsInChildren<BoxCollider>();

		foreach (BoxCollider c in colliders)
		{
			c.enabled = enabled;
		}
	}

	[ContextMenu("FinishTutorialHp")]
	public void FinishTutorialHp()
	{
		EnableTopBar(true);

		_skillHandler.TryToActiveSkillButton(0, true);
		_skillHandler.TryToActiveSkillButton(1, true);
		_skillHandler.TryToActiveSkillButton(2, true);
		_skillHandler.TryToActiveSkillButton(3, true);
		_skillHandler.TryToActiveSkillButton(4, true);
		_skillHandler._MP.GetComponent<UIButton>().isEnabled = true;
		GameManager.Instance.GamePaused = false;
	}

	[ContextMenu("FinishTutorialEnergy")]
	public void FinishTutorialEnergy()
	{
		EnableTopBar(true);

		_skillHandler.TryToActiveSkillButton(0, true);
		_skillHandler.TryToActiveSkillButton(1, true);
		_skillHandler.TryToActiveSkillButton(2, true);
		_skillHandler.TryToActiveSkillButton(3, true);
		_skillHandler.TryToActiveSkillButton(4, true);
		_skillHandler._HP.GetComponent<UIButton>().isEnabled = true;
		GameManager.Instance.GamePaused = false;
	}



	private bool _isInDefense = false;
	private bool _pauseTimerFlag = false;

	[ContextMenu("StartTutorialDefense")]
	public void StartTutorialDefense()
	{
		_isInDefense = true;
		_pauseTimerFlag = false;

		EnableTopBar(false);

		_hudPotion.SetActive(false);
		_skillHandler.ChangeState(0, false);
		_skillHandler.ChangeState(1, false);
		_skillHandler.ChangeState(2, false);
		_skillHandler.ChangeState(3, false);
		_skillHandler.ChangeState(4, false);


		ActionController ac = ObjectManager.Instance.GetMyActionController();
		ac.AIUse.GotoUnControlState(AIAgent.STATE.IDLE);
	}


	[ContextMenu("ReadyTutorialDefense")]
	public void ReadyTutorialDefense()
	{
		if (_isInDefense)
		{
			if (!_pauseTimerFlag) //make sure just pause once.
			{
				_pauseTimerFlag = true;
				_skillHandler.ChangeState(2, true);
				_skillHandler.ShowTutorialDedenseEffect();
				GameManager.Instance.GamePaused = true;
			}
		}

	}

	[ContextMenu("FinishTutorialDedense")]
	public void FinishTutorialDefense()
	{
		GameManager.Instance.GamePaused = false;
		_isInDefense = false;
		
		EnableTopBar(true);
		_hudPotion.SetActive(true);
		
		_skillHandler.TryToActiveSkillButton(0, true);
		_skillHandler.TryToActiveSkillButton(1, true);
		_skillHandler.TryToActiveSkillButton(3, true);
		_skillHandler.TryToActiveSkillButton(4, true);

		ActionController ac = ObjectManager.Instance.GetMyActionController();
		ac.AIUse.UnlockPlayer();

		if (ac.AIUse._aiType == FC_AI_TYPE.PLAYER_MAGE)
		{
			ac.AIUse.GoToAttackForce("Parry", 0);
		}
		else if (ac.AIUse._aiType == FC_AI_TYPE.PLAYER_MONK)
		{
			ac.AIUse.GoToAttackForce("Dodge", 0);
		}
		else
		{
			ac.AIUse.GoToAttackForce("Parry", 0);
		}
	}

	public void EnterTutorialLevel()
	{
		_topBar.SetActive(false);
		_hudPotion.SetActive(false);
		_skillHandler.ChangeButtonState(-1);
	}


	public void ReceiveStartTutorialEvent(EnumTutorial tutorialId)
	{
		if (LevelManager.Singleton.LevelFinishFlag)
		{
			return;
		}

		if (_inTutorialFlag)
		{
			return;
		}

		EnumTutorialState state = PlayerInfo.Instance.GetTutorialState(tutorialId);
		if (state != EnumTutorialState.Inactive)
		{
			return;
		}


		if (tutorialId == EnumTutorial.Battle_Move)
		{
			_joystick.BeginTutorial(tutorialId);
		}
		else if (tutorialId < EnumTutorial.Battle_Revive)
		{
			_skillHandler.BeginTutorial(tutorialId);
		}
	}

	public void ReceiveFinishTutorialEvent(EnumTutorial tutorialId)
	{
		EnumTutorialState state = PlayerInfo.Instance.GetTutorialState(tutorialId);
		if (state != EnumTutorialState.Active)
		{
			//we will finish this two tutorial even it  is not active. 
			if (tutorialId == EnumTutorial.Battle_HealthPotion || tutorialId == EnumTutorial.Battle_EnergyPotion)
			{
				PlayerInfo.Instance.ChangeTutorialState(tutorialId, EnumTutorialState.Finished);
			}
			return;
		}

		if (tutorialId == EnumTutorial.Battle_Move)
		{
			_joystick.FinishTutorial(tutorialId);
		}
		else if (tutorialId < EnumTutorial.Battle_Revive)
		{
			_skillHandler.FinishTutorial(tutorialId);
		}
	}


	public bool TryStartTutorialLevel(EnumTutorial tutorialId)
	{
		EnumTutorialState state = PlayerInfo.Instance.GetTutorialState(tutorialId);
		
		if (state == EnumTutorialState.Inactive) //if this tutorial is not active or finish, check if can start.
		{
			TutorialLevel tutorialLevel = GetTutorialLevel(tutorialId);
			EnumTutorialState preState = EnumTutorialState.Inactive;
			if (tutorialLevel.preId != EnumTutorial.None)
			{
				preState = PlayerInfo.Instance.GetTutorialState(tutorialLevel.preId);
			}
			else
			{
				preState = EnumTutorialState.Finished;
			}


			if (preState == EnumTutorialState.Finished) //if pre turiral have finish , start this tutorial
			{
				PlayerInfo.Instance.ChangeTutorialState(tutorialId, EnumTutorialState.Active);
				StartTutorialLevel(tutorialLevel);
				_inTutorialFlag = true;


				if (tutorialId == EnumTutorial.Battle_HealthPotion || tutorialId == EnumTutorial.Battle_EnergyPotion)
				{
					isTutorialHpOrEnergy = true;
				}

				return true;
			}
		}

		return false;
	}


	public void StartTutorialLevel(TutorialLevel tutorialLevel)
	{
		if (IsValidString(tutorialLevel.StartIds))
		{
			MessageController.Instance.AddMessage(tutorialLevel.start_time, Localization.instance.Get(tutorialLevel.StartIds));
		}
	}


	public bool TryFinishTutorialLevel(EnumTutorial tutorialId)
	{
		_inTutorialFlag = false;
		isTutorialHpOrEnergy = false;

		EnumTutorialState state = PlayerInfo.Instance.GetTutorialState(tutorialId);
		if (state != EnumTutorialState.Finished)
		{
			TutorialLevel tutorialLevel = GetTutorialLevel(tutorialId);

			PlayerInfo.Instance.ChangeTutorialState(tutorialId, EnumTutorialState.Finished);

			if (tutorialLevel.only_once)
			{
				PlayerInfo.Instance.ChangeTutorialState(tutorialId, EnumTutorialState.Finished);
			}

			FinishTutorialLevel(tutorialLevel);
		}

		return true;
	}

	public void FinishTutorialLevel(TutorialLevel tutorialLevel)
	{
		if (IsValidString(tutorialLevel.StartIds))
		{
			MessageController.Instance.CloseMessage(Localization.instance.Get(tutorialLevel.StartIds));
		}

		if (IsValidString(tutorialLevel.FinishIds))
		{
			MessageController.Instance.AddMessage(tutorialLevel.finish_time, Localization.instance.Get(tutorialLevel.FinishIds));
		}
	}

	private HUDSkillHandler _skillHandler;
	private Joystick _joystick;

	private GameObject _topBar;
	private GameObject _hudPotion;

	public void InitHudCompoment()
	{
		_inTutorialFlag = false;
		isTutorialHpOrEnergy = false;

		GameObject go = UIManager.Instance._entryUI;
		_skillHandler = go.GetComponent<HUDSkillHandler>();
		_joystick = go.GetComponent<Joystick>();

		_topBar = go.transform.FindChild("Panel(TopUp)").gameObject;
		_hudPotion = go.transform.FindChild("Panel/Anchor(Potions)").gameObject;
		ResetActiveTutorial();
	}

	public HUDSkillHandler GetHUDSkillHandler()
	{
		return _skillHandler;
	}
	public void CheatFinishALlLevelTutorial()
	{
		foreach (TutorialLevel tutorialLevel in tutorialLevelList._dataList)
		{
			PlayerInfo.Instance.ChangeTutorialState(tutorialLevel.id, EnumTutorialState.Finished);
		}
	}

	public void ResetActiveTutorial()
	{
		foreach (TutorialLevel tutorialLevel in tutorialLevelList._dataList)
		{
			EnumTutorialState state = PlayerInfo.Instance.GetTutorialState(tutorialLevel.id);
			if (state == EnumTutorialState.Active) //if this tutorial is active, set to inactive
			{
				PlayerInfo.Instance.ChangeTutorialState(tutorialLevel.id, EnumTutorialState.Inactive);
			}
		}
	}
}
