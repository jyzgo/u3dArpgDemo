using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class SkillCamera
{
    public string cameraName;
	public float duration;
    public AnimationCurve horizontalRotation;
    public AnimationCurve verticalRotation;
    public AnimationCurve distance;         //the change of distance from camera to target
    public AnimationCurve fieldOfView;
    public AnimationCurve horizontal;       //the change of horizontal from camera to target
    public AnimationCurve vertical;         //the change of vertical from camera to target

    /// <summary>
    /// change camera position, rotation, fov by lerping between last skill camera and me.
    /// Last camera could be null, that means lerping will be based on current camera setting
    /// </summary>
    /// <param name="percent"></param>
    /// <param name="cam"></param>
    /// <param name="trans"></param>
    /// <param name="lastCam"></param>
    public void Update(float time, SkillCameraCache skillcam)
    {
		float hrot = horizontalRotation.Evaluate(time);
		float vrot = verticalRotation.Evaluate(time);
		float dist = distance.Evaluate(time);
		float fov = fieldOfView.Evaluate(time);
        float hori = horizontal.Evaluate(time);
        float vert = vertical.Evaluate(time);

        skillcam.SetParameter(hrot, vrot, dist, fov, hori, vert);
    }
}

[System.Serializable]
public class SkillCameraCache
{
	[HideInInspector]
    public float horizontalRotation = 0.0f;
	[HideInInspector]
    public float verticalRotation = 0.0f;
	[HideInInspector]
    public float distance = 0.0f;
	[HideInInspector]
    public float fieldOfView = 0.0f;
    [HideInInspector]
    public float horizontal = 0.0f;
    [HideInInspector]
    public float vertical = 0.0f;
	
	public float maxHRotPerSec;
	public float maxVRotPerSec;
	public float maxZoomPerSec;
	public float maxFovPerSec;
    public float maxHPerSec;
    public float maxVPerSec;

    public void SetParameter(float hrot, float vrot, float dist, float fov, float hori, float vert)
	{
		float clamp = maxHRotPerSec * Time.deltaTime;
		float delta = Mathf.Max(Mathf.Min(hrot - horizontalRotation, clamp), -clamp);
		horizontalRotation += delta;
		
		clamp = maxVRotPerSec * Time.deltaTime;
		delta = Mathf.Max(Mathf.Min(vrot - verticalRotation, clamp), -clamp);
		verticalRotation += delta;
		
		clamp = maxZoomPerSec * Time.deltaTime;
		delta = Mathf.Max(Mathf.Min(dist - distance, clamp), -clamp);
		distance += delta;
		
		clamp = maxFovPerSec * Time.deltaTime;
		delta = Mathf.Max(Mathf.Min(fov - fieldOfView, clamp), -clamp);
		fieldOfView += delta;

        clamp = maxHPerSec * Time.deltaTime;
        delta = Mathf.Max(Mathf.Min(hori - horizontal, clamp), -clamp);
        horizontal += delta;

        clamp = maxVPerSec * Time.deltaTime;
        delta = Mathf.Max(Mathf.Min(vert - vertical, clamp), -clamp);
        vertical += delta;
	}
}