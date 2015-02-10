using UnityEngine;
using System.Collections;

public class EffectAgent : FCObject ,FCAgent {
#if WQ_CODE_WIP
	ActionController _owner;
#endif
	public static string GetTypeName()
	{
		return "EffectAgent";
	}
	
	[System.Serializable]
	public class EffectInfo
	{
		public int _selfID;
		public FC_EFFECT_KIND _kindID;
		public FC_EFFECT _effectID;
	}
	
	public EffectInfo[] _effectInfos = null;
	public void Init(FCObject ewb)
	{
#if WQ_CODE_WIP
		_owner = ewb as ActionController;
#endif
	}
	public void PlayEffect(int idx,FC_EFFECT_KIND kind)
	{
		foreach(EffectInfo ei in _effectInfos)
		{
			if(ei._selfID == idx && ei._kindID == kind)
			{
				//to play 
				break;
			}
		}
	}
}
