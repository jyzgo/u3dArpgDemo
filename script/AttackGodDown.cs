using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class AttackGodDown : AttackBase
{
    private bool showGodDown = true;
    private float godTime = 10.0f;
   
    public float _currentScale = 1.0f;
    public float _maxScale = 1.3f;

    private bool _isInScale = false;
    private bool _inState = false;

    private EotData eotData;

    protected override void AniOver()
    {
        if (_currentState != AttackBase.ATTACK_STATE.ALL_DONE)
        {
            _nextAttackIdx = FCConst.UNVIABLE_ATTACK_INDEX;
            _currentState = AttackBase.ATTACK_STATE.ALL_DONE;
            _owner.ACOwner.ACStop();
            _owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
            _owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
            //revive will give player some seconds in god mode
            //_owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_GOD);
            _owner.ACOwner.ACRestoreToDefaultSpeed();
            _owner.AttackTaskChange(FCCommand.CMD.STATE_FINISH);

        }
    }

    protected override bool AKEvent(FC_KEY_BIND ekb, bool isPress)
    {
        return true;
    }

    public override void AttackEnter()
    {
        if (null != SkillData)
        {
            eotData = DataManager.Instance.GetEotDataByEotID(SkillData.CurrentLevelData.eotId);

            foreach (Eot eot in eotData.eotList)
            {
                if (godTime < eot.lastTime)
                {
                    godTime = eot.lastTime;
                }
            }

        }
        else
        {
            godTime = 10.0f;
        }

        base.AttackEnter();

        showGodDown = true;
        _owner.ACOwner._avatarController.GodDownColor(godTime);
        RunScaleChange();

    }
       

    public override void AttackQuit()
    {
        base.AttackQuit();
        

    }

    public override void AttackUpdate()
    {
        base.AttackUpdate();

        float currentAnimPercent = _owner.ACOwner.AniGetAnimationNormalizedTime();

        if (showGodDown && currentAnimPercent > 0.3f)
        {
            _owner.ACOwner.ACFire(this._fireInfos[0].FirePortIdx);
            showGodDown = false;
        }
    }


    public override void AniBulletIsFire()
    {
        base.AniBulletIsFire();
    }

    public void RunScaleChange()
    {
        _isInScale = true;
        _inState = true;

        StartCoroutine(STATE());
    }

    IEnumerator STATE()
    {
        ScaleTaskChange(FCCommand.CMD.STATE_ENTER);

        while (_inState)
        {
            ScaleTaskChange(FCCommand.CMD.STATE_UPDATE);

            yield return null;
        }

        ScaleTaskChange(FCCommand.CMD.STATE_QUIT);
    }

    private void ScaleTaskChange(FCCommand.CMD cmd)
    {
        if (cmd == FCCommand.CMD.STATE_ENTER)
        {
			AttackActive(true);
            _owner.EotAgentSelf.AddEot(eotData.eotList.ToArray());
            _owner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_EOT_GODDOWN);
            _owner.ACOwner.SkillGodDown = true;
            //Debug.Log("scale enter");
        }
        else if (cmd == FCCommand.CMD.STATE_UPDATE)
        {
            //Debug.Log("scale update");
            ScaleUpdate();
        }
        else if (cmd == FCCommand.CMD.STATE_QUIT)
        {
            //Debug.Log("scale quit");

            foreach(Eot eot in eotData.eotList)
            {
                _owner.EotAgentSelf.ClearEot(eot.eotType);
            }

            _owner.ACOwner.SkillGodDown = false;

            _owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_EOT_GODDOWN);
        }

    }

    private void ScaleUpdate()
    {
        godTime -= Time.deltaTime;

        if (godTime <= 0.0f)
        {
            _isInScale = false;
        }

        if (_isInScale)
        {
            _currentScale += Time.deltaTime;

            if (_currentScale >= _maxScale)
            {
                _currentScale = _maxScale;
            }
        }
        else
        {
            _currentScale -= Time.deltaTime;

            if (_currentScale <= 1)
            {
                _currentScale = 1;
                _inState = false;
            }
        }

        //Debug.Log("_currentScale = " + _currentScale);

        _owner.ACOwner.gameObject.transform.localScale = new Vector3(_currentScale, _currentScale, _currentScale);

    }


}
