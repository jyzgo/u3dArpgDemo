using UnityEngine;
using System.Collections;

public class EffectBinder : MonoBehaviour {
	
	[System.Serializable]
	public class EffectConfig {
		public string _parentName;
		public GameObject []_effects;
	}
	
	public EffectConfig []_effectConfig;
	EffectInstance []_effects;
	
	public void InstantiateEffects(AvatarController avatar) {
		System.Collections.Generic.List<EffectInstance> effectList = new System.Collections.Generic.List<EffectInstance>();
		foreach(EffectConfig ec in _effectConfig) {
			foreach(GameObject g in ec._effects) {
				GameObject instance = GameObject.Instantiate(g) as GameObject;
				effectList.Add(instance.GetComponent<EffectInstance>());
				instance.transform.parent = Utils.FindTransformByNodeName(avatar.gameObject.transform, ec._parentName);
				instance.transform.localPosition = Vector3.zero;
				instance.transform.localRotation = Quaternion.identity;
			}
		}
		_effects = effectList.ToArray();
	}
	
	public void BeginEffect(int part) {
		foreach(EffectInstance ei in _effects) {
			if (ei._partLevel == part)
				ei.BeginEffect();	
		}
	}
	
	public void FinishEffect(int part, bool force) {
		
		if (part == -1)
		{
			//stop all effects 
			foreach(EffectInstance ei in _effects) {
				ei.FinishEffect(force);
			}
		}
		else
		{
			foreach(EffectInstance ei in _effects) {
				if (ei._partLevel == part)
					ei.FinishEffect(force);
			}
		}
	}
}
