using UnityEngine;
using System.Collections;

public class TownHUDGestureProcessor : GestureProcessor
{
	public static bool _hidePanel = true;
	public BoxCollider _boxCollider;
	
	public override void SetInputZone()
    {
		Vector3 min = UIManager.Instance.MainCamera.WorldToScreenPoint(_boxCollider.bounds.min);
		Vector3 max = UIManager.Instance.MainCamera.WorldToScreenPoint(_boxCollider.bounds.max);
		
        _inputZone = new Rect(min.x, min.y, max.x-min.x, max.y-min.y);
    }

    public override void ProcessGesture(Gesture.GestureData data, float distance)
    {
//        if (data.direction == Gesture.Directions.Left)
//        {			
//			if (_hidePanel)
//			{
//            	gameObject.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
//			}
//        }
//        else 
			if (data.direction == Gesture.Directions.Right)
        {
			if (!_hidePanel)
			{
            	gameObject.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
			}
        }
    }
	
	public override bool ProcessGesture(Gesture.Directions direction, float delta)
    {
        if (direction == Gesture.Directions.Left)
        {			
			if (_hidePanel)
			{
            	gameObject.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
			}
        }
        else if (direction == Gesture.Directions.Right)
        {
			if (!_hidePanel)
			{
            	gameObject.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
			}
        }
		
		return true;
    }
}
