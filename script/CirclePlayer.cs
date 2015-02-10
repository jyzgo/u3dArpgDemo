using UnityEngine;
using System.Collections;

public class CirclePlayer : MonoBehaviour {
	
	public float _radiusIdle = 0.2f;
	public float _radiusBackForAttack = 0.5f;
	public float _radiusForwardForAttack = 0.1f;
	
	protected Vector3 _offsetPos = Vector3.zero;
	
	protected Transform _thisTransform;
	
	void Awake()
	{
		_thisTransform = transform;
	}
	public void UpdateCamera(ActionController ac, bool followPlayer)
	{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
		if(!CheatManager.lockCamera)
		{
			if(ac != null)
			{
				Vector3 v3 = _thisTransform.position;
				v3.y = ac.ThisTransform.position.y;
				_thisTransform.position = v3;
				if(!followPlayer)
				{
					v3 = ac.ThisTransform.localPosition- _thisTransform.position;
					float dis = v3.sqrMagnitude;
					if(dis > _radiusIdle*_radiusIdle)
					{
						dis = v3.magnitude;
						v3 = (1-_radiusIdle/dis)*v3;
						v3 = _thisTransform.position + v3;
						v3.y = ac.ThisTransform.localPosition.y;
						_thisTransform.position = v3;
					}
					_offsetPos = ac.ThisTransform.localPosition - _thisTransform.position;
					_offsetPos.y = 0;
				}
				else
				{
					_thisTransform.position = ac.ThisTransform.localPosition-_offsetPos;
					_offsetPos = _offsetPos * 0.9f;
				}
				_thisTransform.localRotation = ac.ThisTransform.localRotation;
			}	
		}
		else
#endif
		{
			_thisTransform.position = ac.ThisTransform.localPosition;
		}
	}
}
