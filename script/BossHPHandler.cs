using UnityEngine;
using System.Collections;

public class BossHPHandler : MonoBehaviour
{
	public UILabel labelBossName;
	public UILabel labelShieldInfo;
	public Transform hpIndicator;
	public Transform hpIndicatorEffect;
	public TweenScale hpIndicatorAnim;
	public TweenAlpha hpIndicatorEffectAnim;
	public TweenScale hpIndicatorEffectAnim2;

	public UISprite shield;
	public UISprite shieldEffect;
	public TweenAlpha shieldEffectAnim;
	public TweenScale shieldChange;
	public Color[] shieldColors;

	public float increaseTime;
	public float decreaseTime;
	public AnimationCurve shieldBlinkSpeed = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	private bool _isDisplay = false;
	private bool _hasInvisibled = true;
	private float _hpPercent = 0.0f;

	private float _shieldPercent = 0.0f;

	void Awake()
	{
		UIManager.Instance.SetBossHPDisplay = OnBossHPDisplay;
		UIManager.Instance.SetBossHP = OnBossHPChanged;
		UIManager.Instance.SetBossShieldInfo = OnBossShieldRestoreTime;
		UIManager.Instance.SetBossShield = OnBossShieldChanged;

		Vector3 scale = hpIndicator.localScale;
		scale.x = 0.01f;
	}

	void Update()
	{
		if (!_isDisplay && !_hasInvisibled)
		{
			UIManager.Instance.CloseUI("BossUI");
			_hasInvisibled = true;
		}
	}

	void OnBossHPDisplay(bool display, int steps, string name)
	{
		if (display)
		{
			_isDisplay = true;
			UIManager.Instance.OpenUI("BossUI");
			labelBossName.text = name;
		}
		else
		{
			_isDisplay = false;
			_hasInvisibled = false;
		}
	}

	void OnBossHPChanged(float bossHP)
	{
		bossHP = Mathf.Clamp(bossHP, 0.001f, 1.0f);
		if (_hpPercent <= bossHP)
		{
			// increase hp.
			IncreaseHp(bossHP);
		}
		else
		{
			// decrease hp.
			DecreaseHp(bossHP);
		}
	}

	void IncreaseHp(float newHP)
	{
		hpIndicatorAnim.Reset();
		Vector3 scale = hpIndicator.localScale;
		scale.x = _hpPercent;
		hpIndicatorAnim.from = scale;
		scale.x = newHP;
		hpIndicatorAnim.to = scale;
		hpIndicatorAnim.duration = increaseTime;
		hpIndicatorAnim.Play(true);

		_hpPercent = newHP;
	}

	void DecreaseHp(float newHP)
	{
		hpIndicatorAnim.Reset();
		Vector3 scale = hpIndicator.localScale;
		scale.x = _hpPercent;
		hpIndicatorAnim.from = scale;
		scale.x = newHP;
		hpIndicatorAnim.to = scale;
		hpIndicatorAnim.duration = decreaseTime;
		hpIndicatorAnim.Play(true);

		hpIndicatorEffectAnim2.Reset();
		scale = hpIndicatorEffect.localScale;
		scale.x = _hpPercent;
		hpIndicatorEffectAnim2.from = scale;
		scale.x = newHP;
		hpIndicatorEffectAnim2.to = scale;
		hpIndicatorEffectAnim2.Play(true);

		hpIndicatorEffectAnim.Reset();
		hpIndicatorEffectAnim.Play(true);

		_hpPercent = newHP;
	}

	void OnBossShieldChanged(float bossShield)
	{
		bossShield = Mathf.Clamp01(bossShield);
		//		int percent = Mathf.RoundToInt(bossShield * 100.0f);

		int startIndex = Mathf.FloorToInt(bossShield * (shieldColors.Length - 1));
		int endIndex = Mathf.CeilToInt(bossShield * (shieldColors.Length - 1));
		float blend = bossShield * (shieldColors.Length - 1) - startIndex;
		Color lerp = Color.Lerp(shieldColors[startIndex], shieldColors[endIndex], blend);
		shield.color = lerp;
		shieldEffect.color = lerp;
		//_shieldEffectAnim.from = 0.5f * bossShield;
		//_shieldEffectAnim.to = bossShield;
		shieldEffectAnim.duration = shieldBlinkSpeed.Evaluate(bossShield);

		if (bossShield < _shieldPercent)
		{
			shieldChange.Reset();
			shieldChange.Play(true);
		}
		_shieldPercent = bossShield;
	}

	void OnBossShieldRestoreTime(int time)
	{
		string info = time.ToString();
		if (time < 0)
		{
			info = "";
		}
		if (labelShieldInfo.text != info)
		{
			labelShieldInfo.text = info;
		}
	}
}
