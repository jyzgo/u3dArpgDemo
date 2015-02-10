using UnityEngine;
using System.Collections;


//this class just for player Aniamtion in preview screen
public class PreviewAnimation : MonoBehaviour {
	
	private Animator _myAnimator;
	private LoginCharEffect _characterEffect;
	
	void Awake () {
		_myAnimator = gameObject.GetComponent<Animator>();
		_characterEffect = gameObject.GetComponent<LoginCharEffect>();
	}
	
	void OnEnable() {
		_myAnimator.SetInteger("move", 0);
		StartCoroutine(DelayClear());
		if(_characterEffect != null)
		{
			_characterEffect.ShowEffect1();
		}
	}
	
	
	public void PlayPoseOverAnimation() {
		_myAnimator.SetInteger("move", 1);
		StartCoroutine(DelayClear());
		if(_characterEffect != null)
		{
			_characterEffect.ShowEffect2();
		}
	}
	
	public void PlayPoseAnimation() {
		_myAnimator.SetInteger("move", 2);
		StartCoroutine(DelayClear());
		if(_characterEffect != null)
		{
			_characterEffect.ShowEffect2();
		}
	}
	
	IEnumerator DelayClear()
	{
		yield return null;
		yield return null;
		_myAnimator.SetInteger("move", 0);
	}

}
