using UnityEngine;
using System.Collections;

public class AttackRevive : AttackBase {

    private bool showRevive = true;

	protected override void AniOver()
	{
		if(_currentState != AttackBase.ATTACK_STATE.ALL_DONE)
		{
			_nextAttackIdx = FCConst.UNVIABLE_ATTACK_INDEX;
			_currentState = AttackBase.ATTACK_STATE.ALL_DONE;
			_owner.ACOwner.ACStop();
			_owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
			_owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
            _owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_GOD);
			//revive will give player some seconds in god mode
			_owner.ACOwner.ACRestoreToDefaultSpeed();
			_owner.ReviveTaskChange(FCCommand.CMD.STATE_DONE);
			
		}
	}
	
	protected override bool AKEvent(FC_KEY_BIND ekb, bool isPress)
	{
		return true;
	}
	
	public override void AttackEnter()
	{
		base.AttackEnter();
		_owner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
		_owner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_GOD);
		_currentState = AttackBase.ATTACK_STATE.STEP_1;
        showRevive = true;
	}

    public override void AttackEnd()
    {
        base.AttackEnd();

        _owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
        _owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_GOD);
    }

    public override void AttackQuit()
    {
        base.AttackQuit();

       

        //StartCoroutine(AddBulletFrie());
        
    }

    public override void AttackUpdate()
    {
        base.AttackUpdate();

        float currentAnimPercent = _owner.ACOwner.AniGetAnimationNormalizedTime();

        if (showRevive && currentAnimPercent > 0.6f)
        {
            _owner.ACOwner.ACFire(this._fireInfos[0].FirePortIdx);
            showRevive = false;
        }
    }


    //IEnumerator AddBulletFrie()
    //{
    //    _owner.ACOwner.ACFire(this._fireInfos[0].FirePortIdx);

    //    yield return new WaitForSeconds(1);

    //    _owner.HurtAgent.LifeTimeIsOver ();
    //    _owner.ACOwner.ACFire(this._fireInfos[1].FirePortIdx);

    //}


	
	public override void AniBulletIsFire()
	{
		base.AniBulletIsFire();
	}
}
