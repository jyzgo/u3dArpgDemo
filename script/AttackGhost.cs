using UnityEngine;
using System.Collections;

public class AttackGhost : AttackNormal {

	public float _hideTime = 0.12f;
	public float _showTime = 0.44f;
	private int _step = 0;
	
	public void Hide()
	{
		_owner.ACOwner.ACEnableCollisionWithOtherACS(false);
		_owner.ACOwner._avatarController.ChangeMeshRenderers(false);
		_owner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_GOD);
	}
	
	public void Show()
	{
		_owner.ACOwner._avatarController.ChangeMeshRenderers(true);
		_owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_GOD);
	}
	public override void AttackEnter()
	{
		base.AttackEnter();
		_step = 0;
		_currentState = AttackBase.ATTACK_STATE.STEP_1;
		
		//BeginEffect();
	}
	
	public override void AttackEnd()
	{
		base.AttackEnd();
		_currentState = AttackBase.ATTACK_STATE.STEP_2;
	}
	
	public override void AttackQuit()
	{
		base.AttackQuit();
		if(_step == 1)
		{
			Show();
		}
	}
	
	public override void AttackUpdate()
	{
		base.AttackUpdate();
		if(_currentState == AttackBase.ATTACK_STATE.STEP_1)
		{
			
			if(_step == 0 && _owner.ACOwner.AniGetAnimationNormalizedTime() > _hideTime)
			{
				Hide();
				_step = 1;
			}
			
			
			if(_step == 1 && _owner.ACOwner.AniGetAnimationNormalizedTime() > _showTime)
			{
				Show();
				_step = 2;
			}		
		}
	}
}
