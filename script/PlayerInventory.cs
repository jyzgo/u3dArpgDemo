using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using InJoy.Utils;
using System.Reflection;
using FaustComm;

[System.Serializable]
public class ItemInventory : ServerMessage
{
    private long _item_GUID;
    public long Item_GUID { get { return _item_GUID; } set { _item_GUID = value; } }

    private string _itemID;
    public string ItemID { get { return _itemID; } set { _itemID = value; } }

    private int _count;
    public int Count { get { return _count; } set { _count = value; } }//shuliang

    private bool _isNew = false;
    public bool IsNew { get { return _isNew; } set { _isNew = value; } }

    private Dictionary<FC_EQUIP_EXTEND_ATTRIBUTE, int> _extendAttributes = new Dictionary<FC_EQUIP_EXTEND_ATTRIBUTE, int>();
    public Dictionary<FC_EQUIP_EXTEND_ATTRIBUTE, int> ExtendAttributes { get { return _extendAttributes; } set { _extendAttributes = value; } }


    public ItemInventory()
    { 
    }

	public override void Parse(System.IO.BinaryReader reader)
	{
		_item_GUID = reader.ReadInt64();
		_itemID = NetResponse.ReadString(reader);
		_count = reader.ReadInt16();

        byte itemPropNum = reader.ReadByte();

        for (int i = 0; i < itemPropNum; i++)
        {
            FC_EQUIP_EXTEND_ATTRIBUTE attKey = (FC_EQUIP_EXTEND_ATTRIBUTE)reader.ReadInt32();
            int attValue = reader.ReadInt32();

            if (_extendAttributes.ContainsKey(attKey))
            {
                _extendAttributes[attKey] = attValue;
            }
            else
            {
                _extendAttributes.Add(attKey, attValue);
            }
        }        
	}

    public int GetEquipExtendAttributeValueByKey(FC_EQUIP_EXTEND_ATTRIBUTE key)
    {
        if (_extendAttributes.ContainsKey(key))
        {
            return _extendAttributes[key];
        }
        else
        {
            return 0;
        }
    }

    public void SetEquipExtendAttributeValueByKey(FC_EQUIP_EXTEND_ATTRIBUTE attKey, int attValue)
    {
        if (_extendAttributes.ContainsKey(attKey))
        {
            _extendAttributes[attKey] = attValue;
        }
        else
        {
            _extendAttributes.Add(attKey, attValue);
        }
    }

    public int CurrentFusionLevel()
    {
        return GetEquipExtendAttributeValueByKey(FC_EQUIP_EXTEND_ATTRIBUTE.FUSION);
    }

    public string GetFusionAttribute0(bool isCurrent)
    {
        if (ItemData.attrId0 != AIHitParams.None)
        {
            FusionData fd = isCurrent ? CurrentFusionData : NextFusionData;
            float increateData = null != fd ? fd.increaseData : 0;
            float value = ItemData.attrValue0;
            float valueExtra = ItemData.attrValue0 * increateData;
            if (increateData == 0)
                return DataManager.Instance.GetAttriubteDisplay(ItemData.attrId0, value);
            else
                return DataManager.Instance.GetAttriubteDisplay(ItemData.attrId0, value) +
                    DataManager.Instance.GetExtraAttributeValueDisplay(ItemData.attrId0, valueExtra);
        }
        else
            return "";
    }

    public string GetFusionAttribute1(bool isCurrent)
    {
        if (ItemData.attrId1 != AIHitParams.None)
        {
            FusionData fd = isCurrent ? CurrentFusionData : NextFusionData;
            float increateData = null != fd ? fd.increaseData : 0;
            float value = ItemData.attrValue1;
            float valueExtra = ItemData.attrValue1 * increateData;
            if (increateData == 0)
                return DataManager.Instance.GetAttriubteDisplay(ItemData.attrId1, value);
            else
                return DataManager.Instance.GetAttriubteDisplay(ItemData.attrId1, value) +
                    DataManager.Instance.GetExtraAttributeValueDisplay(ItemData.attrId1, valueExtra);
        }
        else
            return "";
    }

    public string GetFusionAttribute2(bool isCurrent)
    {
        if (ItemData.attrId2 != AIHitParams.None)
        {
            FusionData fd = isCurrent ? CurrentFusionData : NextFusionData;
            float increateData = null != fd ? fd.increaseData : 0;
            float value = ItemData.attrValue2;
            float valueExtra = ItemData.attrValue2 * increateData;
            if (increateData == 0)
                return DataManager.Instance.GetAttriubteDisplay(ItemData.attrId2, value);
            else
                return DataManager.Instance.GetAttriubteDisplay(ItemData.attrId2, + value) +
                    DataManager.Instance.GetExtraAttributeValueDisplay(ItemData.attrId2, valueExtra);
        }
        else
            return "";
    }

    public EquipmentAttributeVo AttributeWithFusion0
    {
        get 
        { 
            FusionData fd = CurrentFusionData;
            float increateData = 0;
            if (null != fd)
            {
                increateData = fd.increaseData;
            }
            float newValue = ItemData.attrValue0 * (1 + increateData);
            return new EquipmentAttributeVo(ItemData.attrId0, newValue);
        }
    }

    public EquipmentAttributeVo AttributeWithFusion1
    {
        get
        {
            FusionData fd = CurrentFusionData;
            float increateData = 0;
            if (null != fd)
            {
                increateData = fd.increaseData;
            }
            float newValue = ItemData.attrValue1 * (1 + increateData);
            return new EquipmentAttributeVo(ItemData.attrId1, newValue);
        }
    }

    public EquipmentAttributeVo AttributeWithFusion2
    {
        get
        {
            FusionData fd = CurrentFusionData;
            float increateData = 0;
            if (null != fd)
            {
                increateData = fd.increaseData;
            }
            float newValue = ItemData.attrValue2 * (1 + increateData);
            return new EquipmentAttributeVo(ItemData.attrId2, newValue);
        }
    }

    public FusionData CurrentFusionData
    {
        get 
        {
            int currentFusionLevel = CurrentFusionLevel();
            if (currentFusionLevel > 0)
                return DataManager.Instance.GetFusionData(currentFusionLevel, ItemData.level, ItemData.type);
            else
                return null;
        }
    }

    public FusionData NextFusionData
    {
        get 
        {
            if (DataManager.Instance.FusionDataExit(CurrentFusionLevel() + 1, ItemData.level, ItemData.type))
                return DataManager.Instance.GetFusionData(CurrentFusionLevel() + 1, ItemData.level, ItemData.type);
            else
                return null;
        }
    }

    public float GetHitValueByParams(AIHitParams paramsKey)
    {
        if (null != CurrentFusionData)
        {
            return ItemData.GetHitValueByParams(paramsKey) * (1 + CurrentFusionData.increaseData);
        }
        return ItemData.GetHitValueByParams(paramsKey);
    }

    public int GetFSScore()
    {
        ItemData itemData = ItemData;
        return (int)(itemData.attrValue0 + itemData.attrValue1 + itemData.attrValue2);
    }
	
	public int DisplayRareLevel
	{
        get { return ItemData.rareLevel; }
	}

    public bool IsCanFusion()
    {
        if (IsEquipment())
        {
            return true;
        }
        return false;
    }
	
	public bool IsEquipment()
	{
		return IsEquipment(ItemData);
	}
	
	
	public static bool IsEquipment(ItemData itemData)
	{
		if(itemData.type == ItemType.armor 
			|| itemData.type == ItemType.ornament
			|| itemData.type == ItemType.weapon)
		{
			return true;
		}else{
			return false;	
		}
	}

    public bool IsAvatar()
    {
        return IsAvatar(ItemData);
    }

    public static bool IsAvatar(ItemData itemdata)
    {
        if (ItemType.weapon == itemdata.type ||
            ItemType.armor == itemdata.type
        )
            return true;
        else
            return false;
    }
		
	private ItemData _itemData = null;

	public ItemData ItemData
	{
		get
        {
			_itemData = DataManager.Instance.GetItemData(ItemID);

			return _itemData;
		}	
	}
	
				
	public static string GetUpStringWithoutBracket(string text)
	{
		return "[00ff00]" + GetUpArrow() + text + "[-]";
	}
	
	public static string GetDownStringWithoutBracket(string text)
	{
		return "[ff0000]" + GetDownArrow() + text + "[-]";
	}
	
	public static string GetRightStringWithoutBracket(string text)
	{
		return "[00ff00]" + GetRightArrow() + text + "[-]";
	}
	
	
	public static string GetUpString(string text)
	{
		return "[00ff00](" + GetUpArrow() + text + ")[-]";
	}
	
	public static string GetDownString(string text)
	{
		return "[ff0000](" + GetDownArrow() + text + ")[-]";
	}
	
	public static string GetRightString(string text)
	{
		return "[00ff00](" + GetRightArrow() + text + ")[-]";
	}
	
	
	public static string GetUpArrow()
	{
		return "\u25b2";	
	}
	
	public static string GetDownArrow()
	{
		return "\u25BC";	
	}
	
	public static string GetRightArrow()
	{
		return "\u2192";	
	}
		
	public bool IsEquiped()
	{
		return PlayerInfo.Instance.isEquipped(this);	
	}
	
	public bool IsCanPreview()
	{
		if(IsEquipment() || ItemData.type == ItemType.vanity)
		{
			if(ItemData.roleID == (int)PlayerInfo.Instance.Role
				|| ItemData.roleID == 9)
			{
				return true;
			}
		}
		return false;	
	}
	
	public bool IsLevelEnable()
	{
		return PlayerInfo.Instance.CurrentLevel >= ItemData.enableLevel;
	}
	
	public bool IsCanUse()
	{
		if(IsEquipment())
		{
			return IsCanEquip();
		}
		return true;	
	}
	
	public bool IsCanEquip()
	{
		if(IsLevelEnable() && IsCanPreview())
		{
			return true;
		}
		return false;
	}
	
    public int FS
    {
        get 
        {
            float sum = 0;
            EquipmentAttributeVo[] attrs = new EquipmentAttributeVo[]{AttributeWithFusion0, AttributeWithFusion1, AttributeWithFusion2};
            foreach (EquipmentAttributeVo av in attrs) 
            {
                if (av.attribute != AIHitParams.None)
                {
                    EquipmentFSData fsData = DataManager.Instance.EquipmentFSDataList.GetFSDataByAttribute(av.attribute);
                    float value = av.value;
                    if (fsData != null)
                    {
                        sum += fsData.fs * value;
                    }
                }
            }
            return (int)sum;
        }
    }
    
    public static int SortCompare(ItemInventory a, ItemInventory b)
    {
        if (a.ItemData.type < b.ItemData.type)
        {
            return -1;
        }
        else if (a.ItemData.type > b.ItemData.type)
        {
            return 1;
        }
        else
        {
            ItemSubType aSubType = a.ItemData.subType;
            ItemSubType bSubType = b.ItemData.subType;

            if (aSubType == ItemSubType.potion_health)
                aSubType = ItemSubType.potion_mana;
            else if (aSubType == ItemSubType.potion_mana)
                aSubType = ItemSubType.potion_health;

            if (bSubType == ItemSubType.potion_health)
                bSubType = ItemSubType.potion_mana;
            else if (bSubType == ItemSubType.potion_mana)
                bSubType = ItemSubType.potion_health;

            if (aSubType < bSubType)
            {
                return -1;
            }
            else if (aSubType > bSubType)
            {
                return 1;
            }
            else
            {
                if (PlayerInfo.Instance.CurrentLevel >= a.ItemData.enableLevel
                    &&
                    PlayerInfo.Instance.CurrentLevel < b.ItemData.enableLevel)
                {
                    return -1;
                }
                else if (PlayerInfo.Instance.CurrentLevel < a.ItemData.enableLevel
                    &&
                    PlayerInfo.Instance.CurrentLevel >= b.ItemData.enableLevel)
                {
                    return 1;
                }
                else
                {
                    if (a.ItemData.level > b.ItemData.level)
                    {
                        return -1;
                    }
                    else if (a.ItemData.level < b.ItemData.level)
                    {
                        return 1;
                    }
                    else
                    {
                        if (a.ItemData.rareLevel > b.ItemData.rareLevel)
                        {
                            return -1;
                        }
                        else if (a.ItemData.rareLevel < b.ItemData.rareLevel)
                        {
                            return 1;
                        }
                        else
                        {
                            if (a.FS > b.FS)
                            {
                                return -1;
                            }
                            else if (a.FS < b.FS)
                            {
                                return 1;
                            }
                            else
                            {
                                return (int)((uint)b.Item_GUID - (uint)a.Item_GUID);
                            }
                        }
                    }
                }
            }
        }
    }

    public int CostSC
    {
        get
        {
            if (IsEquipment())
            {
                float sum = 0;
                EquipmentAttributeVo[] attrs = new EquipmentAttributeVo[] { AttributeWithFusion0, AttributeWithFusion1, AttributeWithFusion2 };
                foreach (EquipmentAttributeVo av in attrs)
                {
                    if (av.attribute != AIHitParams.None)
                    {
                        EquipmentFSData fsData = DataManager.Instance.EquipmentFSDataList.GetFSDataByAttribute(av.attribute);
                        float value = av.value;
                        if (fsData != null)
                        {
                            sum += fsData.sc * value * ItemData.sellCount;
                        }
                    }
                }
                return (int)sum;
            }
            else
            {
                return (int)(ItemData.sellCount * Count);
            }
        }
    }
}



[System.Serializable]
public class PlayerInventory : ServerMessage
{
	private List<ItemInventory> _itemList = new List<ItemInventory>();
	public List<ItemInventory> itemList { get { return _itemList; } }
	public Dictionary<string, int> Consumables = new Dictionary<string, int>();

	public int Count { get { return _itemList.Count; } }

	public void AddItemInventory(ItemData itemData, int count)
	{
		if (itemData.type == ItemType.material
		|| itemData.type == ItemType.potion
		|| itemData.type == ItemType.gem)
		{
			ItemInventory itemInventory = GetItem(itemData.id);
			if (itemInventory != null)
			{
				itemInventory.Count += count;
				return;
			}
		}

		ItemInventory newItemInventory = new ItemInventory();
		newItemInventory.ItemID = itemData.id;
		newItemInventory.Count = count;
		newItemInventory.IsNew = true;

		_itemList.Add(newItemInventory);
	}

	public void AddItemInventory(ItemInventory newItem)
	{
		_itemList.Add(newItem);
	}


	public void Clear()
	{
		_itemList.Clear();
	}

	public int GetItemCount(string itemDataId)
	{
		int total = 0;
		foreach (ItemInventory item in _itemList)
		{
			if (item.ItemID == itemDataId)
			{
				if (GameManager.Instance.GameState == EnumGameState.InBattle)
				{
					if (item.ItemData.type == ItemType.potion)
					{
						total += (item.Count - GetConsumables(item.ItemID));
					}
					else
					{
						total += item.Count;
					}
				}
				else
				{
					total += item.Count;
				}
			}
		}
		return total;
	}


	public ItemInventory GetItem(long itemGUID)
	{
		return _itemList.Find(delegate(ItemInventory item) { return item.Item_GUID == itemGUID; });
	}

	public ItemInventory GetItem(string itemId)
	{
		return _itemList.Find(delegate(ItemInventory item) { return item.ItemID == itemId; });
	}

	public ItemInventory GetItem(ItemType itemType, ItemSubType part)
	{
		return _itemList.Find(delegate(ItemInventory item) { return item.ItemData.type == itemType && item.ItemData.subType == part; });
	}

	public List<ItemInventory> GetItemList(string itemId)
	{
		return _itemList.FindAll(delegate(ItemInventory item) { return item.ItemID == itemId; });
	}

	public List<ItemInventory> GetItemLisyByType(ItemType type)
	{
		return _itemList.FindAll(delegate(ItemInventory item) { return item.ItemData.type == type; });
	}

	public void UseItem(string itemId)
	{
		foreach (ItemInventory item in _itemList)
		{
			if (item.ItemID == itemId)
			{
				SetConsumables(itemId, 1, true);
			}
		}
	}

	public void UpdataItemCount(long itemGUID, int count)
	{
		ItemInventory itemInventory = GetItem(itemGUID);

		if (null != itemInventory)
		{
			itemInventory.Count = count;
			if (count == 0)
				RemoveItem(itemInventory);
		}
	}


	public ItemInventory RemoveItem(long itemGUID, int count)
	{
		ItemInventory item = _itemList.Find(delegate(ItemInventory ii) { return ii.Item_GUID == itemGUID; });
		if (null != item)
		{
			if (item.Count >= count)
			{
				item.Count -= count;
			}

			if (GameManager.Instance.GameState == EnumGameState.InBattle)
			{
				if (item.ItemData.type == ItemType.potion)
				{
					SetConsumables(item.ItemID, count, false);
				}
			}

			if (item.Count <= 0)
			{
				_itemList.Remove(item);
			}
		}
		return item;
	}

	public void RemoveItem(ItemInventory item, int count = 1)
	{
		RemoveItem(item.Item_GUID, count);
	}

	private int GetConsumables(string key)
	{
		if (!Consumables.ContainsKey(key))
		{
			Consumables.Add(key, 0);
		}

		return Consumables[key];
	}

	private void SetConsumables(string key, int value, bool isAdd)
	{
		if (!Consumables.ContainsKey(key))
		{
			Consumables.Add(key, 0);
		}

		if (isAdd)
		{
			Consumables[key] += value;
		}
		else
		{
			if (Consumables[key] >= value)
			{
				Consumables[key] -= value;
			}
		}
	}


	public void ChangeConsumables(string key, int value, bool isAdd = false)
	{
		if (Consumables.ContainsKey(key))
		{
			if (isAdd)
			{
				Consumables[key] += value;
			}
			else
			{
				Consumables[key] = value;
			}
		}
		else
		{
			Consumables.Add(key, value);
		}
	}


	public bool HasNewItemInInventory()
	{
		foreach (ItemInventory itemInventory in _itemList)
		{
			if (itemInventory.IsNew)
			{
				return true;
			}
		}
		return false;
	}

	public void ClearNewItemInInventory()
	{
		foreach (ItemInventory itemInventory in _itemList)
		{
			itemInventory.IsNew = false;
		}
	}
    
    public int EquipmentsFS
    {
        get
        {
            int sum = 0;
            foreach(ItemInventory item in itemList)
            {
                sum += item.FS;
            }
            return sum;
        }
    }

	public void SortByScore()
	{
		_itemList.Sort(ScoreCompare);
	}

	private int ScoreCompare(ItemInventory item1, ItemInventory item2)
	{
		int score1 = item1.GetFSScore();
		int score2 = item2.GetFSScore();

		if (score1 > score2)
		{
			return 1;
		}
		else if (score1 < score2)
		{
			return -1;
		}
		else
		{
			return 0;
		}
	}


	public override void Parse(System.IO.BinaryReader reader)
	{
		_itemList.Clear();

		int count = reader.ReadInt16();

		for (int i = 0; i < count; i++)
		{
			ItemInventory ii = new ItemInventory();
			ii.Parse(reader);

			_itemList.Add(ii);
		}
	}

	public void ApplyItemCountChanges(List<ItemCountVo> itemCountOps)
	{
		foreach (ItemCountVo vo in itemCountOps)
		{
			UpdataItemCount(vo.ItemGuid, vo.ItemCount);

			Debug.Log(string.Format("Parser Item count change. GUID = {0}, count = {1}", vo.ItemGuid, vo.ItemCount));
		}
	}

	public bool HasSufficientMaterials(TattooData td)
	{
		bool result = true;
		foreach (MatAmountMapping mapping in td.materials)
		{
			if (GetItemCount(mapping.materialName) < mapping.amount)
			{
				result = false;
				break;
			}
		}
		return result;
	}
}

public class PlayerTattoos : ServerMessage
{
	//contains all tattoos burned on body, not including those empty but available slots
	public Dictionary<EnumTattooPart, ItemInventory> tattooDict = new Dictionary<EnumTattooPart, ItemInventory>();

	public override void Parse(System.IO.BinaryReader reader)
	{
		tattooDict = new Dictionary<EnumTattooPart, ItemInventory>();

		int count = reader.ReadByte();

		for (int i = 0; i < count; i++)
		{
			EnumTattooPart part = (EnumTattooPart)reader.ReadByte();

			ItemInventory ii = new ItemInventory();

			ii.Parse(reader);

			tattooDict.Add(part, ii);
		}
	}

	//if a tattoo exists on the boday part
	public bool IsEuipped(EnumTattooPart part)
	{
		return tattooDict.ContainsKey(part);
	}

	//if a body slot has been unlocked
	private bool IsAvailable(EnumTattooPart part)
	{
		return false;
	}

	public ItemInventory GetItemByGUID(long guid)
	{
		foreach (ItemInventory item in tattooDict.Values)
		{
			if (item.Item_GUID == guid)
			{
				return item;
			}
		}
		return null;
	}

	public int GetTattooCount(string tattooID)
	{
		int count = 0;

		foreach (ItemInventory item in tattooDict.Values)
		{
			if (item.ItemID == tattooID)
			{
				count++;
			}
		}
		return count;
	}
}
