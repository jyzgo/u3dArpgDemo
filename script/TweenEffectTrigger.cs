using UnityEngine;
using System.Collections;

public class TweenEffectTrigger : MonoBehaviour {
	
	public void OnInitialize()
	{
		TweenScale[] tweens = GetComponentsInChildren<TweenScale>(); 
		foreach(TweenScale tween in tweens)
		{
			tween.Reset();
			tween.Play(true);
		}
	}

}
