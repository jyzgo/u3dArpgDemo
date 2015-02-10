using UnityEngine;
using System.Collections;


public class AnimationPlayer : MonoBehaviour 
{
	public enum BubbleType
	{
		BitmapFont,
		DynamicFont
	}
	
	public BubbleType _type = BubbleType.BitmapFont;
	public BubbleType BubblePrefabType
	{
		get
		{
			return _type;
		}
	}
	
	public Animation _target;

	/// <summary>
	/// Optional clip name, if the animation has more than one clip.
	/// </summary>
	public string _clipName;
	
	/// <summary>
	/// Event receiver to trigger the callback on when the animation finishes.
	/// </summary>

	public GameObject eventReceiver;

	/// <summary>
	/// Function to call on the event receiver when the animation finishes.
	/// </summary>

	public string callWhenFinished;

	/// <summary>
	/// Delegate to call. Faster than using 'eventReceiver', and allows for multiple receivers.
	/// </summary>

	public ActiveAnimation.OnFinished onFinished;
	
	
	// Use this for initialization
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
	
	public void Play()
	{
		ActiveAnimation anim = ActiveAnimation.Play(_target, _clipName, AnimationOrTween.Direction.Forward, 
			AnimationOrTween.EnableCondition.DoNothing, AnimationOrTween.DisableCondition.DoNotDisable);
		
		anim.Reset();

		// Set the delegate
		anim.onFinished = onFinished;

		// Copy the event receiver
		if (eventReceiver != null && !string.IsNullOrEmpty(callWhenFinished))
		{
			anim.eventReceiver = eventReceiver;
			anim.callWhenFinished = callWhenFinished;
		}
		else 
		{
			anim.eventReceiver = null;
		}
	}
}
