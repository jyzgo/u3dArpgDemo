using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DictionarySave
{
	protected Dictionary<string, int>  _data;
	public List<string> _key_value = new List<string>();
	
//#if UNITY_EDITOR
	public DictionarySave()
	{
	}
	
	public void SetTarget(Dictionary<string, int> data)
	{
		_data = data;
		UpdateInfo();
	}
	
	private void UpdateInfo()
	{		
		_key_value.Clear();
		foreach(string key in _data.Keys)
		{
			string keyValue = "["+key+"]=" + _data[key];
			_key_value.Add(keyValue);
		}
	}
//#endif
}

