using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterEffectManager : EffectManagerBase {
	         
	#region singleton defines	
	static CharacterEffectManager _inst;
	static public CharacterEffectManager Instance {
		get {
			return _inst;
		}
	}
	
	void Awake() {
		if(_inst != null)
		{
            Debug.LogError("CharacterEffectMananger: detected singleton instance has existed. Destroy this one " + gameObject.name);
			Destroy(this);
			return;
		}
		
		InstantiatePool();
		_inst = this;
		
	}
	
	void OnDestroy() {
		DestroyPool();
		
		if(_inst == this)
		{
			_inst = null;
		}
	}
	#endregion
	
	
	[System.Serializable]
	public class CharacterEffectConfig {
		public FC_CHARACTER_EFFECT _effectIndex;
		public EnumEquipSlot _parentSlot;
		public string _effects;
		private GameObject _originalEffectObject = null;
		
		public GameObject originalEffectObject {
			get {
				return _originalEffectObject;
			}
			set {
				_originalEffectObject = value;
			}
		}		
	}
	
	//character effect config
	public CharacterEffectConfig []_characterEffectConfig;	
	
	//convert the config to a hashtable, sort fast
	private Hashtable _characterEffectTable = new Hashtable(); 
	
	
	private Dictionary<FC_CHARACTER_EFFECT, List<EffectInstance>> _allEffectArray; 
	
	//get config by effect id
//	private CharacterEffectConfig GetConfigByEffect(FC_CHARACTER_EFFECT eff)
	
	protected override void InstantiatePool() {
		base.InstantiatePool();
		
		_myRoot = LevelManager.Singleton.CharacterEffectRoot;
		
		_allEffectArray = new Dictionary<FC_CHARACTER_EFFECT, List<EffectInstance>>();
		for (FC_CHARACTER_EFFECT i = FC_CHARACTER_EFFECT.START; i<FC_CHARACTER_EFFECT.MAX; i++)
		{
			_allEffectArray[i] = new List<EffectInstance>();
		}
			
		//we have enough char effects?
//		Assertion.Check(_characterEffectConfig.GetLength(0) == (int)FC_CHARACTER_EFFECT.MAX, "not enough character effects");

		//preload prefabs
		foreach(CharacterEffectConfig effConfig in _characterEffectConfig)
		{
			GameObject instance = InJoy.AssetBundles.AssetBundles.Load(effConfig._effects, typeof(GameObject)) as GameObject;

			//set original gameobj 
			effConfig.originalEffectObject = instance;
			
			//build hashtable 
			_characterEffectTable[effConfig._effectIndex] = effConfig;
		}	
	}
	
	public override void DestroyPool() {
		base.DestroyPool();
		
		foreach(KeyValuePair<FC_CHARACTER_EFFECT, List<EffectInstance>> pair in _allEffectArray)
		{
			List<EffectInstance> leb = pair.Value;
			leb.Clear();
		}	
		
	}	
	
	// Use this for initialization
	void Start () {
	
	}
	
	
	
	private GameObject AddEffectToPool(FC_CHARACTER_EFFECT effId)
	{
		if (_myRoot == null)
			_myRoot = Utils.NewGameObjectWithParent("Character_Effect_Root");
		
		CharacterEffectConfig effConfig = _characterEffectTable[effId] as CharacterEffectConfig;
		
		GameObject instance = GameObject.Instantiate(effConfig.originalEffectObject) as GameObject;
		Transform t = instance.transform;
		t.parent = _myRoot;
		t.localPosition = Vector3.zero;
		t.localRotation = Quaternion.identity;	
		instance.SetActive(false);
		return instance;
	}	

	//play effect
	//lifetime: character effect may have a lift time from outside, > 0 will take effect.
	//          < 0 means use effect's built-in life time.
	public EffectInstance PlayEffect(FC_CHARACTER_EFFECT effId, AvatarController avatar, float liftTime)
	{
		if (effId == FC_CHARACTER_EFFECT.INVALID)
			return null;
	
		//get empty gameobject?
		List<EffectInstance> effList = _allEffectArray[effId];
		EffectInstance effResult = null;
		if(effList.Count>0)
		{
			foreach(EffectInstance effInst in effList)
			{
				if(effInst == null
					|| effInst.myObject == null)
				{
					int zz = 0;
					zz++;
				}
				if(!effInst.myObject.activeSelf)
				{
					effResult = effInst;
					break;
				}
			}
		}
		
		//no empty? create one
		if(effResult == null)
		{
			GameObject ego = AddEffectToPool(effId);
			effResult = ego.GetComponent<EffectInstance>();
			Assertion.Check(effResult != null);
			effResult.character_effect_id = effId;
			effResult.myTransform = ego.transform;
			effResult.myObject = ego;
			effList.Add(effResult);
			
		}
		
		//set enable and parent and pos
		Assertion.Check(effResult != null);
		effResult.myObject.SetActive(true);
		
		//find parent node
		CharacterEffectConfig effConfig = _characterEffectTable[effId] as CharacterEffectConfig;
		Assertion.Check(effConfig != null);

		Transform parent = Utils.FindTransformByNodeName(avatar.myTransform, 
			FCEquipmentsBase.GetNodeByEquipSlot(effConfig._parentSlot)
			);
		if(parent == null)
		{
			parent = avatar.myTransform;
		}
		Assertion.Check(parent != null);
		effResult.myTransform.parent = parent;
		effResult.myTransform.localPosition = Vector3.zero;
		effResult.myTransform.localRotation = Quaternion.identity;	
		
		//add to living list
		LivingEffect liveEff = new LivingEffect();
		liveEff._effect = effResult;
		liveEff._avatar = avatar;
		liveEff._effID = (int)effId;
		_livingEffects.Add(liveEff);
		
		//get effect instance and start effect
		EffectInstance eff = effResult;
		Assertion.Check(eff != null);
		if (liftTime > 0)
			eff.LifeTick = liftTime;
		else
			eff.LifeTick = eff._lifeTime;
		
		eff.DeadTick = eff._deadTime;
		
		eff.BeginEffect();	
		
		return effResult;
	}	
		
	//Stop effect
	//stop all effects with id on this AV
	//time: stop and deactive left time
	public void StopEffect(FC_CHARACTER_EFFECT effId, AvatarController avatar, float deadTime)
	{
		if (effId == FC_CHARACTER_EFFECT.INVALID)
			return;
		
		//update living objects, count down the life time
		foreach (LivingEffect liveingEff in _livingEffects)
		{
			EffectInstance go = liveingEff._effect;
			Assertion.Check(go != null);
			
			//find avatar match
			if ((liveingEff._effID == (int)effId) && (liveingEff._avatar == avatar))
			{
				//stop it
                EffectInstance eff = go;
				if (eff != null)
				{
					//stop effect and set life time
					//we stop it but do not deactive it to get a "fade out" effect
					
					/*
					if (eff._deadTime > 0)
						eff.lifeTick = eff._deadTime;
					else
					*/	
					
					eff.LifeTick = -1;
					if(deadTime > 0)
					{
						eff.DeadTick = deadTime;
					}
					
					
					eff.FinishEffect(false);
				}			
			}
		}	
       
	}

    public void SetEffectConsiderForce(FC_CHARACTER_EFFECT effId, AvatarController avatar,bool force)
    {
        if (effId == FC_CHARACTER_EFFECT.INVALID)
        {
            return;
        }

        foreach (LivingEffect liveingEff in _livingEffects)
        {
            EffectInstance go = liveingEff._effect;
            Assertion.Check(go != null);

            if ((liveingEff._effID == (int)effId) && (liveingEff._avatar == avatar))
            {
                EffectInstance eff = go;

                if (eff != null)
                {
                    eff._considerForce = force;
                }
            }
        }

    }

	
	// Update is called once per frame
	protected override void Update () {
		base.Update();
	
	}	
}
