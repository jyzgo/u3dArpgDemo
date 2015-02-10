using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FC_StoreData
{
    public int id;

    public string name;

    public int count;

    public string displayNameIds;

    public int order;

    public string storeIconName;

    public int tabIndex;

    public int price;

    public byte status;

    public int limitDisappear;

    public byte discount;

    public int discountPrice;

    public int discountExpire;


    public string DisplayDiscountExpireTime
    {
        get
        {
            double nowTS = TimeUtils.ConvertToUnixTimestamp(TimeUtils.GetPSTDateTime());
            string timeSpan = TimeUtils.GetLocalizedTimeString(new TimeSpan(0, 0, (int)(discountExpire - nowTS)));
            return timeSpan;
        }
    }

    public string DisplayDisappearTime
    {
        get
        {
            double nowTS = TimeUtils.ConvertToUnixTimestamp(TimeUtils.GetPSTDateTime());
            string timeSpan = TimeUtils.GetLocalizedTimeString(new TimeSpan(0, 0, (int)(limitDisappear - nowTS)));
            return timeSpan;
        }
    }

    public bool IsNew
    {
        get
        {
            return (status & (1 << 0)) != 0;
        }
    }

    public bool IsRecommended
    {
        get
        {
            return (status & (1 << 1)) != 0;
        }
    }

    public bool IsSC
    {
        get
        {
            return (status & (1 << 2)) != 0;
        }
    }

    public bool IsDiscount
    {
        get
        {
            return (status & (1 << 3)) != 0;
        }
    }

    public bool IsLimitDisappear
    {
        get 
        {
            return (status & (1 << 4)) != 0;
        }
    }
}

public class FC_StoreDataList : ScriptableObject
{
    public List<FC_StoreData> dataList = new List<FC_StoreData>();

    public FC_StoreData GetStoreById(int id)
    {
        return dataList.Find(delegate(FC_StoreData sd) 
        {
            return sd.id == id;
        });
    }
}

[System.Serializable]
public class StoreSCExchangeData
{
    public int Type;

    public int Count;

    public int CountMax;

    public int RemainCount
    {
        get
        {
            return CountMax - Count;
        }
    }

    public int SC;

    public int CoseHC;

    public int ExpireTime;

    public DateTime ExpireDateTime
    {
        get
        {
            return TimeUtils.ConvertFromUnixTimestamp(ExpireTime);
        }
    }

    public string ExpireDateString
    {
        get
        {
            double nowTS = TimeUtils.ConvertToUnixTimestamp(TimeUtils.GetPSTDateTime());
            return TimeUtils.GetLocalizedTimeString(new TimeSpan(0, 0, (int)(ExpireTime - nowTS)));
        }
    }
}