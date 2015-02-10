using UnityEngine;
using System.Collections;

public class PassiveCriticalBeans : PassiveSkillBase {

	public int _maxChargeCount = 3;
	public float _coolDownTime = 1f;
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
        if (!_beginIncreaseDamage && skillID >= 0 && !_owner.ACOwner.AIUse.CurrentSkill.SkillModule._isNormalSkill)
        {
            string skillName = _owner.ACOwner.AIUse.CurrentSkill.SkillModule._skillName;

            if (_skillName != skillName)
            {
                foreach (string sn in _skillIgnore)
                {
                    if (sn == skillName)
                    {
                        return;
                    }
                }

                if (!_owner.ACOwner.AIUse.AttackCountAgent.SkillIsCD(_skillName))
                {
                    return;
                }

                if (_owner.ACOwner.SkillGodDown)
                {
                    return;
                }

                _chargeCount++;
                _chargeCount = Mathf.Clamp(_chargeCount, 0, _maxChargeCount);

                if (_chargeCount >= _maxChargeCount)
                {
                    _owner.ACOwner.SkillGodDownActive = true;
                }

            }
            else
            {
                _owner.ACOwner.SkillGodDownActive = false;
                _beginIncreaseDamage = true;
                _chargeCount = 0;
            }

            if (_battleCharEffect != null)
            {
                _battleCharEffect.ShowEffect(_chargeCount);
            }


            ///////////////////////////////////////////////////////////

            //if (_chargeCount >= _maxChargeCount)
            //{
            //    string skillName = _owner.ACOwner.AIUse.CurrentSkill.SkillModule._skillName;
            //    foreach (string sn in _skillIgnore)
            //    {
            //        if (sn == skillName)
            //        {
            //            return;
            //        }
            //    }

            //    if (skillName != _skillName)
            //    {
            //        return;
            //    }

            //    _owner.ACOwner.SkillGodDownActive = false;
            //    _beginIncreaseDamage = true;
            //    _chargeCount = 0;
            //    if (_battleCharEffect != null)
            //    {
            //        _battleCharEffect.ShowEffect(_chargeCount);
            //    }
            //}
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
			_beginIncreaseDamage = false;
		}
	}
	
	public void OnHitTarget(ActionController ac, bool fromSkill)
	{
        //if(!_owner.ACOwner.AIUse.AttackCountAgent.SkillIsCD(_skillName))
        //{
        //    return;
        //}

        //if (_owner.ACOwner.SkillGodDown)
        //{
        //    return;
        //}

        //int rank = PlayerInfo.Instance.GetSkillLevel(_skillName);

        //if (0 == rank)
        //{
        //    return;
        //}

        //if (_owner.beActive
        //    && !_beginIncreaseDamage
        //    && _cdTimeCount <= 0
        //    && _owner.ACOwner.AIUse.CurrentSkill != null
        //    && _owner.ACOwner.IsCriticalHit)
        //{
        //    _cdTimeCount = _coolDownTime;
        //    _chargeCount++;
        //    _chargeCount = Mathf.Clamp(_chargeCount, 0, _maxChargeCount);

        //    if (_chargeCount >= _maxChargeCount)
        //    {
        //        _owner.ACOwner.SkillGodDownActive = true;
        //    }
        //    if (_battleCharEffect != null)
        //    {
        //        _battleCharEffect.ShowEffect(_chargeCount);
        //    }
        //}
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
