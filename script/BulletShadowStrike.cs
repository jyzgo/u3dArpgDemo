using UnityEngine;
using System.Collections;

public class BulletShadowStrike : BulletMissile {

	protected override void ActiceLogicSelf(RangerAgent.FirePort rfp)
	{
		if(_owner.FireTarget != null)
		{
			ThisTransform.position = _owner.FireTarget.ThisTransform.localPosition;
			Vector2 radius = Random.insideUnitCircle;
			Vector3 r3 = Vector3.zero;
			r3.x += radius.x;
			r3.z += radius.y;
			if(r3 != Vector3.zero)
			{
				ThisTransform.forward = r3;
			}
		}
		else
		{
			Vector2 radius = Random.insideUnitCircle*0.5f;
			Vector3 r3 = Vector3.zero;
			r3.x += radius.x;
			r3.z += radius.y;
			ThisTransform.position = _owner.ThisTransform.localPosition + r3;
			radius = Random.insideUnitCircle;
			r3 = Vector3.zero;
			r3.x += radius.x;
			r3.z += radius.y;
			if(r3 != Vector3.zero)
			{
				ThisTransform.forward = r3;
			}
			//_owner.SelfMoveAgent.Move(r3);
		}
	}
	
	public override void GetHurtDirection(Transform targetTransform,ref Vector3 direction)
	{
		direction = ThisTransform.forward;
	}
	
}
