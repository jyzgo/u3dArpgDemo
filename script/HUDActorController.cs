using UnityEngine;
using System.Collections;

public class HUDActorController : MonoBehaviour
{
	// Portrait
	public UITexture portrait;

	// HP
	public UISlider hpEffect;
	public UISprite hp;
	public float _reduceSpeed;
	public float shadingTime = 0.6f;

	private bool _shading = false;

	// XP
	public UISlider xpSlider;

	// Energy
	public UISlider energySlider;

	// Level
	public UILabel labelLevel;

	public GameObject dyingEffect;

	public UISprite portraitFrame;

	ActionController _ac;

	// Timer
	float _dyingElapsedTime;
	bool _isDying;

	// Use this for initialization
	void Start()
	{
		xpSlider.sliderValue = PlayerInfo.Instance._currentXpPrecent;
		labelLevel.text = PlayerInfo.Instance.CurrentLevel.ToString();

		dyingEffect.SetActive(false);
		_dyingElapsedTime = 0f;
		_isDying = false;
	}

	public void RegisterEvents(ActionController ac)
	{
		_ac = ac;
		if (_ac != null)
		{
			_ac._hpChangeMessage += OnHPChanged;

			_ac._energyChangeMessage = OnEnergyChanged;

			PlayerInfo.Instance.OnXpPerCentChange += OnXpChanged;

			PlayerInfo.Instance.OnLevelUp += OnLevelUp;
		}
	}

	void UnregisterEvents()
	{
		PlayerInfo.Instance.OnXpPerCentChange -= OnXpChanged;

		PlayerInfo.Instance.OnLevelUp -= OnLevelUp;
	}

	void OnDestroy()
	{
		UnregisterEvents();
	}

	// Update is called once per frame
	void Update()
	{
		if (_ac != null)
		{
			UpdateDyingEffect();
			SliderReduceShading();
		}
	}

	void UpdateDyingEffect()
	{
		if (_isDying)
		{
			_dyingElapsedTime += Time.deltaTime;

			if (_dyingElapsedTime >= 1.5f)
			{
				SoundManager.Instance.PlaySoundEffect("warrior_heartbreak");

				_dyingElapsedTime = 0f;
			}
		}
	}

	void OnHPChanged(float deltaPercent)
	{
		if (deltaPercent < 0.0f)
		{
			_shading = true;
			_reduceSpeed = deltaPercent / shadingTime;

			ActionController ac = ObjectManager.Instance.GetMyActionController();
			float hurt = -deltaPercent * ac.Data.TotalHp;
			BattleSummary.Instance.DamageTaken = (int)hurt;


			if (_ac.IsAlived && _ac.HitPointPercents < 0.2f)
			{
				TutorialManager.Instance.ReceiveStartTutorialEvent(EnumTutorial.Battle_HealthPotion);

				dyingEffect.SetActive(true);
				_isDying = true;
			}
		}
		else
		{
			hpEffect.sliderValue = _ac.HitPointPercents;
		}

		if (_ac.IsAlived && _ac.HitPointPercents > 0.2f)
		{
			dyingEffect.SetActive(false);
			_isDying = false;
			TutorialManager.Instance.GetHUDSkillHandler().CloseEffectFor80();
		}

		hp.fillAmount = _ac.HitPointPercents;

		if (Mathf.Abs(_ac.HitPointPercents - 1.0f) <= float.Epsilon)
		{
			hpEffect.sliderValue = 1.0f;
		}
	}

	void SliderReduceShading()
	{
		if (_shading)
		{
			hpEffect.sliderValue = hpEffect.sliderValue + _reduceSpeed * Time.deltaTime;

			if (hpEffect.sliderValue < hp.fillAmount)
			{
				_shading = false;
			}
		}
	}

	public void OnEnergyChanged(float percent)
	{
		if (energySlider != null)
		{
			energySlider.sliderValue = percent;


			if (percent < 0.2f)
			{
				TutorialManager.Instance.ReceiveStartTutorialEvent(EnumTutorial.Battle_EnergyPotion);
			}
			else
			{
				TutorialManager.Instance.GetHUDSkillHandler().CloseEffectFor90();
			}
		}
	}


	public void OnXpChanged(float percent)
	{
		xpSlider.sliderValue = percent;
	}


	void OnLevelUp(int level)
	{
		labelLevel.text = level.ToString();
	}
}
