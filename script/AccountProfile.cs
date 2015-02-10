using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InJoy.Utils;
using InJoy.FCComm;
using System;
using InJoy.RuntimeDataProtection;

[System.Serializable]
public class AccountProfile
{
    //public int _curRole = 0;

    public bool _haveRated = false;
    
    public PlayerInfo[] _playerProfiles = new PlayerInfo[FCConst.k_role_count];
	
	public AccountProfile()
    {
    }
	
	public static void FillDictionary(Dictionary<string, int> dictionary, ArrayList arrayList)
	{
		dictionary.Clear();
		if (arrayList != null)
		{
			foreach (object val in arrayList)
			{
				ArrayList keyValue = (ArrayList)val;
				string key = (string)keyValue[0];
				int lValue = (int)keyValue[1];
				dictionary.Add(key, lValue);
			}
		}
	}

}