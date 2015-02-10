using UnityEngine;
using System.Collections;

public class ParticleSystemInstance : EffectInstance {

	public ParticleSystem []_particles;
	
	public override void BeginEffect ()
	{
		if (effectStarted)
			return;
		else
			effectStarted = true;
	
		base.BeginEffect();
		
		foreach(ParticleSystem ps in _particles) {
			ps.Clear();
			ps.enableEmission = true;
			ps.startDelay = 0.001f;//fix a bug that position is wrong
			ps.Play();
			// if particle is disabled, play 1st emitter only.
			if(!GameSettings.Instance.IsParticleEnabled()) {
				break;
			}
		}
	}
	
	public override void FinishEffect (bool force)
	{
		//I will consider force, discard non-force finish
		if (_considerForce && !force)
			return;
		
		
		effectStarted = false;
		
		foreach(ParticleSystem ps in _particles) {
			ps.Stop();					
			ps.enableEmission = false;
		}
	}
}
