using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class AttackStorm : AttackBase
{
    public FC_CHARACTER_EFFECT partcileEffect = FC_CHARACTER_EFFECT.WARRIOR_SPIN_END;

    public GlobalEffectInfo stormEffect;

    protected override void AniOver()
    {
        if (_currentState != AttackBase.ATTACK_STATE.ALL_DONE)
        {
            _nextAttackIdx = FCConst.UNVIABLE_ATTACK_INDEX;
            _currentState = AttackBase.ATTACK_STATE.ALL_DONE;

            _owner.AttackTaskChange(FCCommand.CMD.STATE_FINISH);
            _owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
            _owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
            _owner.ACOwner.ACStop();
            _owner.ACOwner.ACRestoreToDefaultSpeed();
        }
    }


    public override void AttackEnter()
    {
        base.AttackEnter();

        //CharacterEffectManager.Instance.PlayEffect(partcileEffect, _owner.ACOwner._avatarController, -1);
    }

    public override void ClearEffect()
    {
        //StopEffect();
    }

    protected override void UpdateBindEffects()
    {
        //play and stop effect depends on the time
        float currentAnimPercent = _owner.ACOwner.AniGetAnimationNormalizedTime();
        if (_attackActiveCount > 0)
        {
            currentAnimPercent += _attackActiveCount - 1;
        }
        int offset = _bindEffects.Length / 2;
        UpdataGlobalEffect(currentAnimPercent);
    }


    private void StopEffect()
    {
        CharacterEffectManager.Instance.StopEffect(partcileEffect, _owner.ACOwner._avatarController, -1);
    }


    private void UpdataGlobalEffect(float currentAnimPercent)
    {
        if (null != stormEffect)
        {
            if (stormEffect.IsPlaying)
            {
                //is playing, stop it?
                if (currentAnimPercent > stormEffect.endPercent)
                {
                    stormEffect.IsPlaying = false;
                    stormEffect.HasPlayed = false;
                }
            }
            else if (!stormEffect.HasPlayed)
            {
                if (currentAnimPercent >= stormEffect.startPercent && currentAnimPercent < stormEffect.endPercent)
                {

                    Vector3 pos = _owner.ACOwner.ThisTransform.position;
                    GlobalEffectManager.Instance.PlayEffect(stormEffect.globalEffect, pos);

                    stormEffect.IsPlaying = true;
                    stormEffect.HasPlayed = true;
                }

            }
        }

    }
}
