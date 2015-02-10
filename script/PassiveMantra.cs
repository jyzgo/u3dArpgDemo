using UnityEngine;
using System.Collections;

public class PassiveMantra : PassiveSkillBase {

	public int _maxChargeCount = 3;
	public float _coolDownTime = 1f;
	public float _damageInCrease = 0.4f;
	public string[] _skillIgnore;
	
	protected float _cdTimeCount = 0;
	protected int _chargeCount = 0;
	
	protected bool _beginIncreaseDamage = false;

	public string _battleEffectPrefabName = null; //the name of passive effect prefab
	private BattleCharEffect _battleCharEffect = null; //the controller of effect
	
	protected override void OnInit()
	{
		if(_owner.ACOwner.IsPlayer)
		{
            //_damageInCrease = _owner._percents;
            //_maxChargeCount = (int)_owner._trueValue;
		}
		_coolDownTime = _owner.coolDownTimeMax;
		_cdTimeCount = 0;
		_chargeCount = 0;
		_owner.ACOwner._onSkillEnter += OnSkillEnter;
		_owner.ACOwner._onSkillQuit += OnSkillQuit;
		_owner.ACOwner._onHitTarget += OnHitTarget;
		_owner.ACOwner._onShowBody += OnShowBody;
		
		
		//get battle char effect and prepare it
		//check effect prefeb name
		Assertion.Check(!string.IsNullOrEmpty(_battleEffectPrefabName));
		//load prefeb
		GameObject battleEffectPrefab = InJoy.AssetBundles.AssetBundles.Load(_battleEffectPrefabName, typeof(GameObject)) as GameObject;
		//create instance
		GameObject instance = GameObject.Instantiate(battleEffectPrefab) as GameObject;
		instance.transform.parent = _owner.ACOwner.ThisTransform;
		instance.transform.localPosition = Vector3.zero;
		instance.transform.localRotation = Quaternion.identity;
		instance.transform.localScale = new Vector3(1.5f, 1f, 1.5f);
		//get component
		_battleCharEffect = instance.GetComponentInChildren<BattleCharEffect>();
		if (_battleCharEffect != null)
		{
			_battleCharEffect.PrepareEffect();
		}
		
		StartCoroutine(STATE());
	}
	
	public void OnShowBody(bool show)
	{
		_battleCharEffect.Show(show);
	}
	public void OnSkillEnter(int skillID)
	{
		if(!_beginIncreaseDamage && skillID >=0 && !_owner.ACOwner.AIUse.CurrentSkill.SkillModule._isNormalSkill)
		{
			if(_chargeCount >= _maxChargeCount)
			{
				string skillName = _owner.ACOwner.AIUse.CurrentSkill.SkillModule._skillName;
				foreach(string sn in _skillIgnore)
				{
					if(sn == skillName)
					{
						return;
					}
				}
				_owner.ACOwner.AIUse.AddDamageScale += _damageInCrease * _maxChargeCount;
				_beginIncreaseDamage = true;
				_chargeCount = 0;
				if(_battleCharEffect != null)
				{
					_battleCharEffect.ShowEffect(_chargeCount);
				}
			}
		}
		else
		{
			OnSkillQuit(skillID);
		}
	}
	
	public void OnSkillQuit(int skillID)
	{
		if(_beginIncreaseDamage)
		{
			_owner.ACOwner.AIUse.AddDamageScale -= _damageInCrease * _maxChargeCount;
			_beginIncreaseDamage = false;
		}
	}
	
	public void OnHitTarget(ActionController ac, bool fromSkill)
	{
		if(_owner.beActive 
			&& !_beginIncreaseDamage
			&& _cdTimeCount <=0 
			&& _owner.ACOwner.AIUse.CurrentSkill != null
			&& _owner.ACOwner.IsCriticalHit)
		{
			_cdTimeCount = _coolDownTime;
			_chargeCount++;
			_chargeCount = Mathf.Clamp(_chargeCount, 0, _maxChargeCount);
			if(_battleCharEffect != null)
			{
				_battleCharEffect.ShowEffect(_chargeCount);
			}
		}
	}
	private IEnumerator STATE()
	{
		while(true)
		{
			if(_cdTimeCount > -1)
			{
				_cdTimeCount -= Time.deltaTime;
			}
			yield return null;
		}
	}
}
