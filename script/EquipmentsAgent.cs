using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AC/EquipmentsAgent")]
public class EquipmentsAgent : FCObject ,FCAgent {
	
	#region data
	public GameObject _equipmentsAgent;
	private List<FCWeapon> _weaponList;
	private ActionController _owner;
	private bool _hasInitWeaponAndArmorData = false;
	
	public bool _debugEquipment = false;
	#endregion
	
	public List<FCWeapon> WeaponList
	{
		get
		{
			return _weaponList;
		}
	}
	public static string GetTypeName()
	{
		return "EquipmentsAgent";
	}
	
	public void Init(FCObject owner)
	{
		_owner = owner as ActionController;
		//EquipAll();
	}
	
	#region logic
	
	protected override void Awake()
	{
		base.Awake();
		_weaponList = new List<FCWeapon>();
	}
	
	public void RefreshWeaponList()
	{
		_weaponList.Clear();
		FCEquipmentsBase[] eebs = _equipmentsAgent.GetComponentsInChildren<FCEquipmentsBase>();
		foreach(FCEquipmentsBase eeb in eebs)
		{
			if(eeb != null && eeb.ObjectID.ObjectType == FC_OBJECT_TYPE.OBJ_WEAPON)
			{
				_weaponList.Add(eeb as FCWeapon);
			}
		}
	}

    //return the root of the equipments
    public void EquipAllModule()
    {
        List<GameObject> equipmentInstanceList = new List<GameObject>();

        if (!_debugEquipment) //use data from server
        {
            if (_owner.Data.isPlayer)
            {
                if (_owner.IsPlayerSelf ||  (PhotonNetwork.room == null))
                {
                    PlayerInfo.Instance.GetSelfEquipmentInstance(equipmentInstanceList);
                }
                else
                {
					//players from other clients
                    MatchPlayerProfile otherInfo = MatchPlayerManager.Instance.GetMatchPlayerProfile(_owner._instanceID);
                    if (otherInfo != null)
                    {
                        PlayerInfo.GetOtherEquipmentInstanceWithIds(equipmentInstanceList, otherInfo._playerInfo.equipIds);
                    }
                }
            }
            else
            {
                PlayerInfo.GetOtherEquipmentInstanceWithIds(equipmentInstanceList, _owner.EquipmentIds);
            }
        }
        else //use prefab equipment data
        {
            GameObject[] objs = _equipmentsAgent.GetComponent<EquipmentsList>()._equipList;

            foreach (GameObject obj in objs)
            {
                GameObject instanceObj = GameObject.Instantiate(obj) as GameObject;
                equipmentInstanceList.Add(instanceObj);
            }
        }

        foreach (GameObject go in equipmentInstanceList)
        {
            go.transform.parent = _equipmentsAgent.transform;
        }

        _owner.GetComponent<AvatarController>().RefreshEquipments(_equipmentsAgent.transform);
    }
	
		
	public void EquipmentAllData()
	{
        //if(!_hasInitWeaponAndArmorData && _owner.IsPlayerSelf)
        //{	
        //    CalculateData();
        //    _hasInitWeaponAndArmorData = true;
        //}

        if (_owner.IsPlayerSelf)
        {
            CalculateData();
        }       
	}
		
	private void CalculateData()
	{
		PlayerInfo profile = PlayerInfo.Instance;
		
		List<ItemInventory> itemList = new List<ItemInventory>();

		foreach (ItemInventory ii in PlayerInfo.Instance.EquippedInventory.itemList)
		{
			itemList.Add(ii);
		}

		itemList.AddRange(PlayerInfo.Instance.playerTattoos.tattooDict.Values);

		foreach (ItemInventory item in itemList)
        {
            if (null != item)
            {
                HandleEquipmentAttribute(item);
            }
        }

		//add effects of tattoo suites
		itemList.Clear();

		itemList.AddRange(PlayerInfo.Instance.playerTattoos.tattooDict.Values);

		foreach (TattooSuiteData tsd in DataManager.Instance.tattooSuiteDataList.dataList)
		{
			bool included = true;
			foreach (string tattooID in tsd.tdList)
			{
				bool found = false;
				foreach (ItemInventory item in itemList) //at most 1 unique ID
				{
					if (item.ItemID == tattooID)
					{
						found = true;
						break;
					}
				}
				included = included && found;

				if (!included)
				{
					break;
				}
			}

            if(!included)
            {
                continue;
            }

			//a suite is found
            HandleTattooSuiteAttribute(tsd);

			//remove the items from list
			foreach (string tattooID in tsd.tdList)
			{
				ItemInventory item = itemList.Find(delegate(ItemInventory ii) { return ii.ItemID == tattooID; });

				itemList.Remove(item);
			}
		}
	}

    private void HandleEquipmentAttribute(ItemInventory ii)
    {
        FusionData fusionData = ii.CurrentFusionData;

        if (null != fusionData)
        {
            _owner.Data.AddHitParamsData(ii.ItemData.attrId0, ii.ItemData.attrValue0 * (1 + fusionData.increaseData));
            _owner.Data.AddHitParamsData(ii.ItemData.attrId1, ii.ItemData.attrValue1 * (1 + fusionData.increaseData));
            _owner.Data.AddHitParamsData(ii.ItemData.attrId2, ii.ItemData.attrValue2 * (1 + fusionData.increaseData));
        }
        else
        {
            _owner.Data.AddHitParamsData(ii.ItemData.attrId0, ii.ItemData.attrValue0);
            _owner.Data.AddHitParamsData(ii.ItemData.attrId1, ii.ItemData.attrValue1);
            _owner.Data.AddHitParamsData(ii.ItemData.attrId2, ii.ItemData.attrValue2);
        }
    }

    private void HandleTattooSuiteAttribute(TattooSuiteData tsd)
    {
        _owner.Data.AddHitParamsData(tsd.attribute0, tsd.value0);
        _owner.Data.AddHitParamsData(tsd.attribute1, tsd.value1);
        _owner.Data.AddHitParamsData(tsd.attribute2, tsd.value2);
    }

	#endregion
}
