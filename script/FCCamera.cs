using UnityEngine;
using System.Collections;

public class FCCamera : MonoBehaviour {

    public float fieldOfView;
	public float fovFactorOnPhone;
	public float heightOffset;
    public Vector2 clipPlanes;
	public AnimationCurve _cameraSpeedCurve;
	public AnimationCurve _targetSpeedCurve;
	
	protected bool _needRotateBlend = true;
	
	public bool NeedRotateBlend
	{
		get
		{
			return _needRotateBlend;
		}
		set
		{
			_needRotateBlend = value;
		}
	}
	// pos input as last position, update to new position. 
	public virtual void UpdatePosAndRotation(Transform target, Transform target2, bool enableClamp, ref Vector3 pos, ref Vector3 lookat) {
		
	}

    //what happens after setting target
    public virtual void OnSetTarget(Transform target, ref Transform target2)
    {
    }
}
