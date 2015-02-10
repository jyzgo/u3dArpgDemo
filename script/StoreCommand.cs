using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

using FaustComm;

[NetCmdID(5030, name = "get store list")]
public class StoreGetListRequest : NetRequest
{
    public override void Encode(BinaryWriter writer)
    {
    }
}

[NetCmdID(5031, name = "buy store goods")]
public class StoreBuyRequest : NetRequest
{
    public int GoodsId;

    public override void Encode(BinaryWriter writer)
    {
        writer.Write(GoodsId);
    }
}

[NetCmdID(5032, name = "exchange sc")]
public class StoreSCExchangeRequest : NetRequest
{
    public int Type;

    public override void Encode(BinaryWriter writer)
    {
        writer.Write((byte)Type);
    }
}

[NetCmdID(5033, name = "store query price")]
public class StoreHCPriceQueryRequest : NetRequest
{
    public List<InventoryHCWorth> WantBuyItems;

    public override void Encode(BinaryWriter writer)
    {
        byte count = (byte)WantBuyItems.Count;
        writer.Write(count);
        for (int i = 0; i < count; i++)
        { 
            WriteString(writer, WantBuyItems[i].ItemId);
        }
    }
}

[NetCmdID(5034, name = "inventory buy price")]
public class InventoryItemsListBuyRequest : NetRequest
{
    public List<InventoryHCWorth> Items;
    public override void Encode(BinaryWriter writer)
    {
        byte count = (byte)Items.Count;
        writer.Write(count);
        foreach (InventoryHCWorth inHCBuy in Items)
        {
            WriteString(writer, inHCBuy.ItemId);
            writer.Write(inHCBuy.Count);
        }
    }
}

[NetCmdID(10030, name = "get store list")]
public class StoreListResponse : NetResponse
{
    public List<FC_StoreData> GoodsOnSellDataList;

    public List<StoreSCExchangeData> ExchangeSCDataList;

    public override void Decode(BinaryReader reader)
    {
        errorCode = reader.ReadInt16();
        if (Succeeded)
        {
            GoodsOnSellDataList = new List<FC_StoreData>();
            int goodsNum = reader.ReadInt16();
            for (int i = 0; i < goodsNum; i++)
            {
                int id = reader.ReadInt32();
                FC_StoreData storeData = DataManager.Instance.storeDataList.GetStoreById(id);
                storeData.price = reader.ReadInt32();
                storeData.status = reader.ReadByte();
                if (storeData.IsLimitDisappear)
                {
                    storeData.limitDisappear = reader.ReadInt32();
                }
                if (storeData.IsDiscount)
                {
                    storeData.discount = reader.ReadByte();
                    storeData.discountPrice = reader.ReadInt32();
                    storeData.discountExpire = reader.ReadInt32();
                }
                GoodsOnSellDataList.Add(storeData);
            }

            ExchangeSCDataList = new List<StoreSCExchangeData>();
            int exchangeNum = reader.ReadInt16();
            for (int i = 0; i < exchangeNum; i++)
            {
                StoreSCExchangeData exData = new StoreSCExchangeData();
                exData.Type = reader.ReadByte();
                exData.Count = reader.ReadByte();
                exData.CountMax = reader.ReadByte();
                exData.SC = reader.ReadInt32();
                exData.CoseHC = reader.ReadInt32();
                exData.ExpireTime = reader.ReadInt32();
                ExchangeSCDataList.Add(exData);
            }
        }
    }
}

[NetCmdID(10031, name = "get store buy")]
public class StoreBuyResponse : NetResponse
{
    public UpdateInforResponseData UpdateData;

    public override void Decode(BinaryReader reader)
    {
        errorCode = reader.ReadInt16();
        if (Succeeded)
        {
            UpdateData = UpdateInforResponseHandler.Instance.UpdateOperation(reader);
        }
    }
}

[NetCmdID(10032, name = "exchange sc handler")]
public class StoreSCExchangeResponse : NetResponse
{
    public StoreSCExchangeData UpdateExchangeData;

    public UpdateInforResponseData UpdateData;

    public override void Decode(BinaryReader reader)
    {
        errorCode = reader.ReadInt16();
        if (Succeeded)
        {
            UpdateExchangeData = new StoreSCExchangeData();
            UpdateExchangeData.Type = reader.ReadByte();
            UpdateExchangeData.Count = reader.ReadByte();
            UpdateExchangeData.CountMax = reader.ReadByte();
            UpdateExchangeData.SC = reader.ReadInt32();
            UpdateExchangeData.CoseHC = reader.ReadInt32();
            UpdateExchangeData.ExpireTime = reader.ReadInt32();
            UpdateData = UpdateInforResponseHandler.Instance.UpdateOperation(reader);
        }
    }
}

[NetCmdID(10033, name = "inventory hc price")]
public class StoreHCPriceQueryResponse : NetResponse
{ 
    public Dictionary<string, InventoryHCWorth> InventoryHCWorthMapping;

    public override void Decode(BinaryReader reader)
    {
        errorCode = reader.ReadInt16();
        InventoryHCWorthMapping = new Dictionary<string, InventoryHCWorth>();
        int count = reader.ReadByte();
        for(int i = 0; i < count; i++)
        {
            InventoryHCWorth inHCWorth = new InventoryHCWorth();
            inHCWorth.ItemId = ReadString(reader);
            inHCWorth.OnePrice = reader.ReadInt32();
            inHCWorth.Discount = reader.ReadByte();
            inHCWorth.DiscountPrice = reader.ReadInt32();
            InventoryHCWorthMapping[inHCWorth.ItemId] = inHCWorth; 
        }
    }
}

[NetCmdID(10034, name = "store buy items")]
public class StoreInventoryItemsListBuyResponse : NetResponse
{
    public UpdateInforResponseData UpdateData;

    public override void Decode(BinaryReader reader)
    {
        errorCode = reader.ReadInt16();
        if (Succeeded)
        {
            UpdateData = UpdateInforResponseHandler.Instance.UpdateOperation(reader);
        }
    }
}

public class InventoryHCWorth
{
    public string ItemId;

    public int Count;

    public int OnePrice;
    
    public byte Discount;

    public int DiscountPrice;
}
