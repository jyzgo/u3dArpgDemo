using UnityEngine;
using System.Collections;

public class FCStateDummy_meleeNormal : FCStateAgent
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
        _currentStateID = AIAgent.STATE.DUMMY;
    }

    IEnumerator STATE()
    {
        _inState = true;

        PlayAnimation();
        StateIn();

        _stateOwner.DummyTaskChange(FCCommand.CMD.STATE_ENTER);
        while (_inState)
        {
            _stateOwner.DummyTaskChange(FCCommand.CMD.STATE_UPDATE);
            yield return null;
        }

        StateOut();
        _stateOwner.DummyTaskChange(FCCommand.CMD.STATE_QUIT);
        _stateOwner.ChangeState();
    }
}
