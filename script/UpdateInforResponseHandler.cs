using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using FaustComm;

public class UpdateInforResponseHandler
{
    #region singleton
    private static UpdateInforResponseHandler _instance;
    public static UpdateInforResponseHandler Instance
    {
        get
        {
            if (null == _instance)
            {
                _instance = new UpdateInforResponseHandler();
            }
            return _instance;
        }
    }
    #endregion

    public UpdateInforResponseData UpdateOperation(BinaryReader reader)
    {
        List<PlayerProp> playerPropsList = new List<PlayerProp>();
        List<ItemInventory> newItemsList = new List<ItemInventory>();
        List<ItemMoveVo> itemMoveOps = new List<ItemMoveVo>();
        List<ItemCountVo> itemCountOps = new List<ItemCountVo>();
        List<ItemAttributeUpdateVoList> itemAttributeOps = new List<ItemAttributeUpdateVoList>();
		List<TattooEquipVo> tattooOpList = new List<TattooEquipVo>();
		List<ItemInventory> updateItemList = new List<ItemInventory>();

        //parse and save first,update later.
        int changeCount = reader.ReadInt16();
        for (int index = 0; index < changeCount; index++)
        {
            UpdateKey uk = (UpdateKey)reader.ReadByte();
            switch(uk)
            {
                case UpdateKey.RoleProp://section 1
                    int propmask = reader.ReadInt32();
                    for (int key = 0; key < 32; key++)
                    {
                        if ((propmask & (1 << key)) != 0)
                        { 
                            playerPropsList.Add(new PlayerProp((PlayerPropKey)key, reader.ReadInt32()));
                        }
                    }
                    break;
                case UpdateKey.RolePropBattle://section 2
                    int propmaskBattle = reader.ReadInt32();
                    for (int key = 0; key < 32; key++)
                    {
                        if ((propmaskBattle & (1 << key)) != 0)
                        { 
                            playerPropsList.Add(new PlayerProp((PlayerPropKey)key + 32, reader.ReadInt32()));
                        }
                    }
                    break;
                case UpdateKey.ItemMove:
                    itemMoveOps.Add(new ItemMoveVo(reader.ReadInt64(),
                        (ItemMoveType)reader.ReadByte()));
                    break;

                case UpdateKey.ItemCount:
					ItemCountVo iv = new ItemCountVo(reader.ReadInt64(),reader.ReadInt16());
					itemCountOps.Add(iv);
					
					if (iv.ItemCount > 0)
					{
						ItemInventory ii = PlayerInfo.Instance.PlayerInventory.GetItem(iv.ItemGuid);
						updateItemList.Add(ii);
					}
                    break;

                case UpdateKey.ItemNew:
                    ItemInventory itemInventory = new ItemInventory();
                    itemInventory.Parse(reader);
                    itemInventory.IsNew = true;
                    newItemsList.Add(itemInventory);

					updateItemList.Add(itemInventory);
                    break;

                case UpdateKey.ItemPropUpdate:
                    Int64 itemGUID = reader.ReadInt64();
                    List<ItemAttributeUpdateVo> list = new List<ItemAttributeUpdateVo>();
                    byte changeAttributesCount = reader.ReadByte();
                    for (int i = 0; i < changeAttributesCount; i++)
                    {
                        list.Add(new ItemAttributeUpdateVo((FC_EQUIP_EXTEND_ATTRIBUTE)reader.ReadInt32(), reader.ReadInt32()));
                    }
                    itemAttributeOps.Add(new ItemAttributeUpdateVoList(itemGUID, list.ToArray()));
                    break;
				
				case UpdateKey.Tattoo:
					EnumTattooPart part = (EnumTattooPart)reader.ReadByte();

					long itemGUID2 = reader.ReadInt64();

					byte op = reader.ReadByte();

					tattooOpList.Add(new TattooEquipVo(itemGUID2, part, op));

					break;
            }
        }

        UpdateInforResponseData data = new UpdateInforResponseData();
        data.itemCountOps = itemCountOps;
        data.itemMoveOps = itemMoveOps;
        data.newItemsList = newItemsList;
        data.playerPropsList = playerPropsList;
        data.itemAttributeOps = itemAttributeOps;
		data.tattooOpList = tattooOpList;
		data.itemUpdateList = updateItemList;
        return data;
    }
}
