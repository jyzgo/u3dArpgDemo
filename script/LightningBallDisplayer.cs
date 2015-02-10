using UnityEngine;
using System.Collections;

public class LightningBallDisplayer : BulletDisplayer {
	
	public ParticleSystem []_particles;	
	public Renderer []_renderers;
	public Animation _myAnimation;
	public GameObject _pointLight;
	
	void Awake() {
		foreach(Renderer r in _renderers) {
			r.sharedMaterial = Utils.CloneMaterial(r.sharedMaterial);
		}
		_animationUpdater = new UpdateAnimation(UpdateEmpty);
	}
	
	public override void StartEffect ()
	{
		foreach(Renderer r in _renderers) {
			r.enabled = true;
			r.sharedMaterial.SetFloat("_startTime", Time.timeSinceLevelLoad);
		}
		foreach(ParticleSystem ps in _particles) {
			ps.enableEmission = true;
			ps.Play();
		}
		_myAnimation.Play("start");
		_animationUpdater = UpdateStartAnimation;
		if(_pointLight != null) {
			_pointLight.SetActive(true);
		}
	}
	
	void Update() {
		_animationUpdater();
	}
	
	delegate void UpdateAnimation();
	UpdateAnimation _animationUpdater;
	
	void UpdateEmpty() {
	}
	
	void UpdateStartAnimation() {
		if(!_myAnimation.isPlaying) {
			_myAnimation.Play("loop");
			_animationUpdater = UpdateEmpty;
		}
	}
	
	void UpdateEndAnimation() {
		if(!_myAnimation.isPlaying) {
			foreach(Renderer r in _renderers) {
				r.enabled = false;
			}
			_animationUpdater = UpdateEmpty;
		}
	}
	
	public override float EndEffect ()
	{
		foreach(ParticleSystem ps in _particles) {
			ps.enableEmission = false;
			ps.Stop();
		}
		_myAnimation.Play("end");
		_animationUpdater = UpdateEndAnimation;
		if(_pointLight != null) {
			_pointLight.SetActive(false);
		}
		return 0.0f;
	}
}
