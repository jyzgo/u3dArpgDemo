using UnityEngine;
using System.Collections;

public class BulletLine : FCBullet {
	
	public float _range;
	public float _hurtStep;
	
	public BillboardLineRenderer _renderer;
	public Color []_levelColor;
	public float []_levelWidth;
	
	public string _soundFx;
	public Transform _lineTransform;
	AudioSource _sfxAudioSource;

	System.Collections.Generic.List<EnemyInRange> _enemys;
	
	class EnemyInRange {
		public ActionController _ac;
		public float _timer;
	};
	
	// physics related.
	int _mask;
	
	FCCommand _fastCommand;
	
	protected override void Awake () {
		base.Awake();
		_isRangerBullet = true; // make me be enable to override function 'FireRange()'
		_enemys = new System.Collections.Generic.List<EnemyInRange>();
		_mask = (1 << LayerMask.NameToLayer("WALL"));
		_mask |= (1 << LayerMask.NameToLayer("WALL_AIR"));
		_mask |= (1 << LayerMask.NameToLayer("GROUND"));
		
		_fastCommand = new FCCommand();
		_fastCommand._param1 = this;
		_fastCommand._cmd = FCCommand.CMD.HURT;
	}
	
	protected override IEnumerator STATE ()
	{
		_damageReceiver.ActiveLogic();
		if(string.IsNullOrEmpty(_soundFx)) {
			_sfxAudioSource = SoundManager.Instance.PlaySoundEffect(_soundFx, true);
		}
		Transform leftHand = _owner._avatarController.GetSlotNode(EnumEquipSlot.weapon_left);
		Transform rightHand = _owner._avatarController.GetSlotNode(EnumEquipSlot.weapon_right);
		
		while(!_isDead) {
			// ami to forward.
			Vector3 origin = (leftHand.position + rightHand.position) / 2.0f;
			Vector3 dir = _owner.ThisTransform.forward;
			ThisTransform.position = origin;
			ThisTransform.LookAt(origin + dir * _range);
			Vector3 scale = _lineTransform.localScale;
			scale.z = _range;
			RaycastHit hitInfo;
			if(Physics.Raycast(origin, dir, out hitInfo, _range, _mask)) {
				scale.z = (hitInfo.point - origin).magnitude;
			}
			_lineTransform.localScale = scale;
			// hurt enemys.
			foreach(EnemyInRange e in _enemys) {
				e._timer -= Time.deltaTime;
				if(e._timer <= 0.0f && e._ac.IsAlived) {
					_fastCommand._param2 = e._ac;
					CommandManager.Instance.SendFast(ref _fastCommand, e._ac);
					IsHitSomeone(_owner, this, _damageType, e._ac);
					e._timer = _hurtStep;
				}
			}
			yield return null;
		}
		
		Dead();
	}
	
	public override bool HandleCommand (ref FCCommand ewd)
	{
		switch(ewd._cmd)
		{
		case FCCommand.CMD.ATTACK_HIT_TARGET:
		{
			ActionController ac = ewd._param1 as ActionController;
			Assertion.Check(ac != null);
			EnemyInRange info = new EnemyInRange();
			info._ac = ac;
			info._timer = 0.0f;
			_enemys.Add(info);
		}
			break;
		case FCCommand.CMD.ATTACK_OUT_OF_RANGE:
		{
			ActionController ac = ewd._param1 as ActionController;
			Assertion.Check(ac != null);
			for(int i = _enemys.Count - 1;i >= 0;--i) {
				if(_enemys[i]._ac == ac) {
					_enemys.RemoveAt(i);
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
	
	public override void FireRanger (ActionController target, Transform firePoint, RangerAgent.FirePort rfp)
	{
		int index = Mathf.RoundToInt(rfp._attribute1);
		_renderer.SetColor(_levelColor[index]);
		Vector3 scale = _lineTransform.localScale;
		scale.x = _levelWidth[index];
		_lineTransform.localScale = scale;
		
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
	
	protected override void OnDestroy()
	{
		if(SoundManager.Instance != null)
		{
			SoundManager.Instance.StopSoundEffect(_sfxAudioSource);
		}
	}
	
	public override void Dead ()
	{
		SoundManager.Instance.StopSoundEffect(_sfxAudioSource);
		_enemys.Clear();
		base.Dead ();
	}
}
