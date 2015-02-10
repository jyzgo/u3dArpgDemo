using UnityEngine;
using System.Collections;

public class FC2Camera : FCCamera
{
    public Vector3 translation;

	public override void UpdatePosAndRotation (Transform target, Transform target2, bool enableClamp, ref Vector3 pos, ref Vector3 lookat)
	{		
		Vector3 lastFocus = lookat;
		
		lookat = target.position + Vector3.up * heightOffset;
		if(enableClamp) {
			Vector3 nextFocus = lookat - lastFocus;
			float focusLen = nextFocus.magnitude;
			if(focusLen > Mathf.Epsilon) {
				focusLen = Mathf.Min(_targetSpeedCurve.Evaluate(focusLen), focusLen);
				lookat = lastFocus + nextFocus.normalized * focusLen;
			}
		}
		pos = lookat + translation;
	}
}
