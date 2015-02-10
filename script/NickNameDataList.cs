using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class NickNameDataList : ScriptableObject
{
    public List<NickNameData> dataList = new List<NickNameData>();

    public NickNameData GetNickNameDataByIndex(int id)
    {
        NickNameData nickName = dataList.Find(delegate(NickNameData data) { return data.id == id; });

        return nickName;
    }

    public string GetNickName()
    {
        List<NickNameData> nickNameList = GetRandomNickDataListByCount(2);

        string nickname = string.Empty;

        if (LocalizationContainer.CurSystemLang == "zh-Hans")
        {
            while (nickNameList[0].prefixCh == string.Empty && nickNameList[1].suffixCh == string.Empty)
            {
                nickNameList = GetRandomNickDataListByCount(2);
            }

            nickname = string.Format("{0}{1}", nickNameList[0].prefixCh, nickNameList[1].suffixCh);
        }
        else if (LocalizationContainer.CurSystemLang == "en")
        {
            while (nickNameList[0].prefixEn == string.Empty && nickNameList[1].suffixEn == string.Empty)
            {
                nickNameList = GetRandomNickDataListByCount(2);
            }

            nickname = string.Format("{0}{1}", nickNameList[0].prefixEn, nickNameList[1].suffixEn);
        }

        return nickname;

    }

    public List<NickNameData> GetRandomNickDataListByCount(int randomCount)
    {
        List<NickNameData> nickNameList = new List<NickNameData>();

        for (int i = 0; i < randomCount; i++)
        {
            int index = UnityEngine.Random.Range(1, dataList.Count);

            NickNameData temp = GetNickNameDataByIndex(index);

            if (!nickNameList.Contains(temp))
            {
                nickNameList.Add(temp);
            }
            else
            {
                i--;
            }
        }

        return nickNameList;
    }

}
