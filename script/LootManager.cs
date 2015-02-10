//#define debug_loot

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class LootObjData
{
	public string _lootId;
	public int _lootCount;
	public GameObject _lootObject = null;
}

public enum LOOTTYPE
{
	RANDOM,
	VERTICLE,
	ARC,
	GRID,
	LINE,
	CIRCLE,
	INCIRCLE,
	MAX
}

public class LootManager : MonoBehaviour
{
    public float _time = 0.6f;
    public float _height = 3.0f;
    public float _liftTime = 0.5f;
    public int _lootMax = 10;

    public string _lootMoveParticlePath;
    public string _lootMoveMathfPath;

    public List<string> _lootParticlePath;
	
    public int _smallScMax = 100;
    public int _middleScMax = 1000;

    public string _lootSmallScPath;
    public string _lootMiddleScPath;
    public string _lootLargeScPath;

	public string []_lootArmors;
	
	public int CurRole
	{
		set; get;
	}
	
	public float MF
	{
		get; set;
	}	
	
    public Transform LootRoot
    {
        get { return _lootRoot; }
    }
    private Transform _lootRoot;

    private static LootManager _instance;
    public static LootManager Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        _instance = this;

        _lootRoot = Utils.NewGameObjectWithParent("LootRoot", this.transform);
    }

    void OnDestroy()
    {
        _instance = this;
        if (_lootRoot != null)
        {
            Destroy(_lootRoot.gameObject);
        }
    }
	
	
	
	public void ClearLootObject()
	{
		if(_lootRoot != null)
		{
			for( int i= _lootRoot.childCount-1 ; i >= 0; i--)
			{
				Transform tran = _lootRoot.GetChild(i);
				if(tran != null)
				{
					tran.parent = null;
					Destroy(tran.gameObject);
				}
			}
		}
	}
	
	
	//cache prafebs
	private GameObject _lootMoveParticlePrefab = null;
	private GameObject _lootSmallScPrefab = null;
	private GameObject _lootMiddleScPrefab = null;
	private GameObject _lootLargeScPrefab = null;
	private GameObject _lootMoveMathfPrefab = null;
	//private GameObject _lootWeaponPrefab = null;
	//private GameObject _lootArmorPrefab = null;	
	private List<GameObject> _lootParticlePrefab = null;				

	
	void Start()
	{
		//preload some resources 	
		_lootParticlePrefab = new List<GameObject>();
		_lootMoveParticlePrefab = InJoy.AssetBundles.AssetBundles.Load(_lootMoveParticlePath, typeof(GameObject)) as GameObject;
		_lootSmallScPrefab = InJoy.AssetBundles.AssetBundles.Load(_lootSmallScPath, typeof(GameObject)) as GameObject;
		_lootMiddleScPrefab = InJoy.AssetBundles.AssetBundles.Load(_lootMiddleScPath, typeof(GameObject)) as GameObject;
		_lootLargeScPrefab = InJoy.AssetBundles.AssetBundles.Load(_lootLargeScPath, typeof(GameObject)) as GameObject;
		_lootMoveMathfPrefab = InJoy.AssetBundles.AssetBundles.Load(_lootMoveMathfPath, typeof(GameObject)) as GameObject;
		//_lootWeaponPrefab = InJoy.AssetBundles.AssetBundles.Load(_lootWeaponPath, typeof(GameObject)) as GameObject;
		//_lootArmorPrefab = InJoy.AssetBundles.AssetBundles.Load(_lootArmorPath, typeof(GameObject)) as GameObject;
		
        foreach(String particlePath in _lootParticlePath)
        {
            GameObject particle = InJoy.AssetBundles.AssetBundles.Load(particlePath, typeof(GameObject)) as GameObject;
			_lootParticlePrefab.Add(particle);
        }
	}

#if debug_loot
    void Update()
    {
        Test1();
    }

    public void Test1()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ActionController player = ObjectManager.Instance.GetMyActionController();

            LootObjData lootData = new LootObjData();
            
			float y = 3;
			if(Time.realtimeSinceStartup % 2 >1)
			{
				lootData._lootId = "money";
				y = 0;
			}else{
				lootData._lootId = "all_helm_1_white_defense";       //armor
				y = 3;
			}
            
            //lootData._lootId = "mage_weapon_0_white_initial";   //weapon
            //lootData._lootId = "gemstone_0_0";      //gem

            lootData._lootCount = 1;
            float x = UnityEngine.Random.Range(-3, 3);
            float z = UnityEngine.Random.Range(-3, 3);
            LootOneForCheatDrop(lootData, player.transform.position, new Vector3(x,  y, z));
        }
    }
#endif

    public bool IsHit(float percent)
    {
        float val = UnityEngine.Random.value;
        if (val < percent)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
	
	
	public void Loot(List<LootObjData> dataList, Vector3 pos, Vector3 dir, LOOTTYPE shape, List<Vector3> lootOffest)
	{
		 LootList(pos, dir, shape, dataList, lootOffest);	
	}

	
	//prepare loot prefabs, should execute after CalculateLootResult
	public void PrepareLootPrefabs(List<LootObjData> dataList)
	{
		for (int i = 0; i < dataList.Count; i++)
        {
            PrepareOneLootPrefab(dataList[i]);
		}
			
	}
		
	private void setLootResult(LootResult result, List<LootObjData> list, int index)
	{
		result._loots[index] = new Dictionary<ItemData, int>();
		foreach(LootObjData ld in list)
		{
			ItemData data = getItemData(ld._lootId);
			if(result._loots[index].ContainsKey(data))
			{
				result._loots[index][data] += ld._lootCount;
			}
			else
			{
				result._loots[index][data] = ld._lootCount;
			}
		}
	}
	
    private void LootWithCount(List<LootObjData> dataList, LootData lootData, float count)
    {
        while (count > 0)
        {
            if (count > 1)
            {
                count -= 1;
                GetOneLoot(dataList, lootData);
            }
            else
            {

                if (IsHit(count))
                {
                    GetOneLoot(dataList, lootData);
                }
                break;
            }
        }
    }


    private LootWeight GetRandomLootWeight(List<LootWeight> list)
    {
        if (list == null)
        {
            Debug.LogError("GetRandomLootWeight list = null");
            return null;
        }

        if (list.Count == 0)
        {
            Debug.LogError("GetRandomLootWeight list count = 0");
            return null;
        }
		
		float item_find = PlayerInfo.Instance == null? this.MF: PlayerInfo.Instance.itemFindPossibility;

		int curRole = PlayerInfo.Instance == null ? this.CurRole : (int)PlayerInfo.Instance.Role;
		
        int total = 0;
        for (int i = 0; i < list.Count; i++)
        {
			if(list[i] == null)
			{
				Debug.LogError("list[i] == null [" + i +"]");
				Assertion.Check(false);
				continue;
			}
			
			ItemData itemData = getItemData(list[i]._itemId);
			float tmpWeight = list[i]._weight;
			if(itemData != null)
			{	
				if(itemData.id == null)
				{
					Debug.LogError("itemData._id == null [" + i +"]");
					Assertion.Check(false);
					continue;
				}
				
				
				if(itemData.rareLevel >=2)
				{
					tmpWeight *= (1+ item_find);
				}
				
				
				if(itemData.id.Contains("tutorial"))
				{
					tmpWeight = 0;	
				}else{
					if(itemData.type == ItemType.weapon)
					{
						if(itemData.roleID == curRole || itemData.roleID == 9)
						{
						
						}else{
							tmpWeight = 0;	
						}
					}
				}
			}
			int weight = (int)tmpWeight;
			
            total += weight;
        }


        int random = UnityEngine.Random.Range(0, total);
        int tmp = 0;
        for (int i = 0; i < list.Count; i++)
        {
			
			if(list[i] == null)
			{
				Debug.LogError("list[i] == null [" + i +"]");
				Assertion.Check(false);
				continue;
			}
			
			ItemData itemData = getItemData(list[i]._itemId);
			float tmpWeight = list[i]._weight;
			if(itemData != null)
			{
				
				if(itemData.id == null)
				{
					Debug.LogError("itemData._id == null [" + i +"]");
					Assertion.Check(false);
					continue;
				}
				
				
				if(itemData.rareLevel >=2)
				{
					tmpWeight *= (1+ item_find);
				}
				
				if(itemData.id.Contains("tutorial"))
				{
					tmpWeight = 0;	
				}else{
					if(itemData.type == ItemType.weapon)
					{
						if(itemData.roleID == curRole || itemData.roleID == 9)
						{
						
						}else{
							tmpWeight = 0;	
						}
					}
				}
			}
			int weight = (int)tmpWeight;

			
            tmp += weight;
            if (random < tmp)
            {
                return list[i];
            }
        }

        Debug.LogError("GetRandomLootWeight can't find");
        return null;
    }


    private void GetOneLoot(List<LootObjData> dataList, LootData lootData)
    {
        LootWeight lootWeight = GetRandomLootWeight(lootData._lootList);

        if (lootWeight != null)
        {
            LootObjData lootObj = new LootObjData();
            lootObj._lootId = lootWeight._itemId;
            lootObj._lootCount = UnityEngine.Random.Range(lootWeight._countMin, lootWeight._countMax); ;
            dataList.Add(lootObj);
        }

    }
	
	private ItemData getItemData(string id)
	{
		return DataManager.Instance.GetItemData(id);
	}

    private void LootWithScore(List<LootObjData> dataList, LootData lootData, float score)
    {
        int count = 0;
        while (score > 0)
        {
            LootWeight lootWeight = GetRandomLootWeight(lootData._lootList);
            if (lootWeight != null)
			{
                int lootCount = UnityEngine.Random.Range(lootWeight._countMin, lootWeight._countMax);

                float lootScore = 0;
                if (lootWeight._itemId == "money")
                {
                    lootScore = lootCount;
                }
                else
                {
                    ItemData data = getItemData(lootWeight._itemId);
                    
                    lootScore = lootCount * (data  == null ? 0 : 0);
                }

                //Debug.Log("score: "+ score + "     :" + lootWeight._itemId + "    "+ lootCount + "    " +lootScore  +"    " + (lootScore < score * 2));


                if (lootScore < score * 2)
                {
                    score -= lootScore;
                    LootObjData lootObj = new LootObjData();
                    lootObj._lootId = lootWeight._itemId;
                    lootObj._lootCount = lootCount;
                    dataList.Add(lootObj);
                }
            }

            count++;
            if (count >= _lootMax)
            {
                //Debug.LogWarning("CalLootWithScore:["+count+"]  totalScore:" + totalScore + " remainScore:"+ score);
                break;
            }
        }

        /*
        string result = "result :   ";
        foreach(LootObjData tmp in dataList)
        {
            result +=  tmp._lootId +  "   ";
        }
        Debug.Log(result);
        */
    }


    public void LootList(Vector3 pos, Vector3 dir, LOOTTYPE shape, List<LootObjData> dataList, List<Vector3> lootOffest)
    {
		if(dataList.Count == 0)
		{
			return;
		}
		
        if (lootOffest == null)
        {
            lootOffest = new List<Vector3>();
        }

        if (lootOffest.Count < dataList.Count)
        {
            FillOffset(lootOffest, dataList.Count, dir, shape);
        }
		
		
		
		bool haveHighItem = false;
		bool haveMoney = false;
		bool haveNormalItem = false;
        for (int i = 0; i < dataList.Count; i++)
        {
			Debug.Log("Loot object [" + dataList[i]._lootId + "] from enemy");
			StartLootOneMove(dataList[i], pos, lootOffest[i]);
			
			ItemData itemData = getItemData(dataList[i]._lootId);
			
			if(itemData != null)
			{
				if(itemData.rareLevel >=2)
				{
					haveHighItem = true;
				}
				
				
				if(itemData.id == "money")
				{
					haveMoney = true;	
				}else{
					haveNormalItem = true;	
				}
			}
        }
		
	
		if(dataList.Count >0)
		{
			StartCoroutine(DelayPlaySound(haveNormalItem,haveHighItem,haveMoney, _time));
		}
	
    }
	
	
	IEnumerator DelayPlaySound(bool haveNormalItem, bool haveHighItem, bool haveMoney, float time)
	{
		yield return new WaitForSeconds(time);
		
		if(haveNormalItem)
		{
			if(haveHighItem)
			{
				SoundManager.Instance.PlaySoundEffect("item_high_drop");	
			}else{
				SoundManager.Instance.PlaySoundEffect("item_drop");
			}
		}

		
		if(haveMoney)
		{
			SoundManager.Instance.PlaySoundEffect("money_drop");
		}
	}

	public void PrepareOneLootPrefab(LootObjData lootObjData)
	{
        GameObject lootMovePrefab = null; //move trace
        GameObject lootObjPrefab = null; //loot obj
        GameObject lootParticlePrefab = null; //particle	
		
		//creata move object
		GameObject moveObj = new GameObject("Loot[" + lootObjData._lootId + "]");
		lootObjData._lootObject = moveObj;
        LootMove move = moveObj.AddComponent<LootMove>();	
		//add move obj to root
		move.transform.parent = _lootRoot;
		Debug.Log("prepare loot object " + lootObjData._lootId + ":" + lootObjData._lootObject.name);
		
		//determine the 3 prefabs
        int particleIndex = 0;
        if (lootObjData._lootId == "money")
        {
            lootMovePrefab = _lootMoveParticlePrefab;

            if (lootObjData._lootCount <= _smallScMax)
            {
                lootObjPrefab = _lootSmallScPrefab;
                particleIndex = 0;
            }
            else if (lootObjData._lootCount <= _middleScMax)
            {
                lootObjPrefab = _lootMiddleScPrefab;
                particleIndex = 1;
            }
            else
            {
                lootObjPrefab = _lootLargeScPrefab;
                particleIndex = 2;
            }
        }
        else
        {
            ItemData itemData = getItemData(lootObjData._lootId);

            if (itemData == null)
            {
                Destroy(moveObj);
                Debug.LogError("Loot id (" + lootObjData._lootId + ") not found.");
                return;
            }

            lootMovePrefab = _lootMoveMathfPrefab;

            if (itemData.subType == ItemSubType.weapon)
            {
				// get entity.
				int role = (int)PlayerInfo.Instance.Role;
				string entityPath = itemData.instance;
				GameObject entity = InJoy.AssetBundles.AssetBundles.Load(entityPath) as GameObject;
				FCEquipmentsBase eb = entity.GetComponent<FCEquipmentsBase>();
				Assertion.Check(eb != null);
				eb = EquipmentAssembler.Singleton.CheckEquipmentResource(eb);
				// get model.
				Assertion.Check(eb._equipmentSlots.Length > 0);
				GameObject model = InJoy.AssetBundles.AssetBundles.Load(eb._equipmentSlots[0]._modelPath) as GameObject;
				if(model == null) {
					Debug.LogError("[Loot Manager] Can not find asset " + eb._equipmentSlots[0]._modelPath);
				}
                lootObjPrefab = model;
            }
            else
            {
				string path;

				if (itemData.subType == ItemSubType.none)
				{
					path = _lootArmors[(int)ItemSubType.weapon];
				}
				else
				{
					path = _lootArmors[(int)itemData.subType];
				}

				int level = itemData.rareLevel;
				string []levelsuffix = new string[]{"0", "0", "1", "2", "2", "2", "2", "2"};
				path = path.Replace(".prefab", levelsuffix[level] + ".prefab");
				GameObject model = InJoy.AssetBundles.AssetBundles.Load(path) as GameObject;
				if(model == null) {
					Debug.LogError("[Loot Manager] Can not find asset " + path);
				}
                lootObjPrefab = model;
            }

            particleIndex = itemData.rareLevel;
        }

        if (particleIndex > _lootParticlePath.Count - 1)
        {
            particleIndex = _lootParticlePath.Count - 1;
        }

        lootParticlePrefab = _lootParticlePrefab[particleIndex];			
			
        move.SetData(lootMovePrefab, lootObjPrefab, lootParticlePrefab, lootObjData._lootId, lootObjData._lootCount, _liftTime);
		move.PrepareMovePrefabs();
		moveObj.SetActive(false);
	}
	
    public void StartLootOneMove(LootObjData lootObjData, Vector3 pos, Vector3 offset)
	{
		Assertion.Check(lootObjData._lootObject != null);
		
		//active the move object
		lootObjData._lootObject.SetActive(true);
		
		//set transfrom and start move
		LootMove move = lootObjData._lootObject.GetComponent<LootMove>();
		move.transform.localPosition = pos;
		move.StartMove(offset, _time);
	}
	
	
	//WARNING!!!!!!!!!!!   only use for cheat drop!!!
    public void LootOneForCheatDrop(LootObjData lootObjData, Vector3 pos, Vector3 offset)
    {
        GameObject moveObj = new GameObject("Loot[" + lootObjData._lootId + "]");
        LootMove move = moveObj.AddComponent<LootMove>();

        move.transform.parent = _lootRoot;
        move.transform.localPosition = pos;

        GameObject lootMovePrefab = null;
        GameObject lootObjPrefab = null;
        GameObject lootParticlePrefab = null;
        int particleIndex = 0;
		

		
        if (lootObjData._lootId == "money")
        {
            lootMovePrefab = _lootMoveParticlePrefab;

            if (lootObjData._lootCount <= _smallScMax)
            {
                lootObjPrefab = _lootSmallScPrefab;
                particleIndex = 0;
            }
            else if (lootObjData._lootCount <= _middleScMax)
            {
                lootObjPrefab = _lootMiddleScPrefab;
                particleIndex = 1;
            }
            else
            {
                lootObjPrefab = _lootLargeScPrefab;
                particleIndex = 2;
            }
        }
        else
        {
			lootMovePrefab = _lootMoveMathfPrefab;
			
            ItemData itemData = getItemData(lootObjData._lootId);

            if (itemData.subType == ItemSubType.weapon)
            {
				// get entity.
				int role = (int)PlayerInfo.Instance.Role;
				string entityPath = itemData.instance;
				GameObject entity = InJoy.AssetBundles.AssetBundles.Load(entityPath) as GameObject;
				FCEquipmentsBase eb = entity.GetComponent<FCEquipmentsBase>();
				Assertion.Check(eb != null);
				// get model.
				Assertion.Check(eb._equipmentSlots.Length > 0);
				GameObject model = InJoy.AssetBundles.AssetBundles.Load(eb._equipmentSlots[0]._modelPath) as GameObject;
				if(model == null) {
					Debug.LogError("[Loot Manager] Can not find asset " + eb._equipmentSlots[0]._modelPath);
				}
                lootObjPrefab = model;
            }
            else
            {
				string path = _lootArmors[(int)itemData.subType];
				int level = itemData.rareLevel;
				string []levelsuffix = new string[]{"0", "0", "1", "2", "2", "2", "2", "2"};
				path = path.Replace(".prefab", levelsuffix[level] + ".prefab");
				GameObject model = InJoy.AssetBundles.AssetBundles.Load(path) as GameObject;
				if(model == null) {
					Debug.LogError("[Loot Manager] Can not find asset " + path);
				}
                lootObjPrefab = model;
            }

            particleIndex = itemData.rareLevel;
        }

        if (particleIndex > _lootParticlePath.Count - 1)
        {
            particleIndex = _lootParticlePath.Count - 1;
        }

        lootParticlePrefab = _lootParticlePrefab[particleIndex];

        move.SetData(lootMovePrefab, lootObjPrefab, lootParticlePrefab, lootObjData._lootId, lootObjData._lootCount, _liftTime);
        move.StartMoveForCheatDrop(offset, _time);
    }


    private void FillOffset(List<Vector3> lootOffset, int count, Vector3 dir, LOOTTYPE shape)
    {
		
		shape = LOOTTYPE.INCIRCLE;
        count -= lootOffset.Count;

        if (shape == LOOTTYPE.RANDOM)
        {
            shape = (LOOTTYPE)UnityEngine.Random.Range(2, (int)(LOOTTYPE.MAX - 1));
        }

        if (shape == LOOTTYPE.GRID)
        {
            FillOffsetGrid(lootOffset, count, 3, 2);
        }
        else if (shape == LOOTTYPE.ARC)
        {
            FillOffsetArc(lootOffset, count, dir, 3, 45);
        }
        else if (shape == LOOTTYPE.LINE)
        {
            FillOffsetLine(lootOffset, count, dir, 2.5f, 1);
        }
        else if (shape == LOOTTYPE.CIRCLE)
        {
            FillOffsetCircle(lootOffset, count, 3);
        }
        else if (shape == LOOTTYPE.INCIRCLE)
        {
            FillOffsetInCircle(lootOffset, count, 3);
        }
        else
        {
            FillOffsetVerticle(lootOffset, count);
        }
    }


    private void FillOffsetVerticle(List<Vector3> lootOffset, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3(0, _height, 0);
            lootOffset.Add(offset);
        }
    }


    private void FillOffsetInCircle(List<Vector3> lootOffset, int count, float radius)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3(0, _height, 0);

            Vector2 tmp = UnityEngine.Random.insideUnitCircle;
            tmp *= radius;
            offset.x += tmp.x;
            offset.z += tmp.y;

            lootOffset.Add(offset);
        }
    }


    private void FillOffsetCircle(List<Vector3> lootOffset, int count, float radius)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3(0, _height, 0);
            int angle = UnityEngine.Random.Range(0, 360);
            float rad = angle * Mathf.Deg2Rad;
            offset.x = radius * Mathf.Cos(rad);
            offset.z = radius * Mathf.Sin(rad);

            lootOffset.Add(offset);
        }
    }


    private void FillOffsetGrid(List<Vector3> lootOffset, int count, int gridSize, float distance)
    {
        int totalSize = gridSize * gridSize;
        int begin = UnityEngine.Random.Range(0, totalSize - 1);

        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3(0, _height, 0);

            int index = (begin + i) % totalSize;
            offset.x = (index / gridSize - 1) * distance;
            offset.z = (index % gridSize - 1) * distance;

            lootOffset.Add(offset);
        }
    }


    private void FillOffsetArc(List<Vector3> lootOffset, int count, Vector3 dir, float radius, float detalAngle)
    {
        float beginAngle = Vector3.Angle(dir, Vector3.right);
        if (dir.z < 0)
        {
            beginAngle = 360 - beginAngle;
        }

        beginAngle -= (count - 1) * detalAngle / 2;

        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3(0, _height, 0);
            float rad = beginAngle * Mathf.Deg2Rad;
            offset.x = radius * Mathf.Cos(rad);
            offset.z = radius * Mathf.Sin(rad);
            beginAngle += detalAngle;

            lootOffset.Add(offset);
        }
    }

    private void FillOffsetLine(List<Vector3> lootOffset, int count, Vector3 dir, float radius, float detalDistance)
    {
        dir.Normalize();

        Vector3 detalOffset = dir * radius;

        for (int i = 0; i < count; i++)
        {
            Vector3 tmp = new Vector3(0, _height, 0);


            float ang = Vector3.Angle(dir, Vector3.right);
            if (dir.z < 0)
            {
                ang = 360 - ang;
            }

            ang += 90;
            float rad = ang * Mathf.Deg2Rad;


            float len = (i + 1) / 2 * detalDistance;
            if (i % 2 == 0)
            {
                len *= -1;
            }

            tmp.x += len * Mathf.Cos(rad);
            tmp.z += len * Mathf.Sin(rad);

            tmp += detalOffset;

            lootOffset.Add(tmp);
        }
    }
}
