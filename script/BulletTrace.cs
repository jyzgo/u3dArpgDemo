using UnityEngine;
using System.Collections;

public class BulletTrace : BulletMissile {
	
	public float _traceDistance = 5f;
	
	protected override void ActiceLogicSelf(RangerAgent.FirePort rfp)
	{
		if(_owner.TargetAC != null)
		{
			ThisTransform.position = _owner.TargetAC.ThisTransform.localPosition;
		}
		else
		{
			Vector2 radius = Random.insideUnitCircle*5;
			Vector3 r3 = Vector3.zero;
			r3.x += radius.x;
			r3.z += radius.y;
			ThisTransform.position = _owner.ThisTransform.localPosition + r3;

		}
	}
	
	public override void GetHurtDirection(Transform targetTransform,ref Vector3 direction)
	{
		direction = targetTransform.forward;
	}
}
