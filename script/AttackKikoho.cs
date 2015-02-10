using UnityEngine;
using System.Collections;

public class AttackKikoho : AttackContinuity {
	
	public float _rotSpeed;
	public string _soundFx;
	public AudioSource _sfxAudioSource = null;
	
	public override void AttackEnd()
	{
		base.AttackEnd();
		Assertion.Check(_fireInfos.Length > 0);
		_owner.ACOwner.ACKillBulletByFirePort(this._fireInfos[0].FirePortIdx);
	}
	
	public override void AttackEnter()
	{
		base.AttackEnter();
		_sfxAudioSource = SoundManager.Instance.PlaySoundEffect(_soundFx);
		_owner.ACOwner.CurrentAngleSpeed = _rotSpeed;
		_timeCount = 999f;
		
		if(_owner.ACOwner.IsClientPlayer)
		{
			_owner._updateAttackRotation = true;
		}
		//_owner.ACOwner.ACFire(FirePortIdx);
	}
	
	public override bool DirectionKeyEvent(Vector3 direction, bool isPress)
	{
		if(_owner.ACOwner.IsPlayerSelf && isPress)
		{
			_owner.ACOwner.ACRotateTo(direction,-1,true);	
		}
		else
		{
			_owner.ACOwner.ACSTopRotate();
		}
		return true;	
	}
	
	public override void AttackQuit()
	{
		_owner.ACOwner.ACRevertToDefalutMoveParams();
		base.AttackQuit();
		Assertion.Check(_fireInfos.Length > 0);
		_owner.ACOwner.ACKillBulletByFirePort(this._fireInfos[0].FirePortIdx);
		
		if(_sfxAudioSource != null) {
			SoundManager.Instance.StopSoundEffect(_sfxAudioSource);
		}
		
		
	}
	
	public override void AniBulletIsFire()
	{
		base.AniBulletIsFire();
		//_energyCostPhase = true;
		_timeCount = _timeLast;
	}
}
