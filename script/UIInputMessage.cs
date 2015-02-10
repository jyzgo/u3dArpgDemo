using UnityEngine;
using System.Collections;


[AddComponentMenu("NGUI/Interaction/Input Message")]
public class UIInputMessage : MonoBehaviour 
{
	public enum Trigger
	{
		OnSelect,
		OnInputChanged,
	}
	
	public GameObject target;
	public string functionName;
	public Trigger trigger = Trigger.OnSelect;
	public bool includeChildren = false;

	// Use this for initialization
	void Start () {
	
	}
	
	void OnSelect(bool selected)
	{
		if (trigger == Trigger.OnSelect) 
		{
			Send(selected);
		}
	}
	
	void OnInputChanged(object o)
	{
		if (trigger == Trigger.OnInputChanged) 
		{
			Send(o);
		}
	}
	
	void Send (object o)
	{
		if (string.IsNullOrEmpty(functionName)) return;
		if (target == null) target = gameObject;

		if (includeChildren)
		{
			Transform[] transforms = target.GetComponentsInChildren<Transform>();

			for (int i = 0, imax = transforms.Length; i < imax; ++i)
			{
				Transform t = transforms[i];
				t.gameObject.SendMessage(functionName, o, SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			target.SendMessage(functionName, o, SendMessageOptions.DontRequireReceiver);
		}
	}
}
