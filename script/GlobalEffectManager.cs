using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GlobalEffectManager : EffectManagerBase {
	
	#region singleton defines	
	static GlobalEffectManager _inst;
	static public GlobalEffectManager Instance {
		get {
			return _inst;
		}
	}
	
	void Awake() {
		if(_inst != null)
		{
            Debug.LogError("GlobalEffectManager: detected singleton instance has existed. Destroy this one " + gameObject.name);
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
	
	
	public string []_globalEffectList; //a effect list, according to the FC_GLOBAL_EFFECT enum
	private List<GameObject> _globalEffectPrefab = null;	

	private Dictionary<FC_GLOBAL_EFFECT, List<EffectInstance>> _allEffectArray; 
	
	
	protected override void InstantiatePool() {
		base.InstantiatePool();

		_myRoot = LevelManager.Singleton.GlobalEffectRoot;
		
		_allEffectArray = new Dictionary<FC_GLOBAL_EFFECT, List<EffectInstance>>();
		for (FC_GLOBAL_EFFECT i = FC_GLOBAL_EFFECT.START; i<FC_GLOBAL_EFFECT.MAX; i++)
		{
			_allEffectArray[i] = new List<EffectInstance>();
		}
		
		//we have enough global effects?
		Assertion.Check(_globalEffectList.GetLength(0) == (int)FC_GLOBAL_EFFECT.MAX, "not enough global effects");
		
		//preload prefabs
		_globalEffectPrefab = new List<GameObject>();
		foreach(string effPath in _globalEffectList)
		{
			GameObject instance = InJoy.AssetBundles.AssetBundles.Load(effPath, typeof(GameObject)) as GameObject;
			_globalEffectPrefab.Add(instance);
		}	
		
		// init 4 instances for each effect.
		for(FC_GLOBAL_EFFECT i = FC_GLOBAL_EFFECT.START;i < FC_GLOBAL_EFFECT.MAX;++i) {
			List<EffectInstance> list = _allEffectArray[i];
			for(int j = 0;j < 4;++j) {
				GameObject go = AddEffectToPool(i);
				EffectInstance ei = go.GetComponent<EffectInstance>();
				Assertion.Check(ei != null);
				ei.global_effect_id = i;
				list.Add(ei);
			}
		}
	}
	
	public override void DestroyPool() {
		base.DestroyPool();
		
		foreach(KeyValuePair<FC_GLOBAL_EFFECT, List<EffectInstance>> pair in _allEffectArray)
		{
			List<EffectInstance> leb = pair.Value;
			leb.Clear();
		}	
		
	}

	
	public void PlayEffect(FC_GLOBAL_EFFECT effId, Vector3 effPos)
	{
		PlayEffect(effId, effPos, Quaternion.identity);
	}
	
	
	static int Compare (EffectInstance a, EffectInstance b)
	{
		EffectInstance effA = a;
		EffectInstance effB = b;
		
		if (effA.LifeTick > effB.LifeTick)
			return 1;
		else
			return -1;
	}
	
	public EffectInstance PlayEffect(FC_GLOBAL_EFFECT effId, Vector3 effPos, Quaternion effRot)
	{
		if (effId == FC_GLOBAL_EFFECT.INVALID)
			return null;

		
		//get empty gameobject?
		List<EffectInstance> effList = _allEffectArray[effId];
		EffectInstance effResult = null;
		if(effList.Count>0)
		{
			foreach(EffectInstance effInst in effList)
			{
				if(!effInst.gameObject.activeSelf)
				{
					effResult = effInst;
					break;
				}
			}
		}
		
		//no empty? create one
		if(effResult == null)
		{
			if (effList.Count == FCConst.GLOBAL_EFFECT_COUNT)
			{
				//already full, select a slot which is nearly dead
				effList.Sort(Compare);
				EffectInstance slot = effList[0];
				
				EffectInstance effTemp = slot;
				if (effTemp != null)
				{
					//recycle back to pool
					effTemp.LifeTick = effTemp._lifeTime;
					effTemp.FinishEffect(true);
					slot.gameObject.SetActive(false);
					effResult = slot;
				}				
			}
			else
			{
				//size is not full, add a new one
				GameObject ego = AddEffectToPool(effId);
				effResult = ego.GetComponent<EffectInstance>();
				Assertion.Check(effResult != null);
				effResult.global_effect_id = effId;
				effList.Add(effResult);
			}
			
		}
		
		//set enable and pos
		Assertion.Check(effResult != null);
		effResult.gameObject.SetActive(true);
		Transform effectRsltTransform = effResult.myTransform;
		effectRsltTransform.position = effPos;
		effectRsltTransform.rotation = effRot;
		
		//add to living list
		LivingEffect liveEff = new LivingEffect();
		liveEff._effect = effResult;
		liveEff._avatar = null;
		liveEff._effID = (int) effId;

		_livingEffects.Add(liveEff);
		
		
		//get effect instanse and start effect
		EffectInstance eff = effResult;
		if (eff != null)
		{
			eff.LifeTick = eff._lifeTime;
			eff.DeadTick = eff._deadTime;
			eff.BeginEffect();	
		}
		
		return effResult;
	}
	
	//play effect with ID at pos and rot
	public EffectInstance PlayEffect(FC_GLOBAL_EFFECT effId, Vector3 effPos, Quaternion effRot, Vector3 scale)
	{
		EffectInstance eff = PlayEffect( effId, effPos, effRot);
		if(eff != null) {
			eff.myTransform.localScale = scale;
		}
		return eff;
	}

    public void StopEffect(FC_GLOBAL_EFFECT effId)
    {
        List<LivingEffect> tobeRemoveEffects = new List<LivingEffect>();
		
        foreach (LivingEffect livingEff in _livingEffects)
        {
            if (livingEff._effID == (int)effId)
            {
                EffectInstance go = livingEff._effect;

                go.LifeTick = -1;
                go.DeadTick = -1;
                go.FinishEffect(true);
                go.myTransform.parent = _myRoot;
                go.gameObject.SetActive(false);
                tobeRemoveEffects.Add(livingEff);
            }
        }

        //remove living effects
        foreach (LivingEffect livingEff in tobeRemoveEffects)
        {
            _livingEffects.Remove(livingEff);
        }

        tobeRemoveEffects.Clear();
    }
		
	private GameObject AddEffectToPool(FC_GLOBAL_EFFECT effId)
	{
		if (_myRoot == null)
			_myRoot = Utils.NewGameObjectWithParent("Global_Effect_Root");
		
		GameObject instance = GameObject.Instantiate(_globalEffectPrefab[(int)effId]) as GameObject;
		instance.transform.parent = _myRoot;
		instance.transform.localPosition = Vector3.zero;
		instance.transform.localRotation = Quaternion.identity;	

		instance.SetActive(false);
		return instance;
	}	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update();
		
/*		
		//update living effect, count down the life time
		foreach(KeyValuePair<FC_GLOBAL_EFFECT, List<GameObject>> pair in _allEffectArray)
		{
			List<GameObject> leb = pair.Value;
			foreach(GameObject go in leb)
			{
				if (go.activeSelf)
				{
					EffectInstance eff = go.GetComponent<EffectInstance>();
					if (eff != null)
					{
						if (eff.lifeTick > 0)
						{
							//reduce eff life tick
							eff.lifeTick -= Time.deltaTime;		
						}
						else
						{
							//recycle back to pool
							eff.lifeTick = eff._lifeTime;
							eff.FinishEffect(true);
							go.SetActive(false);
						}
					}
				}
				
			}
		}

*/				
		
	}

}
