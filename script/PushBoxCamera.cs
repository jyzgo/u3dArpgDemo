using UnityEngine;
using System.Collections;

public class PushBoxCamera : FC2Camera
{    
    public GameObject boxPrefab;

    public override void UpdatePosAndRotation(Transform target, Transform target2, bool enableClamp, ref Vector3 pos, ref Vector3 lookat)
    {
        if (target2 == null) return;

        Vector3 lastFocus = lookat;
        lookat = target2.position + Vector3.up * heightOffset;
        if (enableClamp)
        {
            Vector3 nextFocus = lookat - lastFocus;
            float focusLen = nextFocus.magnitude;
            if (focusLen > Mathf.Epsilon)
            {
                focusLen = Mathf.Min(_targetSpeedCurve.Evaluate(focusLen), focusLen);
                lookat = lastFocus + nextFocus.normalized * focusLen;
            }
        }
        pos = lookat + translation;
    }

    public override void OnSetTarget(Transform target, ref Transform target2)
    {
        //create the camera box
        target2 = Utils.InstantiateGameObjectWithParent(boxPrefab, target.parent);

        target2.GetComponent<CameraBox>().SetTarget(target, this);
    }
}
