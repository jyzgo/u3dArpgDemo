//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Sends a message to the remote object when something happens.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Message")]
public class UIButtonMessage : MonoBehaviour
{
	public enum Trigger
	{
		OnClick,
		OnMouseOver,
		OnMouseOut,
		OnPress,
		OnRelease,
		OnDoubleClick,
	}

	public GameObject target;
	public string functionName;
	public Trigger trigger = Trigger.OnClick;
	public bool includeChildren = false;
	
	public static int _pressCount = 0;
	public static bool _allowMulti = false;
	private bool _isPressed = false;

	bool mStarted = false;
	bool mHighlighted = false;

	void Start () { mStarted = true; }

	void OnEnable () {
		if(_isPressed)
		{
			_isPressed = false;
			--_pressCount;
		}
		if (mStarted && mHighlighted) OnHover(UICamera.IsHighlighted(gameObject)); }
	
	void OnDisable()
	{
		if(_isPressed)
		{
			_isPressed = false;
			--_pressCount;
		}
	}
	
	void OnDestroy()
	{
		if(_isPressed)
		{
			--_pressCount;
		}
	}

	void OnHover (bool isOver)
	{
		if (enabled)
		{
			if (((isOver && trigger == Trigger.OnMouseOver) ||
				(!isOver && trigger == Trigger.OnMouseOut))) Send();
			mHighlighted = isOver;
		}
	}

	void OnPress (bool isPressed)
	{
		if (enabled)
		{
			if(isPressed && !_isPressed)
			{
				_isPressed = true;
				++_pressCount;
			}
			if(!isPressed && _isPressed)
			{
				_isPressed = false;
				--_pressCount;
			}
			if (((isPressed && trigger == Trigger.OnPress) ||
				(!isPressed && trigger == Trigger.OnRelease))) Send();
		}
	}
	
	void OnClick () { 
		if(enabled)
		{
			if (trigger == Trigger.OnClick && (_allowMulti || _pressCount <= 0)) Send(); 
		}
	}

	void OnDoubleClick () { if (enabled && trigger == Trigger.OnDoubleClick) Send(); }

	void Send ()
	{
		if (string.IsNullOrEmpty(functionName)) return;
		if (target == null) target = gameObject;

		if (includeChildren)
		{
			Transform[] transforms = target.GetComponentsInChildren<Transform>();

			for (int i = 0, imax = transforms.Length; i < imax; ++i)
			{
				Transform t = transforms[i];
				t.gameObject.SendMessage(functionName, gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			target.SendMessage(functionName, gameObject, SendMessageOptions.DontRequireReceiver);
		}
	}
}