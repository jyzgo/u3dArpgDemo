using UnityEngine;
using System.Collections;

public class LoginCharAnimation : MonoBehaviour {
	
	public Animator _myAnimator;
	
	public LoginCharEffect _characterEffect;
	
	void OnEnable() {
		_myAnimator.SetInteger("move", 1);
		_characterEffect.ShowEffect1();
	}
	
	void OnDisable() {
		_myAnimator.SetInteger("move", 0);
		_characterEffect.ShowEffect();
	}
	
	public void PlayStartupAnimation() {
		_myAnimator.SetInteger("move", 2);
		_characterEffect.ShowEffect2();
	}
}
