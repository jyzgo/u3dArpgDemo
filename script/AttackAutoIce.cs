using UnityEngine;
using System.Collections;

public class AttackAutoIce : AttackNormal {
	
	public FC_GLOBAL_EFFECT _bulletEffect = FC_GLOBAL_EFFECT.ICE_GROUND;
	public FC_GLOBAL_EFFECT _beHitEffect = FC_GLOBAL_EFFECT.PARRY_SUCCESS;
		
	public override void AttackEnter ()
	{
		base.AttackEnter ();
		
		_owner.ACOwner.ACFire(FirePortIdx);
		Vector3 pos = _owner.ACOwner.ThisTransform.position;
		GlobalEffectManager.Instance.PlayEffect(_bulletEffect, pos);
		Transform trans = Utils.FindTransformByNodeName(_owner.ACOwner.ThisTransform, "B");
		if (trans == null)
			Debug.LogError("PARRY_EFFECT_POS is null");
		
		pos = trans.position;			
		GlobalEffectManager.Instance.PlayEffect(_beHitEffect, pos);
	}
	
	public override void AttackEnd()
	{
		base.AttackEnd();
	}
	
	public override void AttackQuit()
	{
		base.AttackQuit();
	}
	
	protected override void AniOver()
	{
		base.AniOver();
	}
}
