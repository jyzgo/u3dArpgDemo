using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(DiscoverAgent))]
public class DiscoverAgentEditor : Editor
{
	DiscoverAgent _target;
	void OnEnable(){
		_target = (DiscoverAgent)target;
	}
	
	void OnSceneGUI(){
		if(_target.enabled && _target.transform.parent != null)
		{
			//DrawRegion(_target.transform ,360,_target._sightRaduisBack);
			Color color = new Color(1,1,1,0.1f);
			DrawRegion(_target.transform ,360,_target._sightRaduisBack,color);
			color = new Color(1,0,0,0.1f);
			DrawRegion(_target.transform ,_target._sightAngle ,_target._sightRaduisFront,color);
		}
	}
	
	void DrawRegion(Transform targetTransform,float angle,float raduis,Color color)
	{
		Handles.color = color;
		Vector3 begin = targetTransform.right;
		if(angle < 360f)
		{
			float beginAngle = -(angle/2)+targetTransform.parent.eulerAngles.y;
			begin.x = Mathf.Sin(beginAngle/180*Mathf.PI);
			begin.z = Mathf.Cos(beginAngle/180*Mathf.PI);
			Handles.DrawSolidArc(targetTransform.position,targetTransform.up,begin,angle,raduis);
		}
		else
		{
			Handles.DrawSolidArc(targetTransform.position,targetTransform.up,begin,360f,raduis);
		}
		
	}
}
