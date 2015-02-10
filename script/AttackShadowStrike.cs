using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackShadowStrike : AttackBase {

	public int _strikeCountMax = 12;
	public int _strikeCountMin = 5;
	public float _lastTime = 1f;
	public float _skrikeCoolDownTime = 0.2f;
	public float _skrikeCoolDownTimeMin = 0.1f;
	public float _cdTimeChangeSpeed = 20f;
	public float _cdTimeRecoverSpeed = 1f;
	public CapsuleCollider _colliderRangeToStrike = null;
	
	public Transform _bulletLauncher = null;
	
	public string _battleEffectPrefabName = null; //the name of passive effect prefab
	private BattleCharEffect _battleCharEffect = null; //the controller of effect
	
	public BattleCharEffect BattleBaseEffect
	{
		get
		{
			return _battleCharEffect;
		}
	}
	
	public int _countMaxForShowBolt = 2;
	protected int _countForBolt = 0;
	public int _boltMaxCount = 6;
	protected int _boltCount = 0;
	public float _beginCountTime = 0.2f;
	protected float _currentCDCounter = 0;
	protected float _currentCDTime = 0.2f;
	protected float _lifeCount = 0;
	protected int _strikeCount = 0;
	
	public float _delayToNextTime = 0.2f;
	protected bool _inAttackDelay = false;
	protected float _delayTimeCount = 0;
	protected int _shadowStep = 0;
	
	protected bool _needTips = true;
	
	public class HitTargetInfo
	{
		public ActionController _target = null;
		public int _strikeCount = 0;
		public int GetFinalWeight(float radius,ActionController owner)
		{
			int ret = 0;
			ret += _strikeCount*100;
			float ra = ((_target.ThisTransform.localPosition - owner.ThisTransform.localPosition).magnitude - _target.BodyRadius)/radius;
			ra = Mathf.Clamp(ra, 0, 1f);
			ret += (int)(75f*(1f-ra));
			ret += Random.Range(0,25);
			if(!_target.IsAlived)
			{
				ret += 10000;
			}
			return ret;
		}
	}
	
	protected List<HitTargetInfo> _targetList = new List<HitTargetInfo>();
	
	protected override void FirstInit ()
	{
		base.FirstInit ();
		_colliderRangeToStrike.enabled = false;
		Assertion.Check(!string.IsNullOrEmpty(_battleEffectPrefabName));
		//load prefeb
		
	}
	public override void Init(FCObject owner)
	{
		base.Init(owner);
		if(_battleCharEffect == null)
		{
			GameObject battleEffectPrefab = InJoy.AssetBundles.AssetBundles.Load(_battleEffectPrefabName, typeof(GameObject)) as GameObject;
		//create instance
			GameObject instance = GameObject.Instantiate(battleEffectPrefab) as GameObject;
			instance.transform.parent = _owner.ACOwner.ThisTransform;
			instance.transform.localPosition = Vector3.zero;
			instance.transform.localRotation = Quaternion.identity;
			instance.transform.localScale = new Vector3(1f, 1f, 1f);
			//get component
			_battleCharEffect = instance.GetComponentInChildren<BattleCharEffect>();
			if (_battleCharEffect != null)
			{
				_battleCharEffect.PrepareEffect();
			}
		}
	}
	
	protected override void AniOver ()
	{
		if(_shadowStep == 0)
		{
			_owner.ACOwner.ACHide();
			_shadowStep = 1;
			_strikeCount++;
			_lifeCount = 0;
			//start effect
			_battleCharEffect.ShowStartEffect(true);
			_battleCharEffect.ResetLocation(_owner.ACOwner.ThisTransform.position);
			_battleCharEffect.LockLocation(true);
			
		}
		else if(_shadowStep == 3)
		{
			AttackEnd();
		}
	}
	
	public override void AttackEnter ()
	{
		base.AttackEnter ();
		_currentCDTime = _skrikeCoolDownTime;
		_currentCDCounter = _currentCDTime - 0.1f;
		_lifeCount = 0;
		_strikeCount = 0;
		_targetList.Clear();
		ThisObject.layer = (int)_owner.ACOwner.Faction+1;
		_colliderRangeToStrike.enabled = true;
		if(_fireInfos != null && _fireInfos.Length != 0)
		{
			_owner.ACOwner.ACGetFirePort(FirePortIdx).FirePoints[0] = _bulletLauncher;
			//_owner.ACOwner.ACGetFirePort(FirePortIdx)._shootByFirePointDirection = true;
			_owner.ACOwner.ACGetFirePort(FirePortIdx)._shootByFirePointDirection = false;
		}
		_owner.ACOwner.FireTarget = null;
		
		_owner.ACOwner.ACSetAvoidPriority(FCConst.HILL_WEIGHT);
		
		_countForBolt = _countMaxForShowBolt-1;
		_boltCount = 0;
		_delayTimeCount = _delayToNextTime;
		_owner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN);
		_shadowStep = 0;
	}
	
	public ActionController GetFireTarget()
	{
		ActionController newAC = null;
		HitTargetInfo tt = null;
		int weight = 99999;
		foreach(HitTargetInfo hti in _targetList)
		{
			int ret = hti.GetFinalWeight(_colliderRangeToStrike.radius, _owner.ACOwner);
			if(ret < weight)
			{
				weight = ret;
				tt = hti;
			}
		}
		if(tt != null)
		{
			newAC = tt._target;
			tt._strikeCount++;
		}
		return newAC;
	}
	protected Vector3 GetFireDirection()
	{
		Vector3 newDirection = _owner.ACOwner.ThisTransform.forward;
		HitTargetInfo tt = null;
		int weight = 99999;
		foreach(HitTargetInfo hti in _targetList)
		{
			int ret = hti.GetFinalWeight(_colliderRangeToStrike.radius, _owner.ACOwner);
			if(ret < weight)
			{
				weight = ret;
				tt = hti;
			}
		}
		if(tt != null)
		{
			Vector3 dir = (tt._target.ThisTransform.localPosition - _owner.ACOwner.ThisTransform.localPosition);
			dir.y =0;
			dir.Normalize();
			if(dir != Vector3.zero)
			{
				newDirection = dir;
			}
		}
		else
		{
			int angle = Random.Range(0,360);
			Vector3 dir = Quaternion.Euler(new Vector3(0,angle,0))*Vector3.forward;
			newDirection = dir;
		}
		return newDirection;
	}
	
	public override void AttackUpdate ()
	{
		base.AttackUpdate ();
		if(_shadowStep == 0)
		{
			_lifeCount += Time.deltaTime;
			if(_lifeCount >= _beginCountTime)
			{
				_owner.ACOwner.ACHide();
				_shadowStep = 1;
				_strikeCount++;
				_lifeCount = 0;
				//start effect
				_battleCharEffect.ShowStartEffect(true);
				_battleCharEffect.ResetLocation(_owner.ACOwner.ThisTransform.position);
				_battleCharEffect.LockLocation(true);
				
			}
		}
		else if(_shadowStep == 1)
		{
			_lifeCount += Time.deltaTime;
			HitTargetInfo tt = null;
			foreach(HitTargetInfo hti in _targetList)
			{
				if(hti._target == null || !hti._target.IsAlived  )
				{
					tt = hti;
				}
			}
			if(tt != null)
			{
				_targetList.Remove(tt);
			}
			if(_lifeCount >= _lastTime && _strikeCount >= _strikeCountMin)
			{
				if(_strikeCount >= _strikeCountMax && _boltCount >= _boltMaxCount)
				{
					_attackCanSwitch = true;
					_shouldGotoNextHit = true;
					_inAttackDelay = true;
					_shadowStep = 2;
				}
				else
				{
					_shouldGotoNextHit = false;
					_shadowStep = 3;
					_owner.ACOwner.ACShow();
					_owner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.shadow_show_end;
					_owner.ACOwner.ACPlayAnimation(_owner.AnimationSwitch);
					_boltCount = 0;
				}
				
			}
			else
			{
				_currentCDCounter += Time.deltaTime;
				if(_currentCDCounter >= _currentCDTime)
				{
					_currentCDCounter -= _currentCDTime;
					//_bulletLauncher.forward = GetFireDirection();
					_owner.ACOwner.FireTarget = GetFireTarget();
					_owner.ACOwner.ACFire(FirePortIdx);
					//_owner.ACOwner.FireTarget = null;
					_strikeCount++;
				}
			}
		}
		else if(_shadowStep == 2)
		{
			if(_delayTimeCount >0)
			{
				_delayTimeCount -= Time.deltaTime;
				if(_delayTimeCount <= 0)
				{
					AttackEnd();
				}
			}
		}
		/*Vector2 radius = Random.insideUnitCircle*0.5f;
		Vector3 r3 = Vector3.zero;
		r3.x += radius.x;
		r3.z += radius.y;
		_owner.ACOwner.SelfMoveAgent.Move(r3);*/
	}
	
	public override void AttackQuit ()
	{
		base.AttackQuit ();
		_colliderRangeToStrike.enabled = false;
		ThisObject.layer = FCConst.LAYER_DEFAULT;
		_targetList.Clear();
		_owner.ACOwner.FireTarget = null;
		_owner.ACOwner.ACSetAvoidPriority(_owner._bodyMass);
		_owner.ACOwner.ACShow();
		

		
		//bolt count is not max, play bad end effect
		if(_boltCount < _boltMaxCount)
		{
			_battleCharEffect.ShowEffect(0);
			_battleCharEffect.ShowStartEffect(false);			
			_battleCharEffect.ShowSpecialEndEffect(0);
		}
		
		_owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN);
	}
	void OnTriggerEnter(Collider other)
	{
		ActionController ac = ActionControllerManager.Instance.GetACByCollider(other);
		if(ac != null && ac.IsAlived)
		{
			bool ret = false;
			foreach(HitTargetInfo hti in _targetList)
			{
				if(hti._target == ac)
				{
					ret = true;
					break;
				}
			}
			if(!ret)
			{
				HitTargetInfo th = new HitTargetInfo();
				th._target = ac;
				th._strikeCount = 0;
				_targetList.Add(th);
			}
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		ActionController ac = ActionControllerManager.Instance.GetACByCollider(other);
		if(ac != null)
		{
			HitTargetInfo tt = null;
			foreach(HitTargetInfo hti in _targetList)
			{
				if(hti._target == ac)
				{
					tt = hti;
				}
			}
			if(tt != null)
			{
				_targetList.Remove(tt);
			}
		}
	}
	
	protected override bool AKEvent (FC_KEY_BIND ekb, bool isPress)
	{
		if(ekb != FC_KEY_BIND.NONE && ekb != FC_KEY_BIND.DIRECTION && isPress)
		{
			ekb = _currentBindKey;
		}
		base.AKEvent(ekb,isPress);
		if(_shadowStep == 1 && ekb == _currentBindKey)
		{
			if(isPress)
			{
				_countForBolt ++;
			}
			if(_countForBolt >= _countMaxForShowBolt)
			{
				_boltCount++;
				if(_boltCount >= _boltMaxCount)
				{
					_boltCount = _boltMaxCount;
					_strikeCount = _strikeCountMax;
					
				}
				UIBattleSkillTips.Instance.NextStar();
				_battleCharEffect.ShowEffect(_boltCount);
				_countForBolt = 0;
			}
			if(isPress)
			{
				_currentCDTime -= _cdTimeChangeSpeed * Time.deltaTime;
				_currentCDTime = Mathf.Clamp(_currentCDTime, _skrikeCoolDownTimeMin, _skrikeCoolDownTime);
			}
			else
			{
				_currentCDTime += _cdTimeRecoverSpeed * Time.deltaTime;
				_currentCDTime = Mathf.Clamp(_currentCDTime, _skrikeCoolDownTimeMin, _skrikeCoolDownTime);
			}
		}
		return true;
	}
	
}
