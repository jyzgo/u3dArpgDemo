using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using InJoy.Utils;
using InJoy.FCComm;
using System.Reflection;
using System.Collections.Generic;

using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public static class Utils
{
    private static string _md5Key = ")Jfy3=-~;37!`%,d=uo}+ym*y$x9HU[]UDrw+)bT&#S4rfs~R|";

    private static byte[] _keyBytes = ASCIIEncoding.ASCII.GetBytes("injoy1st");

    public static void ChangeColliderZPos(GameObject go, bool active, float defaultZ, float activeZ)
    {
        BoxCollider collider = go.GetComponent<BoxCollider>();
        if (active)
        {
            collider.center = new Vector3(0, 0, activeZ);
        }
        else
        {
            collider.center = new Vector3(0, 0, defaultZ);
        }
    }

    public static string decodeUnicode(string text)
    {
        System.Text.StringBuilder output = new System.Text.StringBuilder();
        for (int i = 0; i < text.Length; ++i)
        {
            if (text[i] == '\\' && text[i + 1] == 'u')
            {
                output.Append(char.ConvertFromUtf32(System.Convert.ToInt32(text.Substring(i + 2, 4), 16)));
                i += 5;
            }
            else
            {
                output.Append(text[i]);
            }
        }
        return output.ToString();
    }


    public static GameObject GetNode(string nodeName, GameObject rootNode)
    {
        if (rootNode == null || nodeName == null || nodeName.Length == 0)
        {
            return null;
        }

        Transform[] allChildren = rootNode.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildren)
        {
            if (child.gameObject != rootNode)
            {
                if (child.gameObject.name == nodeName)
                {
                    return child.gameObject;
                }
            }
        }
        return rootNode;
    }

    public static bool IsContainIllegalSymbols(string text)
    {
        int length = text.Length;
        for (int i = 0; i < length; i++)
        {
            char c = text[i];
            if (Char.IsPunctuation(c) || Char.IsSeparator(c) || Char.IsSymbol(c)
                || Char.IsWhiteSpace(c) || Char.IsControl(c))
            {
                return true;
            }
        }
        return false;
    }

    public static bool FilterWords(string text)
    {
        //must be lower
        text = text.ToLower();
        List<List<string>> allStringList = new List<List<string>>();
        allStringList.Add(new List<string>());
        allStringList.Add(new List<string>());

        int startIndex = 0;
        int typeIndex = -1;
        int length = text.Length;
        for (int i = 0; i < length; i++)
        {
            char c = text[i];
            if (i == length - 1 && typeIndex != -1)
            {
                allStringList[typeIndex].Add(text.Substring(startIndex, length - startIndex));
            }
            else if (Char.IsPunctuation(c) || Char.IsSeparator(c) || Char.IsSymbol(c)
                || Char.IsWhiteSpace(c) || Char.IsControl(c) || Char.IsNumber(c))
            {
                continue;
            }
            else if (Char.IsLower(c)) //is Latin
            {
                if (typeIndex == 1)
                {
                    allStringList[typeIndex].Add(text.Substring(startIndex, i - startIndex));
                    startIndex = i;
                }
                typeIndex = 0;
            }
            else if (Char.IsLetter(c))// is Chinese and other
            {
                if (typeIndex == 0)
                {
                    allStringList[typeIndex].Add(text.Substring(startIndex, i - startIndex));
                    startIndex = i;
                }
                typeIndex = 1;
            }

            //Debug.Log(" GetUnicodeCategory : " + Char.GetUnicodeCategory(c).ToString());
        }
        //print all
        foreach (List<string> subList in allStringList)
        {
            foreach (string str in subList)
            {
                //Debug.Log(str);
            }
        }
        //filter bad words
        List<string> wordsList = allStringList[0];
        foreach (string subText in wordsList)
        {
            foreach (string badWordStr in GameSettings.Instance.BadWordsRules)
            {
                int start = 0;
                int i = subText.IndexOf(badWordStr, start);
                while (i != -1)
                {
                    start = i + badWordStr.Length;
                    if (!(i > 0 && Char.IsLower(subText[i - 1])) && !(start < subText.Length && Char.IsLower(subText[start])))
                    {
                        Debug.Log("Bad words : " + badWordStr + " -- " + subText);
                        return false;
                    }
                    i = subText.IndexOf(badWordStr, start);
                }
            }
        }

        List<string> charatersList = allStringList[1];
        foreach (string subText in charatersList)
        {
            foreach (string badWordStr in GameSettings.Instance.BadCharactersRules)
            {
                if (subText.Contains(badWordStr))
                {
                    Debug.Log("Bad words : " + badWordStr + " -- " + subText);
                    return false;
                }
            }
        }
        return true;
    }

    public static bool IsNumberOrEnglish(char ch)
    {
        return (ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z');
    }

    public static bool IsAllNumberOrEnglish(string str)
    {
        int length = str.Length;
        for (int i = 0; i < length; i++)
        {
            if (!IsNumberOrEnglish(str[i]))
            {
                return false;
            }
        }
        return true;
    }

    public static int CountOfNotNumberOrEnglish(string str)
    {
        int sum = 0;
        for (int i = 0,length = str.Length; i < length; i++)
        {
            if (!IsNumberOrEnglish(str[i]))
            {
                ++sum;
            }
        }
        return sum;
    }

    public static bool IsMail(string email)
    {
        return Regex.IsMatch(email, @"^\s*([A-Za-z0-9_-]+(\.\w+)*@([\w-]+\.)+\w{2,10})\s*$");
    }

    public static string Md5Encryt(string str)
    {
        byte[] buffer = Encoding.Default.GetBytes(str);
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] data = md5.ComputeHash(buffer);
        md5.Clear();
        StringBuilder sb = new StringBuilder();
        foreach (byte b in data)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }

    public static string DesEncrypt(string str)
    {
        DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
        MemoryStream memoryStream = new MemoryStream();
        CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoProvider.CreateEncryptor(_keyBytes, _keyBytes), CryptoStreamMode.Write);
        StreamWriter writer = new StreamWriter(cryptoStream);
        writer.Write(str);
        writer.Flush();
        cryptoStream.FlushFinalBlock();
        writer.Flush();
        return System.Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
    }

    public static string DesDecrypt(string str)
    {
        DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
        MemoryStream memoryStream = new MemoryStream(System.Convert.FromBase64String(str));
        CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoProvider.CreateDecryptor(_keyBytes, _keyBytes), CryptoStreamMode.Read);
        StreamReader reader = new StreamReader(cryptoStream);
        return reader.ReadToEnd();
    }

    public static bool HasFlag(int iValue, int source)
    {
        return (source & (1 << (iValue))) != 0;
    }

    public static void SetFlag(int iValue, ref int source)
    {
        source |= (1 << (iValue));
    }

    public static void ClearFlag(int iValue, ref int source)
    {
        source &= (~(1 << (iValue)));
    }

    /// <summary>
    /// Find a transfrom in the hierachy of the root transform, the name is not necessarily a path name.
    /// </summary>
    /// <param name="root"></param>
    /// <param name="nodeName"></param>
    /// <returns></returns>
    public static Transform FindTransformByNodeName(Transform root, string nodeName)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>();

        foreach (Transform t in children)
        {
            if (t.name == nodeName)
            {
                return t;
            }
        }
        return null;
    }

    /// <summary>
    /// Instantiate a new GameObject and set its parent.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static Transform NewGameObjectWithParent(string name)
    {
        return NewGameObjectWithParent(name, null);
    }

    public static Transform NewGameObjectWithParent(string name, Transform parent)
    {
        GameObject go = new GameObject(name);

        SetParent(go, parent);

        return go.transform;
    }

    /// <summary>
    /// Set the parent of a given GameObject, with its local parameters to default.
    /// </summary>
    /// <param name="go"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static void SetParent(GameObject go, Transform parent)
    {
        Transform t = go.transform;

        t.parent = parent;

        t.localPosition = Vector3.zero;

        t.localRotation = Quaternion.identity;

        t.localScale = Vector3.one;
    }

    /// <summary>
    /// Set the parent of a given Transform, with its local parameters to default.
    /// </summary>
    /// <param name="child"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static void SetParent(Transform child, Transform parent)
    {
        Transform t = child;

        t.parent = parent;

        t.localPosition = Vector3.zero;

        t.localRotation = Quaternion.identity;

        t.localScale = Vector3.one;
    }

    /// <summary>
    /// Instantiate the prefab and set its parent. The parent could be null.
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static Transform InstantiateGameObjectWithParent(GameObject prefab, Transform parent)
    {
        GameObject go = GameObject.Instantiate(prefab) as GameObject;

        SetParent(go, parent);

        return go.transform;
    }


    public static Material CloneMaterial(Material mat)
    {
        Material newMat = new Material(mat.shader);
        newMat.name = mat.name + "(Clone)";
        newMat.CopyPropertiesFromMaterial(mat);
        return newMat;
    }

    public static string GetSplitNumberString(int number)
    {
        if (number < 1000)
        {
            return number.ToString();
        }

        string strNum = "";
        while (number / 1000 > 0)
        {
            int temp = number % 1000;
            string tempStr = "";
            if (temp < 10)
            {
                tempStr += "00";
            }
            else if (temp < 100)
            {
                tempStr += "0";
            }
            tempStr += temp.ToString();
            strNum = "," + tempStr + strNum;
            number = number / 1000;
        }
        if (number > 0)
        {
            strNum = number.ToString() + strNum;
        }
        return strNum;
    }


    public static string GetSplitNumberStringWithK(int number)
    {
        if (number < 1000)
        {
            return GetSplitNumberString(number);
        }
        else
        {

            int kNumber = number / 1000;
            return GetSplitNumberString(kNumber) + "k";
        }
    }


    /// <summary>
    /// this function use for Serializable Class, don't support MonoBehaviour
    /// </summary>
    /// <param name="obj"> clone target</param>
    public static object Clone(object obj)
    {
        object target = null;
        Type type = obj.GetType();
        if (type.GetConstructor(Type.EmptyTypes) != null)
        {
            target = Activator.CreateInstance(type);
        }
        //except nonpublics
        foreach (FieldInfo info in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (info.FieldType.IsPrimitive || info.FieldType.IsValueType
                || info.FieldType.IsEnum || info.FieldType.Equals(typeof(System.String)))
            {
                info.SetValue(target, info.GetValue(obj));
            }
            else if (typeof(IList).IsAssignableFrom(info.FieldType))
            {
                IList subTargetList = info.GetValue(target) as IList;
                if (subTargetList == null)
                {
                    // for arrayList type, there is no generic arguments.
                    if (info.FieldType.GetGenericArguments().Length == 0)
                    {
                        continue;
                    }
                    Type subType = info.FieldType.GetGenericArguments()[0];
                    Type customList = typeof(List<>).MakeGenericType(subType);
                    subTargetList = (IList)Activator.CreateInstance(customList);
                    info.SetValue(target, subTargetList);
                }
                IList subObjList = info.GetValue(obj) as IList;
                foreach (var subObj in subObjList)
                {
                    if (subObj == null)
                    {
                        // Is this crazy?
                        subTargetList.Add(null);
                    }
                    else
                    {
                        subTargetList.Add(Clone(subObj));
                    }
                }
            }
            else
            {
                object subObj = info.GetValue(obj);
                if (subObj == null)
                {
                    info.SetValue(target, null);
                }
                else
                {
                    info.SetValue(target, Clone(subObj));
                }
            }
        }
        return target;
    }

    public delegate void OnUpdateList(IList subObjList, Hashtable table, string key, Type subType);

    public static int GetRoleByName(string roleName)
    {
        int index = 0;
        foreach (string s in FCConst.k_role_names)
        {
            if (s.ToLower() == roleName.ToLower())
            {
                return index;
            }
            index++;
        }
        return -1;
    }

    public static void CustomGameServerMessage(FaustComm.NetRequest request, FaustComm.ServerCallbackDelegate callback)
    {
        return;
    }


    public static void AddUIButtonMessage(GameObject button, GameObject target, string functionName)
    {
        if (null != button)
        {
            if (null == button.GetComponent<UIButtonMessage>())
            {
                button.AddComponent<UIButtonMessage>();
            }

            button.GetComponent<UIButtonMessage>().target = target;
            button.GetComponent<UIButtonMessage>().functionName = functionName;
        }
    }

    /// <summary>
    ///         easy to destroy parent's children(depth 1)
    /// </summary>
    /// <param name="parent"></param>
    public static void ClearChildrenGameobjects(Transform parent)
    {
        List<GameObject> willDeleteChildren = new List<GameObject>();

        foreach (Transform child in parent)
        {
            willDeleteChildren.Add(child.gameObject);
        }
        foreach (GameObject go in willDeleteChildren)
        {
            go.transform.parent = null;
            UnityEngine.Object.Destroy(go);
        }
        willDeleteChildren.Clear();
    }

    public static int CompareNickNameFromMinToMax(NickNameData data1, NickNameData data2)
    {
        if (null == data1)
        {
            if (null == data2)
            {
                return 0;
            }

            return 1;
        }

        if (null == data2)
        {
            return -1;
        }

        return data1.id.CompareTo(data2.id);
    }

	public static List<LootObjData> ParseLootList(BinaryReader reader)
	{
		int count = reader.ReadByte();

		List<LootObjData> list = new List<LootObjData>();

		for (int i = 0; i < count; i++)
		{
			LootObjData data = new LootObjData();

			data._lootId = FaustComm.NetResponse.ReadString(reader);

			data._lootCount = reader.ReadInt32();

			list.Add(data);
		}
		return list;
	}

	public static string GetErrorIDS(int errorCode)
	{

		if (DataManager.Instance.errorDataList.ErrorDefineMapping.ContainsKey(errorCode))
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return DataManager.Instance.errorDataList.ErrorDefineMapping[errorCode].ToLocalization();
#else
			string text;
			if (errorCode < 1000) //network error, protocol error, etc, always shows "network error"
			{
				if (errorCode == (int)FaustComm.ErrorType.session_timeout || errorCode == (int)FaustComm.ErrorType.session_error)
				{
					text = Localization.instance.Get("IDS_MESSAGE_GLOBAL_FATALERROR");
				}
				else
				{
					text = Localization.instance.Get("IDS_MESSAGE_GLOBAL_NETWORKERROR");
				}
			}
			else //logic error
			{
				text = DataManager.Instance.errorDataList.ErrorDefineMapping[errorCode].ToLocalization();
			}

			return text;
#endif
        }
		else
		{
			return string.Format(Localization.instance.Get("IDS_MESSAGE_GLOBAL_ERROR_UNKNOW"), errorCode);
		}
	}

	public static string GetItemPartNamesIDS(ItemType itemType, ItemSubType subType)
	{
        string result = "unkonw";
        if(itemType == ItemType.armor || itemType == ItemType.ornament)
        {
            switch(subType)
            {
                case ItemSubType.armpiece:
                    result = "IDS_NAME_PART_ARMPIECE";
                    break;
                case ItemSubType.belt:
                    result = "IDS_NAME_PART_BELT";
                    break;
                case ItemSubType.chest:
                    result = "IDS_NAME_PART_CHEST";
                    break;
                case ItemSubType.helmet:
                    result = "IDS_NAME_PART_HELMET";
                    break;
                case ItemSubType.leggings:
                    result = "IDS_NAME_PART_LEGGINGS";
                    break;
                case ItemSubType.necklace:
                    result = "IDS_NAME_PART_NECKLACE";
                    break;
                case ItemSubType.ring:
                    result = "IDS_NAME_PART_RING";
                    break;
                case ItemSubType.shoulder:
                    result = "IDS_NAME_PART_SHOULDER";
                    break;
                case ItemSubType.vanity:
                    result = "";
                    break;
            }
        }
        else if(itemType == ItemType.weapon)
        {
            result = "IDS_NAME_PART_WEAPON";
        }
        else if(itemType == ItemType.tattoo)
        {
            result = "IDS_NAME_PART_TATTOO";
        }
        else if(itemType == ItemType.tribute)
        {
            result = "IDS_NAME_PART_TRIBUTE";
        }
        else if(itemType == ItemType.material)
        {
            result = "IDS_NAME_PART_MATERIAL";
        }
        else if(itemType == ItemType.recipe)
        {
            result = "IDS_NAME_PART_RECIPE";
        }
        else if(itemType == ItemType.potion)
        {
            result = "IDS_NAME_PART_POTION";
        }
        return result;
	}

	//in same order of EnumTattooPart
	public static string[] k_tattoo_part_names = new string[]
	{
		"IDS_TATTOO_POSHEAD",
		"IDS_TATTOO_POSSHOULDERLEFT",
		"IDS_TATTOO_POSSHOULDERRIGHT",
		"IDS_TATTOO_POSUPPERARMLEFT",
		"IDS_TATTOO_POSUPPERARMRIGHT",
		"IDS_TATTOO_POSLOWERARMLEFT",
		"IDS_TATTOO_POSLOWERARMRIGHT",
		"IDS_TATTOO_POSCHEST",
		"IDS_TATTOO_POSWAIST",
		"IDS_TATTOO_POSTHIGHLEFT",
		"IDS_TATTOO_POSTHIGHRIGHT",
		"IDS_TATTOO_POSCALFLEFT",
		"IDS_TATTOO_POSCALFRIGHT",
		"IDS_TATTOO_POSBACK",
	};

	//returns the IDS of tattoo part names with "," as delimeters
	public static string GetTattooApplicablePositions(List<EnumTattooPart> partList)
	{
		string s = string.Empty;
		
		List<string> list = new List<string>();

		foreach (EnumTattooPart part in partList)
		{
			string partIDS = Utils.k_tattoo_part_names[(int)part];
			if (!list.Contains(partIDS))
			{
				string ids = Localization.instance.Get(partIDS);
				if (s == string.Empty)
				{
					s += ids;
				}
				else
				{
					s += "," + ids;
				}
				list.Add(partIDS);
			}
		}
		return s;
	}

	public static string GetRaritySpriteName(int rareLevel)
	{
		return (71 + rareLevel).ToString();
	}
	
	//quest related
	public static void RememberViewedQuest(int questID)
	{
		string viewedQuests = PlayerPrefs.GetString(PrefsKey.ViewedQuests, string.Empty);

		PlayerPrefs.SetString(PrefsKey.ViewedQuests, viewedQuests + questID.ToString() + ";");

		PlayerPrefs.Save();
	}

	public static bool IsViewedQuest(int questID)
	{
		string viewedQuests = PlayerPrefs.GetString(PrefsKey.ViewedQuests, string.Empty);

		//Debug.Log("Quests are: " + viewedQuests);

		return viewedQuests.Contains(questID.ToString() + ";");
	}

	public static void DestroySingleton<T>() where T: UnityEngine.Component
	{
		T instance = GameObject.FindObjectOfType<T>();

		if (instance != null)
		{
			UnityEngine.Object.Destroy(instance.gameObject);
		}
	}
}