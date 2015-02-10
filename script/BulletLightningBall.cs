using UnityEngine;
using System.Collections;

public class BulletLightningBall : FCBullet {

	System.Collections.Generic.List<ActionController> _savedTarget; // enter lightning range, but has no enough lightning for them now.
	System.Collections.Generic.List<BulletLightningBallChain> _freeChains;
	BulletLightningBallChain []_lightningChains;
	int _curLevelTargetCount;
	public int _maxTargetCount;
	public float _duration;
	public GameObject _chain;
	public Transform _core;
	public float _startDelay;
	
	public string _soundFx_start;
	public string _soundFx_loop;
	AudioSource _sfxAudioSource;
	
	protected override void Awake () {
		base.Awake();
		_isRangerBullet = true; // make me be enable to override function 'FireRange()'
		_savedTarget = new System.Collections.Generic.List<ActionController>();
		_freeChains = new System.Collections.Generic.List<BulletLightningBallChain>();
		// add pool for thunder chain.
		_lightningChains = new BulletLightningBallChain[_maxTargetCount];
		for(int i = 0;i < _maxTargetCount;++i) {
			GameObject go = GameObject.Instantiate(_chain) as GameObject;
			go.transform.parent = LevelManager.Singleton.BulletRoot;
			go.SetActive(false);
			BulletLightningBallChain blbc = go.GetComponent<BulletLightningBallChain>();
			blbc._pool = this;
			blbc._targetPart = _targetSolt;
			_lightningChains[i] = blbc;
		}
		Reset();
	}
	
	void Reset() {
		_savedTarget.Clear();
		_freeChains.Clear();
		for(int i = 0;i < _curLevelTargetCount;++i) {
			_lightningChains[i].End();
		}
	}
	
	public void AddFreeChain(BulletLightningBallChain chain) {
		_freeChains.Add(chain);
	}
	
	public bool AssignChainToNewTarget(BulletLightningBallChain chain) {
		ActionController ac = null;
		for(int i = _savedTarget.Count - 1;i >= 0; --i) {
			// clear dead enemys met in the target array.
			if(!_savedTarget[i].IsAlived) {
				_savedTarget.RemoveAt(i);
				continue;
			}
			// successed to assign new target.
			ac = _savedTarget[i];
			_savedTarget.RemoveAt(i);
			chain.Active(ac, this);
			return true;
		}
		return false;
	}
	
	protected override IEnumerator STATE ()
	{
		SoundManager.Instance.PlaySoundEffect(_soundFx_start, false);
		_sfxAudioSource = SoundManager.Instance.PlaySoundEffect(_soundFx_loop, true);
		Vector3 fwd = _owner.ThisTransform.forward;
		yield return new WaitForSeconds(_startDelay);
		float _timer = _duration;
		_damageReceiver.ActiveLogic();
		Reset();
		if(_moveAgent != null)
		{
			_moveAgent.SetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
			_moveAgent.CurrentSpeed = _hitInfo[0]._shotSpeed;
			_moveAgent.GoByDirection(ref fwd,99, true);
		}
		while(_timer >= 0.0f)
		{
			_timer -= Time.deltaTime;			
			yield return null;
		}
		// release all chains.
		foreach(BulletLightningBallChain blbc in _lightningChains) {
			blbc.gameObject.SetActive(false);
		}
		SoundManager.Instance.StopSoundEffect(_sfxAudioSource);
		Dead();
	}
	
	protected override void OnDestroy()
	{
		if(SoundManager.Instance != null)
		{
			SoundManager.Instance.StopSoundEffect(_sfxAudioSource);
		}
	}
	public override bool HandleCommand (ref FCCommand ewd)
	{
		switch(ewd._cmd)
		{
		case FCCommand.CMD.ATTACK_HIT_TARGET:
		{
			ActionController ac = ewd._param1 as ActionController;
			Assertion.Check(ac != null);
			AddNewEnemy(ac);
		}
			break;
		case FCCommand.CMD.ATTACK_OUT_OF_RANGE:
		{
			ActionController ac = ewd._param1 as ActionController;
			Assertion.Check(ac != null);
			// remove from waiting list.
			_savedTarget.Remove(ac);
			// remove from lightning chain.
			foreach(BulletLightningBallChain blbc in _lightningChains) {
				if(blbc._target == ac) {
					blbc.End();
					// seek new target.
					AssignChainToNewTarget(blbc);
					break;
				}
			}
		}
			break;
		default:
			break;
		}
		return true;
	}
	
	void AddNewEnemy(ActionController ac) {
		if(!_savedTarget.Contains(ac)) {
			_savedTarget.Add(ac);
			// give target damage.
			if(_freeChains.Count > 0 && AssignChainToNewTarget(_freeChains[0])) {
				_freeChains.RemoveAt(0);
			}
		}
	}
	
	public override void FireRanger (ActionController target, Transform firePoint, RangerAgent.FirePort rfp)
	{
		_duration = rfp._attribute1;
		_curLevelTargetCount = Mathf.RoundToInt(rfp._attribute2);
		_damageReceiver.SetRadius(rfp._rangeInfo._param1);
		RangerAgent.FireRangeInfo fri = rfp._rangeInfo;
		base.FireRanger(target, firePoint, rfp);
		_attackInfo._attackPoints = _owner.TotalAttackPoints;
		_attackInfo._criticalChance = _owner.Data.TotalCritRate;
		_attackInfo._criticalDamage = _owner.Data.TotalCritDamage;
        _attackInfo._skillTriggerChance = _owner.Data.TotalSkillTriggerRate;
        _attackInfo._skillAttackDamage = _owner.Data.TotalSkillAttackDamage;
		if(fri != null)
		{
			_damageScale = rfp.DamageScale;
			_attackInfo._effectTime = fri._effectTime;
			_attackInfo._damageScale = _damageScale;
		}
		if(_hitInfo.Length > 0) {
			Assertion.Check(_hitInfo.Length == 1);
			_attackInfo._hitType = _hitInfo[0]._hitType;
		}
		
		_target = null;
		_isFrom2P = _owner.IsClientPlayer;
		ThisObject.layer = (int)_faction+1;
		
		ThisTransform.localPosition = firePoint.position;
		
		_firePoint = firePoint;
		
		ActiveLogic(rfp);
	}
	
	public override void Dead()
	{
		if(_moveAgent != null)
		{
			_moveAgent.Stop();
			StopAllCoroutines();
		}
		base.Dead();
	}
}
