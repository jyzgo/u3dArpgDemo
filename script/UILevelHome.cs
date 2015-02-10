using UnityEngine;
using System.Collections.Generic;

public class UILevelHome : MonoBehaviour
{
	public GameObject _moveBtn;
	public Transform _skillGroup;

	public UIButton _hpButton;
	public UIButton _mpButton;
	public UIImageButton _hcButton;
	public UIImageButton _pausedButton;

	public UILabel _battleTimer;

	void Awake()
	{
		if (Screen.width / (float)Screen.height < 1.34f)
		{
			_skillGroup.transform.localScale = Vector3.one * 0.75f;
		}
		else if (Screen.width / (float)Screen.height < 1.51f)
		{
			_skillGroup.transform.localScale = Vector3.one * 0.9f;
		}
	}

	void Start ()
	{
		LevelManager.Singleton._onPlayerInitialized = OnLoadMemberPlayerUI;
		LevelManager.Singleton._onPlayerDestroyed = OnDestroyMemberPlayerUI;
	}
	void OnDestroy()
	{
		if (LevelManager.Singleton != null)
		{
			LevelManager.Singleton._onPlayerInitialized = null;
			LevelManager.Singleton._onPlayerDestroyed = null;
		}
	}

	void OnInitialize()
	{
#if ((!UNITY_IPHONE && !UNITY_ANDROID) || UNITY_EDITOR)
		_moveBtn.SetActive(true);
#else
		_moveBtn.SetActive(false);
#endif

		UIManager.Instance.OpenUI("MessageController");
#if PVP_ENABLED
		if (GameManager.Instance.IsPVPMode)
		{
			_battleTimer.gameObject.SetActive(false);
		}
#endif
	}

	public void DisableButtons()
	{
		if (_hpButton != null)
			_hpButton.isEnabled = false;

		if (_mpButton != null)
			_mpButton.isEnabled = false;

		if (_hcButton != null)
			_hcButton.isEnabled = false;

		if (_pausedButton != null)
			_pausedButton.isEnabled = false;
	}
	void OnLoadMemberPlayerUI(int index)
	{
		int myIndex = MatchPlayerManager.Instance.GetPlayerIndex();
		if (index == myIndex)
		{
			int playerClass = (int)PlayerInfo.Instance.Role;
			HUDSkillHandler hudSkillHandler = GetComponent<HUDSkillHandler>();
			hudSkillHandler.SetSkillIcons(playerClass);
			return;
		}
	}

	void OnDestroyMemberPlayerUI(int index)
	{
	}

	void Update()
	{
		if (_battleTimer.gameObject.activeSelf)
		{
			_battleTimer.text = UIUtils.GetBattleTimerString(BattleSummary.Instance.TimeConsumed);
		}
	}
}
