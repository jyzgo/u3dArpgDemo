using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EffectManagerBase : MonoBehaviour {
	
	//use this to record living effects.
	protected class LivingEffect {
		public AvatarController _avatar = null;
		public EffectInstance _effect = null;
		public int _effID = -1;
	}
	protected List<LivingEffect> _livingEffects = null;	
	
	protected Transform _myRoot = null;
	
	
	
	protected virtual void InstantiatePool() {
		_livingEffects = new List<LivingEffect>();
	}
	
	public virtual void DestroyPool() {	
		_livingEffects.Clear();		
	}
	
	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	protected virtual void Update () {
		
		List<LivingEffect> tobeRemoveEffects = new List<LivingEffect>();
		
		//update living objects, count down the life time
		foreach (LivingEffect livingEff in _livingEffects)
		{
			EffectInstance go = livingEff._effect;
			Assertion.Check(go != null);

			EffectInstance eff = go;
			if (eff != null)
			{
				if (eff.LifeTick > 0)
				{
					//reduce eff life tick
					eff.LifeTick -= Time.deltaTime;		
				}
				else
				{
					
					if(eff.DeadTick > 0)
					{
						eff.DeadTick -= Time.deltaTime;
						eff.UpdatePointLight();
						
					}else{
						
						
						//recycle back to pool
						eff.LifeTick = eff._lifeTime;
						eff.DeadTick = eff._deadTime;
						eff.FinishEffect(true);
						eff.myTransform.parent = _myRoot;
						go.gameObject.SetActive(false);
						
						//remove it from living list
						tobeRemoveEffects.Add(livingEff);
					}
					

				}
			}
		}
		

		//remove living effects
		foreach (LivingEffect livingEff in tobeRemoveEffects)
		{
			_livingEffects.Remove(livingEff);
		}
		
		tobeRemoveEffects.Clear();
		
	}
}
