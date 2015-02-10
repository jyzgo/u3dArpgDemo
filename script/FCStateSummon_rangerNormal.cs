using UnityEngine;
using System.Collections;

public class FCStateSummon_rangerNormal : FCStateAgent
{
    public override void Init(AIAgent owner)
    {
        base.Init(owner);
    }

    public override void Run()
    {
        StartCoroutine(STATE());
    }

    void Awake()
    {
        _currentStateID = AIAgent.STATE.SUMMON;
    }

    IEnumerator STATE()
    {
        _inState = true;

        _stateOwner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
        _stateOwner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
        _stateOwner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN);
        _stateOwner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_RIGIDBODY2);

        _stateOwner.SummonTaskChange(FCCommand.CMD.STATE_ENTER);
        float timeLast = _stateOwner.TimeForIdleThisTime;

        PlayAnimation();
        StateIn();
        while (_inState)
        {
            _stateOwner.SummonTaskChange(FCCommand.CMD.STATE_UPDATE);
            if (timeLast > 0)
            {
                timeLast -= Time.deltaTime;
            }
            else
            {
                _stateOwner.SummonTaskChange(FCCommand.CMD.STATE_DONE);

                _stateOwner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
                _stateOwner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
                _stateOwner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN);
                _stateOwner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_RIGIDBODY2); 
            }
            StateProcess();
            yield return null;
        }
        StateOut();
        _stateOwner.SummonTaskChange(FCCommand.CMD.STATE_QUIT);
        _stateOwner.ChangeState();
    }
}
