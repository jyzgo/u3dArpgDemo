using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HUDSkillHandler : MonoBehaviour
{
	// Skill Buttons
	public GameObject[] _skillIcons;
	UIButton[] _skillButton;
	Texture[] _skillTextures;
	UITexture[] _skillTexes;
	UILabel[] _skillLabels;
	ParticleSystem[][] _skillParticles;

	// Shaders
	public Shader _grayShader;
	public Shader _normalShader;

	// Potion
	public GameObject _MP;
	private UIButton _MP_Button;
	private UITexture _MP_Tex;

	public UILabel _MPPotionCount;
	public GameObject _MP_HC;
	ParticleSystem[] _MP_Particles;

	public GameObject _HP;
	private UIButton _HP_Button;
	private UITexture _HP_Tex;

	public UILabel _HPPotionCount;
	public GameObject _HP_HC;
	ParticleSystem[] _HP_Particles;

	// MP
	int _MP_PVE_cooldownTime;
	public float _MP_instantRecoverPercent;
	public float _MP_laterRecoverPercent;
	public float _MP_laterRecoverTime;

	// HP
	int _HP_PVE_cooldownTime;
	public float _HP_instantRecoverPercent;
	public float _HP_laterRecoverPercent;
	public float _HP_laterRecoverTime;

	public GameObject _skillSlot3;
	public GameObject _skillSlot4;

	void Awake()
	{
		_skillTextures = new Texture[_skillIcons.Length];
		_skillTexes = new UITexture[_skillIcons.Length];
		_skillLabels = new UILabel[_skillIcons.Length];
		_skillParticles = new ParticleSystem[_skillIcons.Length][];

		_skillButton = new UIButton[_skillIcons.Length];

		for (int i = 0; i <= 4; i++)
		{
			_skillTexes[i] = _skillIcons[i].GetComponentInChildren<UITexture>();
			_skillLabels[i] = _skillIcons[i].GetComponentInChildren<UILabel>();
			_skillParticles[i] = _skillIcons[i].GetComponentsInChildren<ParticleSystem>();
			_skillButton[i] = _skillIcons[i].GetComponent<UIButton>();
		}

		_MP_Button = _MP.GetComponent<UIButton>();
		_MP_Tex = _MP.GetComponentInChildren<UITexture>();
		_MP_Particles = _MP.GetComponentsInChildren<ParticleSystem>();

		_HP_Button = _HP.GetComponent<UIButton>();
		_HP_Tex = _HP.GetComponentInChildren<UITexture>();
		_HP_Particles = _HP.GetComponentsInChildren<ParticleSystem>();

		InitTutorialEffect_Awake();
	}

	// Use this for initialization
	void Start()
	{
		InitializePotions();

		_HP_PVE_cooldownTime = System.Convert.ToInt32(DataManager.Instance.CurGlobalConfig.getConfig("potion_hp_pve_cooldown").ToString());
		_MP_PVE_cooldownTime = System.Convert.ToInt32(DataManager.Instance.CurGlobalConfig.getConfig("potion_mp_pve_cooldown").ToString());
	}

	// Update is called once per frame
	void OnEnable()
	{
		SetState(_MP_Button, _MP_Tex, _MPPotionCount, _MP_Particles, true);
		SetState(_HP_Button, _HP_Tex, _HPPotionCount, _HP_Particles, true);

		UILabel labelMP = _MP.GetComponentInChildren<UILabel>();
		if (labelMP != null)
		{
			labelMP.text = "";
		}

		UILabel labelHP = _HP.GetComponentInChildren<UILabel>();
		if (labelHP != null)
		{
			labelHP.text = "";
		}
	}

	void OnDestroy()
	{
		foreach (Texture t in _skillTextures)
		{
			if (t != null)
			{
				Resources.UnloadAsset(t);
			}
		}
	}

	private void ConfigureSkillIcons()
	{
		for (FC_KEY_BIND key = FC_KEY_BIND.ATTACK_2; key <= FC_KEY_BIND.ATTACK_5; key++)
		{
			SkillData sd = DataManager.Instance.GetSkillByKey(key);

			if (sd != null)
			{
				Texture2D tex = InJoy.AssetBundles.AssetBundles.Load(sd.iconPath) as Texture2D;

				SetSkillIcon(key, tex);
			}
			else
			{
				SetSkillIcon(key, null);
			}
		}
	}

	// Different character class will have different skill icons.
	public void SetSkillIcons(int playerClass)
	{
		Texture2D tex = null;

		if ((FC_AI_TYPE)playerClass == FC_AI_TYPE.PLAYER_MAGE)
		{
			tex = InJoy.AssetBundles.AssetBundles.Load("Assets/UI/bundle/SkillIcons/Mage_magic_ball.png") as Texture2D;
			SetSkillIcon(FC_KEY_BIND.ATTACK_1, tex);
		}
		else if ((FC_AI_TYPE)playerClass == FC_AI_TYPE.PLAYER_WARRIOR)
		{
			tex = InJoy.AssetBundles.AssetBundles.Load("Assets/UI/bundle/SkillIcons/Warrior_NormalAttack.png") as Texture2D;
			SetSkillIcon(FC_KEY_BIND.ATTACK_1, tex);
		}

		ConfigureSkillIcons();

		ActionController ac = ObjectManager.Instance.GetMyActionController();
		ac._enableInputKey = EnableInputKey;
		ac._updateInputKeyState = UpdateInputKeyState;


		InitTutorialEffect();
	}

	public void UpdateInputKeyState(FC_KEY_BIND keyBind, float timeLast, float timeLastPercent)
	{
		UILabel label = _skillLabels[(int)keyBind - 1];
		label.text = Mathf.RoundToInt(timeLast).ToString();

		if (label.text == "0")
		{
			label.text = "";
		}
	}

	void SetSkillIcon(FC_KEY_BIND pos, Texture2D icon)
	{
		GameObject skillButton = _skillIcons[(int)pos - 1];

		if (icon != null)
		{
			skillButton.SetActive(true);

			UITexture tex = _skillTexes[(int)pos - 1];
			tex.mainTexture = icon;

			if (_skillTextures[(int)pos - 1] != null)
			{
				Resources.UnloadAsset(_skillTextures[(int)pos - 1]);
			}

			_skillTextures[(int)pos - 1] = icon;
		}
		else // There is no skill in this slot.
		{
			skillButton.SetActive(false);

			if (_skillTextures[(int)pos - 1] != null)
			{
				Resources.UnloadAsset(_skillTextures[(int)pos - 1]);
			}

			_skillTextures[(int)pos - 1] = null;
		}
	}

	void SetSkillState(FC_KEY_BIND pos, bool active)
	{
		int skillIndex = (int)pos - 1;

        if (pos == FC_KEY_BIND.ATTACK_5)
        {
            ActionController ac = ObjectManager.Instance.GetMyActionController();
            if (null != ac )
            {
                active = ac.SkillGodDownActive;
            }
        }

		SetState(_skillButton[skillIndex], _skillTexes[skillIndex], _skillLabels[skillIndex], _skillParticles[skillIndex], active);
	}

	void EnableInputKey(FC_KEY_BIND keyBind, bool beActive)
	{
		SetSkillState(keyBind, beActive);
	}

	void OnPressNormalAttack()
	{
		List<FCObject> clients = InputManager.Instance.Clients;

		foreach (FCObject client in clients)
		{
			CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_PRESS,
					FCConst.FC_KEY_ATTACK_1, FC_PARAM_TYPE.INT,
					new Vector3(-1, 0, 0), FC_PARAM_TYPE.VECTOR3,
					client.ObjectID, FCCommand.STATE.RIGHTNOW, true);
		}
	}

	void OnReleaseNormalAttack()
	{
		List<FCObject> clients = InputManager.Instance.Clients;

		foreach (FCObject client in clients)
		{
			CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
					FCConst.FC_KEY_ATTACK_1, FC_PARAM_TYPE.INT,
					client.ObjectID, FCCommand.STATE.RIGHTNOW, true);
		}
	}

	void OnPressSkillAttack()
	{

		List<FCObject> clients = InputManager.Instance.Clients;

		foreach (FCObject client in clients)
		{

			CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_PRESS,
					FCConst.FC_KEY_ATTACK_2, FC_PARAM_TYPE.INT,
					new Vector3(-1, 0, 0), FC_PARAM_TYPE.VECTOR3,
					client.ObjectID, FCCommand.STATE.RIGHTNOW, true);
		}
	}

	void OnReleaseSkillAttack()
	{
		List<FCObject> clients = InputManager.Instance.Clients;

		foreach (FCObject client in clients)
		{
			CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
					FCConst.FC_KEY_ATTACK_2, FC_PARAM_TYPE.INT,
					client.ObjectID, FCCommand.STATE.RIGHTNOW, true);
		}

		TutorialManager.Instance.ReceiveFinishTutorialEvent(EnumTutorial.Battle_Skill1);

		SkillData sd = DataManager.Instance.GetSkillByKey(FC_KEY_BIND.ATTACK_2);
		BattleSummary.Instance.UseSkill(sd.skillID);
	}

	void OnPressAttack3()
	{
		List<FCObject> clients = InputManager.Instance.Clients;

		foreach (FCObject client in clients)
		{
			CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_PRESS,
					FCConst.FC_KEY_ATTACK_3, FC_PARAM_TYPE.INT,
					new Vector3(-1, 0, 0), FC_PARAM_TYPE.VECTOR3,
					client.ObjectID, FCCommand.STATE.RIGHTNOW, true);
		}
	}

	void OnReleaseAttack3()
	{
		List<FCObject> clients = InputManager.Instance.Clients;

		foreach (FCObject client in clients)
		{
			CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
					FCConst.FC_KEY_ATTACK_3, FC_PARAM_TYPE.INT,
					client.ObjectID, FCCommand.STATE.RIGHTNOW, true);
		}

		TutorialManager.Instance.ReceiveFinishTutorialEvent(EnumTutorial.Battle_Skill2);

		SkillData sd = DataManager.Instance.GetSkillByKey(FC_KEY_BIND.ATTACK_3);
		BattleSummary.Instance.UseSkill(sd.skillID);
	}

	void OnPressAttack4()
	{
		List<FCObject> clients = InputManager.Instance.Clients;

		foreach (FCObject client in clients)
		{
			CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_PRESS,
					FCConst.FC_KEY_ATTACK_4, FC_PARAM_TYPE.INT,
					new Vector3(-1, 0, 0), FC_PARAM_TYPE.VECTOR3,
					client.ObjectID, FCCommand.STATE.RIGHTNOW, true);
		}


	}

	void OnReleaseAttack4()
	{
		List<FCObject> clients = InputManager.Instance.Clients;

		foreach (FCObject client in clients)
		{
			CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
					FCConst.FC_KEY_ATTACK_4, FC_PARAM_TYPE.INT,
					client.ObjectID, FCCommand.STATE.RIGHTNOW, true);
		}

		TutorialManager.Instance.ReceiveFinishTutorialEvent(EnumTutorial.Battle_Skill3);

		SkillData sd = DataManager.Instance.GetSkillByKey(FC_KEY_BIND.ATTACK_4);
		BattleSummary.Instance.UseSkill(sd.skillID);
	}

	void OnPressAttack5()
	{
		List<FCObject> clients = InputManager.Instance.Clients;

		foreach (FCObject client in clients)
		{
			CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_PRESS,
					FCConst.FC_KEY_ATTACK_5, FC_PARAM_TYPE.INT,
					new Vector3(-1, 0, 0), FC_PARAM_TYPE.VECTOR3,
					client.ObjectID, FCCommand.STATE.RIGHTNOW, true);
		}
	}

	void OnReleaseAttack5()
	{
		List<FCObject> clients = InputManager.Instance.Clients;

		foreach (FCObject client in clients)
		{
			CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
					FCConst.FC_KEY_ATTACK_5, FC_PARAM_TYPE.INT,
					client.ObjectID, FCCommand.STATE.RIGHTNOW, true);
		}

		TutorialManager.Instance.ReceiveFinishTutorialEvent(EnumTutorial.Battle_Skill4);

		SkillData sd = DataManager.Instance.GetSkillByKey(FC_KEY_BIND.ATTACK_5);
		BattleSummary.Instance.UseSkill(sd.skillID);
	}

	/// <summary>
	/// Begin of Potions
	/// </summary>

	// Initialize Potions.
	void InitializePotions()
	{
		PlayerInventory inventory = PlayerInfo.Instance.PlayerInventory;
		int role = (int)PlayerInfo.Instance.Role;

		// MP Potion
		if (_MP.transform.parent.gameObject.activeSelf)
		{
			ItemData itemdata = DataManager.Instance.GetItemData(FCConst.k_potion_mp);
			Texture2D icon_mp = InJoy.AssetBundles.AssetBundles.Load(itemdata.iconPath) as Texture2D;
			UITexture tex = _MP.GetComponentInChildren<UITexture>();
			tex.mainTexture = icon_mp;
			_MPPotionCount.text = inventory.GetItemCount(FCConst.k_potion_mp).ToString();
		}

		// HP Potion
		if (_HP.transform.parent.gameObject.activeSelf)
		{
			ItemData itemdata = DataManager.Instance.GetItemData(FCConst.k_potion_hp);
			Texture2D icon_hp = InJoy.AssetBundles.AssetBundles.Load(itemdata.iconPath) as Texture2D;
			UITexture tex = _HP.GetComponentInChildren<UITexture>();
			tex.mainTexture = icon_hp;
			Texture2D icon_bg = InJoy.AssetBundles.AssetBundles.Load(UIGlobalSettings.Instance._consumableBgSprites[itemdata.rareLevel]) as Texture2D;

			_HPPotionCount.text = inventory.GetItemCount(FCConst.k_potion_hp).ToString();
		}
		UpdatePotions();
	}

	// Eat energy potion.
	void OnClickIncreaseEnergy()
	{
		if (ObjectManager.Instance.GetMyActionController().EnergyIsFull)
		{
			return;
		}

		PlayerInventory inventory = PlayerInfo.Instance.PlayerInventory;
		int count = inventory.GetItemCount(FCConst.k_potion_mp);

		if (count > 0)
		{
			inventory.UseItem(FCConst.k_potion_mp);
			_MPPotionCount.text = inventory.GetItemCount(FCConst.k_potion_mp).ToString();

			UseMPPotion();
			++BattleSummary.Instance.MpCost;
			NetworkManager.Instance.UseItemInBattle(LevelManager.Singleton.CurrentLevelData.id, FCConst.k_potion_mp, 1, 0, OnTakePotion);
		}
		else
		{
			StoreData sd = StoreDataManager.Instance.GetStoreData("potion_mp_1");
			if (PlayerInfo.Instance.HardCurrency >= sd._normalInfo._hardCurrency)
			{
				PlayerInfo.Instance.ReduceHardCurrency(sd._normalInfo._hardCurrency);

				UseMPPotion();

				NetworkManager.Instance.UseItemInBattle(LevelManager.Singleton.CurrentLevelData.id, FCConst.k_potion_mp, 1, 1, OnTakePotion);
			}
			else
			{
				EnumTutorialState tState = PlayerInfo.Instance.GetTutorialState(EnumTutorial.Battle_EnergyPotion);
				if (tState == EnumTutorialState.Active)
				{
					UseMPPotion();
				}
				else
				{
					string msg = Localization.instance.Get("IDS_HC_NOT_ENOUGH");
					UIMessageBoxManager.Instance.ShowMessageBox(msg, "", MB_TYPE.MB_OKCANCEL, OnBuyHCCallback);

					GameManager.Instance.GamePaused = true;
				}
			}
		}

		UpdatePotions();
	}

	void UseMPPotion()
	{
		TutorialManager.Instance.ReceiveFinishTutorialEvent(EnumTutorial.Battle_EnergyPotion);
		SetState(_MP_Button, _MP_Tex, _MPPotionCount, _MP_Particles, false);

		UILabel label = _MP.GetComponentInChildren<UILabel>();

		// Recover MP: _MP_instantRecoverPercent,_MP_laterRecoverPercent, _MP_laterRecoverTime
		CommandManager.Instance.Send(FCCommand.CMD.POTION_ENERGY,
			_MP_instantRecoverPercent, FC_PARAM_TYPE.FLOAT,
			_MP_laterRecoverPercent, FC_PARAM_TYPE.FLOAT,
			ObjectManager.Instance.GetMyActionController().ObjectID, FCCommand.STATE.RIGHTNOW, true);

		CommandManager.Instance.SendCommandToOthers(FCCommand.CMD.CLIENT_POTION_ENERGY,
			ObjectManager.Instance.GetMyActionController().ObjectID,
			Vector3.zero,
			_MP_instantRecoverPercent, FC_PARAM_TYPE.FLOAT,
			_MP_laterRecoverPercent, FC_PARAM_TYPE.FLOAT,
			null, FC_PARAM_TYPE.NONE);

		SoundManager.Instance.PlaySoundEffect("eat_mp_potion");

		StartCoroutine(CooldownPotion(_MP_PVE_cooldownTime, _MP_Button, _MP_Tex, label, _MP_Particles));


	}

	void OnBuyHCCallback(ID_BUTTON buttonID)
	{
		if (buttonID == ID_BUTTON.ID_OK)
		{
			//todo: IAP in battle UIMessageBoxManager.Instance.ShowShortcutIAP(OnBuyHCFinishCallback, true);
		}
		else
		{
			GameManager.Instance.GamePaused = false;
		}
	}

	void OnBuyHCFinishCallback()
	{
		GameManager.Instance.GamePaused = false;
	}

	// Use HP potion to reach full hp.
	void UseHPPotion()
	{
		TutorialManager.Instance.ReceiveFinishTutorialEvent(EnumTutorial.Battle_HealthPotion);
		SetState(_HP_Button, _HP_Tex, _HPPotionCount, _HP_Particles, false);

		UILabel label = _HP.GetComponentInChildren<UILabel>();

		// Recover HP: _HP_instantRecoverPercent,_HP_laterRecoverPercent, _HP_laterRecoverTime
		CommandManager.Instance.Send(FCCommand.CMD.POTION_HP,
			_HP_instantRecoverPercent, FC_PARAM_TYPE.FLOAT,
			_HP_laterRecoverPercent, FC_PARAM_TYPE.FLOAT,
			ObjectManager.Instance.GetMyActionController().ObjectID, FCCommand.STATE.RIGHTNOW, true);

		CommandManager.Instance.SendCommandToOthers(FCCommand.CMD.CLIENT_POTION_HP,
			ObjectManager.Instance.GetMyActionController().ObjectID,
			Vector3.zero,
			_HP_instantRecoverPercent, FC_PARAM_TYPE.FLOAT,
			_HP_laterRecoverPercent, FC_PARAM_TYPE.FLOAT,
			null, FC_PARAM_TYPE.NONE);

		SoundManager.Instance.PlaySoundEffect("eat_hp_potion");

		StartCoroutine(CooldownPotion(_HP_PVE_cooldownTime, _HP_Button, _HP_Tex, label, _HP_Particles));
	}

	// Eat HP potion.
	void OnClickIncreaseHP()
	{
		if (ObjectManager.Instance.GetMyActionController().HPIsFull
			|| !ObjectManager.Instance.GetMyActionController().IsAlived)
		{
			return;
		}

		PlayerInventory inventory = PlayerInfo.Instance.PlayerInventory;
		int count = inventory.GetItemCount(FCConst.k_potion_hp);

		if (count > 0)
		{
			inventory.UseItem(FCConst.k_potion_hp);
			_HPPotionCount.text = inventory.GetItemCount(FCConst.k_potion_hp).ToString();

			UseHPPotion();
			
			++BattleSummary.Instance.HpCost;

			NetworkManager.Instance.UseItemInBattle(LevelManager.Singleton.CurrentLevelData.id, FCConst.k_potion_hp, 1, 0, OnTakePotion);
		}
		else
		{
			StoreData sd = StoreDataManager.Instance.GetStoreData("potion_hp_hc");
			if (PlayerInfo.Instance.HardCurrency >= sd._normalInfo._hardCurrency)
			{
				PlayerInfo.Instance.ReduceHardCurrency(sd._normalInfo._hardCurrency);

				UseHPPotion();

				NetworkManager.Instance.UseItemInBattle(LevelManager.Singleton.CurrentLevelData.id, FCConst.k_potion_hp, 1, 1, OnTakePotion);
			}
			else
			{

				EnumTutorialState tState = PlayerInfo.Instance.GetTutorialState(EnumTutorial.Battle_HealthPotion);
				if (tState == EnumTutorialState.Active)
				{
					UseHPPotion();
				}
				else
				{
					string msg = Localization.instance.Get("IDS_HC_NOT_ENOUGH");
					UIMessageBoxManager.Instance.ShowMessageBox(msg, "", MB_TYPE.MB_OKCANCEL, OnBuyHCCallback);

					GameManager.Instance.GamePaused = true;
				}

			}
		}

		UpdatePotions();
	}

	void OnTakePotion(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			UpdateInforResponseData updateData = (msg as BattleUseItemResponse).updateData;

			if (updateData != null)
			{
				PlayerInfo.Instance.PlayerInventory.ApplyItemCountChanges(updateData.itemCountOps);
			}
		}
		else
		{
			//remember this, to fail this battle
			LevelManager.Singleton.IsCheat = true;

			Debug.LogError("Cheat on taking potions!");
		}
	}

	void UpdatePotions()
	{
		PlayerInventory inventory = PlayerInfo.Instance.PlayerInventory;
		int count = 0;

		count = inventory.GetItemCount(FCConst.k_potion_mp);
		if (count <= 0)
		{
			_MP_HC.SetActive(true);
			_MPPotionCount.gameObject.SetActive(false);
		}
		else
		{
			_MP_HC.SetActive(false);
			_MPPotionCount.gameObject.SetActive(true);
		}

		count = inventory.GetItemCount(FCConst.k_potion_hp);
		if (count <= 0)
		{
			_HP_HC.SetActive(true);
			_HPPotionCount.gameObject.SetActive(false);
		}
		else
		{
			_HP_HC.SetActive(false);
			_HPPotionCount.gameObject.SetActive(true);
		}
	}

	void SetState(UIButton uiButton, UITexture uiTexture, UILabel uiLabel, ParticleSystem[] particles, bool active)
	{
		if (uiTexture == null)
		{
			return;
		}

		if (active)
		{
			uiLabel.text = "";

			if (!uiButton.isEnabled && (particles.Length > 0))
			{
				foreach (ParticleSystem particle in particles)
				{
					particle.Play();
				}
			}
		}

		uiButton.isEnabled = active;
	}

	IEnumerator CooldownPotion(int time, UIButton uiButton, UITexture uiTexture, UILabel countdown, ParticleSystem[] ps)
	{
		while (time > 0)
		{
			time--;
			countdown.text = time.ToString();

			yield return new WaitForSeconds(1);
		}

		countdown.text = "";

		SetState(uiButton, uiTexture, countdown, ps, true);
	}

	/// <summary>
	/// End of Potions
	/// </summary>
	/// 

	public void TryToActiveSkillButton(int buttonIndex, bool active)
	{
		if (buttonIndex < _skillIcons.Length)
		{
			Debug.Log(buttonIndex);
			if (active
				&& _skillIcons[buttonIndex].activeInHierarchy
				&& _skillIcons[buttonIndex].GetComponentInChildren<UILabel>().text == "")
			{
				SetState(_skillButton[buttonIndex], _skillTexes[buttonIndex], _skillLabels[buttonIndex], _skillParticles[buttonIndex], active);
			}
			else if (!active)
			{
				SetState(_skillButton[buttonIndex], _skillTexes[buttonIndex], _skillLabels[buttonIndex], _skillParticles[buttonIndex], active);
			}
		}
	}

	public void ChangeState(int buttonIndex, bool active)
	{
		if (buttonIndex < _skillIcons.Length)
		{
			SetState(_skillButton[buttonIndex], _skillTexes[buttonIndex], _skillLabels[buttonIndex], _skillParticles[buttonIndex], active);
		}
	}


	public void ChangeButtonState(int unlockIndex)
	{
		for (int i = 0; i < _skillIcons.Length; i++)
		{
			_skillIcons[i].SetActive(unlockIndex >= i);
		}

	}


	public GameObject[] _buttonEffect = new GameObject[5];


	public void InitTutorialEffect_Awake()
	{
		for (int i = 0; i < _buttonEffect.Length; i++)
		{
			_buttonEffect[i].gameObject.SetActive(false);
		}
	}

	public void InitTutorialEffect()
	{
		bool use_skill2_50 = PlayerInfo.Instance.GetTutorialState(EnumTutorial.Battle_Skill2) == EnumTutorialState.Finished;
		_skillIcons[1].SetActive(PlayerInfo.Instance.GetTutorialState(EnumTutorial.Battle_Skill1) == EnumTutorialState.Finished);
		_skillIcons[2].SetActive(use_skill2_50);

		_skillSlot3.SetActive(use_skill2_50 && !_skillIcons[3].activeInHierarchy);
		_skillSlot4.SetActive(use_skill2_50 && !_skillIcons[4].activeInHierarchy);
	}


	public void BeginTutorial(EnumTutorial tutorialId)
	{
		if (tutorialId == EnumTutorial.Battle_HealthPotion || tutorialId == EnumTutorial.Battle_EnergyPotion)
		{
			if (LevelManager.Singleton.IsTutorialLevel())
			{
				return; //don't active this tutorial in "tutorial level".
			}
		}

		if (TutorialManager.Instance.TryStartTutorialLevel(tutorialId))
		{
			if (tutorialId == EnumTutorial.Battle_Attack)
			{
				_buttonEffect[0].SetActive(true);
				TutorialManager.Instance.GetHUDSkillHandler().ChangeButtonState(0);
			}
			else if (tutorialId ==  EnumTutorial.Battle_KillAnEnemy)
			{
				StartCoroutine(DelayShowArrowFor30());
			}
			else if (tutorialId == EnumTutorial.Battle_Skill1)
			{
				TutorialManager.Instance.GetHUDSkillHandler().ChangeButtonState(1);
				_buttonEffect[1].SetActive(true);
			}
			else if (tutorialId == EnumTutorial.Battle_Skill2)
			{
				TutorialManager.Instance.GetHUDSkillHandler().ChangeButtonState(2);
				TutorialManager.Instance.StartTutorialDefense();
			}
			else if (tutorialId == EnumTutorial.Battle_Skill3)
			{
				_buttonEffect[3].SetActive(true);
			}
			else if (tutorialId == EnumTutorial.Battle_Skill4)
			{
				_buttonEffect[4].SetActive(true);
			}
			else if (tutorialId == EnumTutorial.Battle_HealthPotion)
			{
				_buttonEffect[5].SetActive(true);
				//StartCoroutine(DelayHideEffectFor80());
				TutorialManager.Instance.StartTutorialHp();
			}
			else if (tutorialId == EnumTutorial.Battle_EnergyPotion)
			{

				_buttonEffect[6].SetActive(true);
				//StartCoroutine(DelayHideEffectFor90());
				TutorialManager.Instance.StartTutorialEnergy();
			}
		}
	}


	public void ShowTutorialDedenseEffect()
	{
		_buttonEffect[2].SetActive(true);
	}


	public void FinishTutorial(EnumTutorial tutorialId)
	{
		if (TutorialManager.Instance.TryFinishTutorialLevel(tutorialId))
		{
			if (tutorialId == EnumTutorial.Battle_Attack)
			{
				_buttonEffect[0].SetActive(false);
			}
			else if (tutorialId == EnumTutorial.Battle_KillAnEnemy)
			{
				_buttonEffect[0].SetActive(false);
			}
			else if (tutorialId == EnumTutorial.Battle_Skill1)
			{
				_buttonEffect[1].SetActive(false);
			}
			else if (tutorialId == EnumTutorial.Battle_Skill2)
			{
				_buttonEffect[2].SetActive(false);
				TutorialManager.Instance.FinishTutorialDefense();
			}
			else if (tutorialId == EnumTutorial.Battle_Skill3)
			{
				_buttonEffect[3].SetActive(false);
			}
			else if (tutorialId == EnumTutorial.Battle_Skill4)
			{
				_buttonEffect[4].SetActive(false);
			}
			else if (tutorialId == EnumTutorial.Battle_HealthPotion)
			{
				_buttonEffect[5].SetActive(false);
				TutorialManager.Instance.FinishTutorialHp();
			}
			else if (tutorialId == EnumTutorial.Battle_EnergyPotion)
			{
				_buttonEffect[6].SetActive(false);
				TutorialManager.Instance.FinishTutorialEnergy();
			}
		}
	}


	public void CloseEffectFor80()
	{
		EnumTutorialState state = PlayerInfo.Instance.GetTutorialState(EnumTutorial.Battle_HealthPotion);
		if (state == EnumTutorialState.Active)
		{
			MessageController.Instance.CloseCurMessage();
			_buttonEffect[5].SetActive(false);

			EnumTutorialState count = PlayerInfo.Instance.GetTutorialState(EnumTutorial.Battle_HealthPotion);
			count++;
			PlayerInfo.Instance.ChangeTutorialState(EnumTutorial.Battle_HealthPotion, count);
			if (count > EnumTutorialState.Finished)
			{
				TutorialManager.Instance.TryFinishTutorialLevel(EnumTutorial.Battle_HealthPotion);
			}
			else
			{
				PlayerInfo.Instance.ChangeTutorialState(EnumTutorial.Battle_HealthPotion, EnumTutorialState.Inactive); //inactive	
			}
		}
	}

	IEnumerator DelayHideEffectFor80()
	{
		yield return new WaitForSeconds(10f);
		CloseEffectFor80();
	}


	public void CloseEffectFor90()
	{
		EnumTutorialState state = PlayerInfo.Instance.GetTutorialState(EnumTutorial.Battle_EnergyPotion);
		if (state == EnumTutorialState.Active)
		{
			MessageController.Instance.CloseCurMessage();
			_buttonEffect[6].SetActive(false);

			EnumTutorialState count = PlayerInfo.Instance.GetTutorialState(EnumTutorial.Battle_EnergyPotion);
			count++;
			PlayerInfo.Instance.ChangeTutorialState(EnumTutorial.Battle_EnergyPotion, count);
			if (count > EnumTutorialState.Finished)
			{
				TutorialManager.Instance.TryFinishTutorialLevel(EnumTutorial.Battle_EnergyPotion);
			}
			else
			{
				PlayerInfo.Instance.ChangeTutorialState(EnumTutorial.Battle_EnergyPotion, 0); //inactive
			}
		}
	}

	IEnumerator DelayHideEffectFor90()
	{
		yield return new WaitForSeconds(10f);
		CloseEffectFor90();
	}


	IEnumerator DelayShowArrowFor30()
	{
		yield return new WaitForSeconds(5f);

		EnumTutorialState state = PlayerInfo.Instance.GetTutorialState(EnumTutorial.Battle_KillAnEnemy);
		if (state == EnumTutorialState.Active)
		{
			_buttonEffect[0].SetActive(true);
		}
	}
}
