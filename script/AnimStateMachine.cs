using UnityEngine;
using System.Collections;

public class AnimStateMachine
{
    protected ActionController _actionCtrl;
    protected Animator _animator;

    public virtual void Enter(AniSwitch parameter) { }

    public void Update()
    {
        if (!_animator.IsInTransition(0))
        {
            OnUpdate();
        }
    }

    protected virtual void OnUpdate() { }

    public virtual void Init(ActionController actionCtrl, Animator animator)
    {
        _actionCtrl = actionCtrl;
        _animator = animator;
    }
}

public class NormalAnimStateMachine : AnimStateMachine
{
    public override void Enter(AniSwitch parameter)
    {
        _animator.SetInteger("state", parameter._parameter);
    }
}

public class MoveAnimStateMachine : AnimStateMachine
{
    public override void Enter(AniSwitch parameter)
    {
        _animator.SetInteger("state", 10 + parameter._parameter);
    }
}

public class AttackAnimStateMachine : AnimStateMachine
{
    int _weaponActivity;
    int _switchActivity;
    int _fireActivity;
    int _effectActivity;

    public override void Init(ActionController actionCtrl, Animator animator)
    {
        base.Init(actionCtrl, animator);
        // TODO: get hash code of parameters.
    }

    public override void Enter(AniSwitch parameter)
    {
        _animator.SetInteger("state", 20 + parameter._parameter);
        _weaponActivity = 0;
        _switchActivity = 0;
        _fireActivity = 0;
        _effectActivity = 0;
    }

    protected override void OnUpdate()
    {
        // get current parameter.
        float weaponActivity = _animator.GetFloat("weaponActivity");
        float switchActivity = _animator.GetFloat("switchActivity");
        float fireActivity = _animator.GetFloat("fireActivity");
        float effectActivity = _animator.GetFloat("effectActivity");
        // check event callback.

        if (weaponActivity > 0.5f && _weaponActivity == 0)
        {
            _weaponActivity = Mathf.RoundToInt(weaponActivity);
            _actionCtrl.AniActiveWeapon();
        }
        if (weaponActivity < 0.5f && _weaponActivity > 0)
        {
            _actionCtrl.AniDeActiveWeapon();
            _weaponActivity = 0;
        }
        if (switchActivity > 0.5f && _switchActivity == 0)
        {
            _switchActivity = Mathf.RoundToInt(switchActivity);
            _actionCtrl.AttackCanSwitch();
        }
        if (fireActivity > 0.5f && _fireActivity == 0)
        {
            _fireActivity = Mathf.RoundToInt(fireActivity);
            _actionCtrl.AniFireBullet(_fireActivity - 1);
        }
        if (fireActivity < 0.5f && _fireActivity > 0)
        {
            _fireActivity = 0;
        }
        if (effectActivity > 0.5f && _effectActivity == 0)
        {
            _effectActivity = Mathf.RoundToInt(effectActivity);
            _actionCtrl.AniPlayEffect(_effectActivity - 1);
        }
        if (effectActivity < 0.5f && _effectActivity > 0)
        {
            _actionCtrl.AniStopEffect(_effectActivity - 1);
            _effectActivity = 0;
        }
    }
}

public class SpecialAttackStateMachine : AnimStateMachine
{
    int _weaponActivity;
    int _switchActivity;
    int _fireActivity;
    int _effectActivity;

    public override void Enter(AniSwitch parameter)
    {
        _animator.SetInteger("state", 40 + parameter._parameter);
        _weaponActivity = 0;
        _switchActivity = 0;
        _fireActivity = 0;
        _effectActivity = 0;
    }
    protected override void OnUpdate()
    {
        // get current parameter.
        float weaponActivity = _animator.GetFloat("weaponActivity");
        float switchActivity = _animator.GetFloat("switchActivity");
        float fireActivity = _animator.GetFloat("fireActivity");
        float effectActivity = _animator.GetFloat("effectActivity");
        // check event callback.

        if (weaponActivity > 0.5f && _weaponActivity == 0)
        {
            _weaponActivity = Mathf.RoundToInt(weaponActivity);
            _actionCtrl.AniActiveWeapon();
        }
        if (weaponActivity < 0.5f && _weaponActivity > 0)
        {
            _actionCtrl.AniDeActiveWeapon();
            _weaponActivity = 0;
        }
        if (switchActivity > 0.5f && _switchActivity == 0)
        {
            _switchActivity = Mathf.RoundToInt(switchActivity);
            _actionCtrl.AttackCanSwitch();
        }
        if (fireActivity > 0.5f && _fireActivity == 0)
        {
            _fireActivity = Mathf.RoundToInt(fireActivity);
            _actionCtrl.AniFireBullet(_fireActivity - 1);
        }
        if (fireActivity < 0.5f && _fireActivity > 0)
        {
            _fireActivity = 0;
        }
        if (effectActivity > 0.5f && _effectActivity == 0)
        {
            _effectActivity = Mathf.RoundToInt(effectActivity);
            _actionCtrl.AniPlayEffect(_effectActivity - 1);
        }
        if (effectActivity < 0.5f && _effectActivity > 0)
        {
            _actionCtrl.AniStopEffect(_effectActivity - 1);
            _effectActivity = 0;
        }
    }
}

public class ChargeStateMachine : AnimStateMachine
{
    int _weaponActivity;
    int _switchActivity;
    int _fireActivity;
    int _effectActivity;

    public override void Enter(AniSwitch parameter)
    {
        _animator.SetInteger("state", 60 + parameter._parameter);
        _weaponActivity = 0;
        _switchActivity = 0;
        _fireActivity = 0;
        _effectActivity = 0;
    }

    protected override void OnUpdate()
    {
        // get current parameter.
        float weaponActivity = _animator.GetFloat("weaponActivity");
        float switchActivity = _animator.GetFloat("switchActivity");
        float fireActivity = _animator.GetFloat("fireActivity");
        float effectActivity = _animator.GetFloat("effectActivity");
        // check event callback.

        if (weaponActivity > 0.5f && _weaponActivity == 0)
        {
            _weaponActivity = Mathf.RoundToInt(weaponActivity);
            _actionCtrl.AniActiveWeapon();
        }
        if (weaponActivity < 0.5f && _weaponActivity > 0)
        {
            _actionCtrl.AniDeActiveWeapon();
            _weaponActivity = 0;
        }
        if (switchActivity > 0.5f && _switchActivity == 0)
        {
            _switchActivity = Mathf.RoundToInt(switchActivity);
            _actionCtrl.AttackCanSwitch();
        }
        if (fireActivity > 0.5f && _fireActivity == 0)
        {
            _fireActivity = Mathf.RoundToInt(fireActivity);
            _actionCtrl.AniFireBullet(_fireActivity - 1);
        }
        if (fireActivity < 0.5f && _fireActivity > 0)
        {
            _fireActivity = 0;
        }
        if (effectActivity > 0.5f && _effectActivity == 0)
        {
            _effectActivity = Mathf.RoundToInt(effectActivity);
            _actionCtrl.AniPlayEffect(_effectActivity - 1);
        }
        if (effectActivity < 0.5f && _effectActivity > 0)
        {
            _actionCtrl.AniStopEffect(_effectActivity - 1);
            _effectActivity = 0;
        }
    }
}

public class HurtAnimStateMachine : AnimStateMachine
{
    int _fireActivity;
    int _weaponActivity;
    int _switchActivity;

    public override void Enter(AniSwitch parameter)
    {
        _fireActivity = 0;
        _weaponActivity = 0;
        _switchActivity = 0;
        _animator.SetInteger("state", 80 + parameter._parameter);
    }

    protected override void OnUpdate()
    {
        // get current parameter.
        float weaponActivity = _animator.GetFloat("weaponActivity");
        float fireActivity = _animator.GetFloat("fireActivity");
        float switchActivity = _animator.GetFloat("switchActivity");

        // check event callback.
        if (fireActivity > 0.5f && _fireActivity == 0)
        {
            _fireActivity = Mathf.RoundToInt(fireActivity);
            _actionCtrl.AniFireBullet(_fireActivity - 1);
        }
        if (fireActivity < 0.5f && _fireActivity > 0)
        {
            _fireActivity = 0;
        }
        if (weaponActivity > 0.5f && _weaponActivity == 0)
        {
            _weaponActivity = Mathf.RoundToInt(weaponActivity);
            _actionCtrl.AniActiveWeapon();
        }
        if (weaponActivity < 0.5f && _weaponActivity > 0)
        {
            _actionCtrl.AniDeActiveWeapon();
            _weaponActivity = 0;
        }

        if (switchActivity > 0.5f && _switchActivity == 0)
        {
            _switchActivity = Mathf.RoundToInt(switchActivity);
            _actionCtrl.AttackCanSwitch();
        }
    }
}