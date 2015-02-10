using UnityEngine;
using System.Collections;

public class FeatureCamera : FCCamera
{
	public Transform _realTarget; // target used in this camera.
	public Vector3 _translation;

    public override void UpdatePosAndRotation (Transform target, Transform target2, bool enableClamp, ref Vector3 pos, ref Vector3 lookat)
	{
		lookat = _realTarget.position + Vector3.up * heightOffset;
		pos = lookat + _translation;
	}
}
