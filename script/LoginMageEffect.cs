using UnityEngine;
using System.Collections;

public class LoginMageEffect : LoginCharEffect {
	
	public ParticleSystem []_fireParticles;
	
	public override void UpdateEffect (float effect)
	{
	}
	
	public override void ShowEffect ()
	{
		foreach(ParticleSystem ps in _fireParticles) {
			ps.enableEmission = false;
			ps.Stop();
		}
	}
	
	public override void ShowEffect1 ()
	{
		foreach(ParticleSystem ps in _fireParticles) {
			ps.enableEmission = true;
			ps.Play();
		}
	}
}
