using System;
using System.Collections.Generic;

public class UpdateInforResponseData
{
    public delegate void ItemMoveDelegate(ItemMoveVo[] guids);
    public static ItemMoveDelegate EquipMoveObserver;

    public delegate void ItemUpdateDelegate(ItemAttributeUpdateVoList[] updateItems);
    public static ItemUpdateDelegate ItemUpdateObserver;

    public delegate void ItemsCountUpdateDelegate(List<ItemCountVo> itemsCountVos);
    public static ItemsCountUpdateDelegate ItemsCountUpdateObserver;

    public delegate void ItemNewAddedDelegate(ItemInventory[] newItems);
    public static ItemNewAddedDelegate ItemNewAddObserver;

    public delegate void PlayerPropUpdateDelegate(PlayerProp[] props);
    public static PlayerPropUpdateDelegate PlayerPropUpdateObserver;

	public delegate void PlayerTattoosUpdateDelegate(List<TattooEquipVo> ops);
	public static PlayerTattoosUpdateDelegate PlayerTattoosUpdateObserver;

	public delegate void PlayerQuestUpdateDelegate(List<int> ops);
	public static PlayerQuestUpdateDelegate PlayerQuestUpdateObserver;
	
	public List<PlayerProp> playerPropsList;
    public List<ItemInventory> newItemsList;
    public List<ItemMoveVo> itemMoveOps;
    public List<ItemCountVo> itemCountOps;
    public List<ItemAttributeUpdateVoList> itemAttributeOps;
	public List<TattooEquipVo> tattooOpList;
	public List<ItemInventory> itemUpdateList;  //all items including new, updated, amount changed items

    public void Broadcast()
    {
        //itemCountOps
        if (itemCountOps.Count > 0 && null != ItemsCountUpdateObserver)
        {
            ItemsCountUpdateObserver(itemCountOps);
        }

        //itemAttributeOps
        if (itemAttributeOps.Count > 0 && null != ItemUpdateObserver)
        {
            ItemUpdateObserver(itemAttributeOps.ToArray());
        }

        //itemMoveOps
        if (itemMoveOps.Count > 0 && null != EquipMoveObserver)
        {
            EquipMoveObserver(itemMoveOps.ToArray());
        }

        //newItemsList
        if (newItemsList.Count > 0 && null != ItemNewAddObserver)
        {
            ItemNewAddObserver(newItemsList.ToArray());
        }

        //playerPropsList
        if (playerPropsList.Count > 0 && null != PlayerPropUpdateObserver)
        {
            PlayerPropUpdateObserver(playerPropsList.ToArray());
        }

		//player tattoos update
		if (tattooOpList.Count > 0 && null != PlayerTattoosUpdateObserver)
		{
			PlayerTattoosUpdateObserver(tattooOpList);
		}
    }
}

public struct PlayerProp
{
    public PlayerPropKey Key;
    public int Value;
    public PlayerProp(PlayerPropKey key, int value)
    {
        this.Key = key;
        this.Value = value;
    }
}

public struct ItemMoveVo
{
    public ItemMoveVo(Int64 guid, ItemMoveType moveType)
    {
        this.ItemGUID = guid;
        this.MoveType = moveType;
    }
    public Int64 ItemGUID;
    public ItemMoveType MoveType;
}

public struct ItemCountVo
{
    public ItemCountVo(Int64 guid, int count)
    {
        this.ItemGuid = guid;
        this.ItemCount = count;
    }
    public Int64 ItemGuid;
    public int ItemCount;
}

public struct TattooEquipVo
{
	public TattooEquipVo(Int64 guid, EnumTattooPart part, byte op)
    {
        this.ItemGuid = guid;
        this.part = part;
		this.op = op;
    }
    public Int64 ItemGuid;
    public EnumTattooPart part;
	public byte op;
}

public struct ItemAttributeUpdateVoList
{
    public ItemAttributeUpdateVoList(Int64 itemGuid, ItemAttributeUpdateVo[] VoList)
    {
        this.ItemGuid = itemGuid;
        this.VoList = VoList;
    }
    public Int64 ItemGuid;
    public ItemAttributeUpdateVo[] VoList;
}

public struct ItemAttributeUpdateVo
{
    public FC_EQUIP_EXTEND_ATTRIBUTE Key;
    public int Value;
    public ItemAttributeUpdateVo(FC_EQUIP_EXTEND_ATTRIBUTE key, int value)
    {
        this.Key = key;
        this.Value = value;
    }
}