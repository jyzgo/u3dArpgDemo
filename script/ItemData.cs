using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public enum ItemSubType
{
	none = -1,
	weapon,
	helmet,
	shoulder,
	chest,
	belt,
	armpiece,
	leggings,
	ring,
	necklace,
	vanity,
	tattoo,
	potion_mana = 20,
	potion_health,
	potion_revive,
	ornament_necklace = 30,
	ornament_ring,
	tribute_A = 40,
	tribute_B,
	tribute_C,
}


public enum ItemType
{
	none = -1,
	weapon,
	armor,
	ornament,
	gem,
	potion,
	material,
	tribute,
	vanity,
	hc,
	sc,
	tattoo,
	recipe
}

[Serializable]
public class ItemData
{
    //common
    public string id;
    public ItemType type;
    public int enableLevel;
    public int roleID;
    public ItemSubType subType;
    public string nameIds;
    public string descriptionIds;
    public int level;		//item level, decides how advanced this item is, never changes
    public int rareLevel;	//rarity

    public string iconPath;
    public string instance;

    //equipment
    public float sellCount;
    public int stack;

    public AIHitParams attrId0;
    public float attrValue0;

    public AIHitParams attrId1;
    public float attrValue1;

    public AIHitParams attrId2;
    public float attrValue2;

    public int equipmentScore;


    public float GetHitValueByParams(AIHitParams paramsKey)
    {
        if (paramsKey == attrId0)
        {
            return attrValue0;
        }
        else if (paramsKey == attrId1)
        {
            return attrValue1;
        }
        else if (paramsKey == attrId2)
        {
            return attrValue2;
        }
        else
        {
            return 0;
        }
    }

    public int FS
    {
        get
        {
            float sum = 0;
            EquipmentAttributeVo[] attrs = new EquipmentAttributeVo[]
            {
                new EquipmentAttributeVo(attrId0, attrValue0),
                new EquipmentAttributeVo(attrId1, attrValue1),
                new EquipmentAttributeVo(attrId2, attrValue2)
            };
            foreach (EquipmentAttributeVo av in attrs)
            {
                if (av.attribute != AIHitParams.None)
                {
                    EquipmentFSData fsData = DataManager.Instance.EquipmentFSDataList.GetFSDataByAttribute(av.attribute);
                    float value = av.value;
                    if (av.attribute == AIHitParams.CriticalDamage || av.attribute == AIHitParams.CriticalChance)
                    {
                        value /= 1000;
                    }
                    if (fsData != null)
                    {
                        sum += fsData.fs * value;
                    }
                }
            }
            return (int)sum;
        }
    }

    public string DisplayName
    {
        get { return Localization.Localize(nameIds); }
    }

    public string DisplayNameWithRareColor
    {
        get
        {
            Color color = FCConst.RareColorMapping[(EnumRareLevel)rareLevel];
            return "[" + NGUITools.EncodeColor(color) + "]" + DisplayName + "[-]";
        }
    }
}

public enum EnumRareLevel
{
    white = 0,
    green,
    blue,
    purple,
    gold,
    MAX
}

//parts that a tattoo can be printed on
public enum EnumTattooPart
{
	Head,
	ShoulderLeft,
	ShoulderRight,
	UpperArmLeft,
	UpperArmRight,
	LowerArmLeft,
	LowerArmRight,
	Chest,
	Waist,
	ThighLeft,
	ThighRight,
	CalfLeft,
	CalfRight,
	Back
}


[System.Serializable]
public class TattooData
{
	public string tattooID;		//copy of itemData.itemID
	public string recipeID;
	public string suiteID;		//empty means no in any suite
	public int ord;
	public List<EnumTattooPart> applicableParts;	//which parts this tattoo can be applied on
	public int level;
	public int bearingPoint;
	public int learnHC;	//when player has no scroll but want to learn
	public int hcCost;	//when materials are not enough
	public int scCost;
	public List<MatAmountMapping> materials;
	public string GetDisplayName()
	{
		ItemData itemData = DataManager.Instance.GetItemData(this.tattooID);

		string displayName = Localization.instance.Get(itemData.nameIds);
		
		Color color = FCConst.RareColorMapping[(EnumRareLevel)itemData.rareLevel];

		return string.Format("[{0}]{1}[-]", NGUITools.EncodeColor(color), displayName);
	}
}

[System.Serializable]
public class MatAmountMapping
{
	public string materialName;
	public int amount;

	public MatAmountMapping(string matName, int amount)
	{
		this.materialName = matName;

		this.amount = amount;
	}
}

[System.Serializable]
public class TattooSuiteData
{
	public string suiteID;
	public string nameIDS;
	public string descIDS;
	public int ord;
	public int level;
	public int rareLevel;
	public List<string> tdList;		//tattoos included
	public AIHitParams attribute0;
	public float value0;
	public AIHitParams attribute1;
	public float value1;
	public AIHitParams attribute2;
	public float value2;

	public float GetHitValueByParams(AIHitParams paramsKey)
	{
		if (paramsKey == attribute0)
		{
			return value0;
		}
		else if (paramsKey == attribute1)
		{
			return value1;
		}
		else if (paramsKey == attribute2)
		{
			return value2;
		}
		else
		{
			return 0;
		}
	}

	public string GetDisplayName()
	{
		string displayName = Localization.instance.Get(this.nameIDS);

		Color color = FCConst.RareColorMapping[(EnumRareLevel)this.rareLevel];

		return string.Format("[{0}]{1}[-]", NGUITools.EncodeColor(color), displayName);
	}

}

[System.Serializable]
public class TattooExchangeData
{
	public string id; //could be tattoo id or recipeID
	public int costSC;
	public List<MatAmountMapping> materials;
}