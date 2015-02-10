using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BaseConfigData
{
	public string _id;
	public string _type;
}

[System.Serializable]
public class IntConfigData: BaseConfigData
{
	public int _value;
}

[System.Serializable]
public class BoolConfigData: BaseConfigData
{
	public bool _value;
}

[System.Serializable]
public class StringConfigData: BaseConfigData
{
	public string _value;
}

[System.Serializable]
public class FloatConfigData: BaseConfigData
{
	public float _value;
}

public class GlobalConfig: ScriptableObject
{
	public List<BoolConfigData> _boolDatas = new List<BoolConfigData>();
	public List<IntConfigData> _intDatas = new List<IntConfigData>();
	public List<StringConfigData> _stringDatas = new List<StringConfigData>();
	public List<FloatConfigData> _floatDatas = new List<FloatConfigData>();
	
	public Dictionary<string, BaseConfigData> _map = new Dictionary<string, BaseConfigData>();
	
	public object getConfig(string name)
	{
		BaseConfigData bd = _map[name];
		if(bd._type == "int")
		{
			return ((IntConfigData)bd)._value;
		}
		if(bd._type == "string")
		{
			return ((StringConfigData)bd)._value;
		}
		if(bd._type == "bool")
		{
			return ((BoolConfigData)bd)._value;
		}
		if(bd._type == "float")
		{
			return ((FloatConfigData)bd)._value;
		}
		return null;
	}
	
	public void initMap()
	{
		foreach(BoolConfigData data in _boolDatas)
		{
			_map[data._id] = data;
		}
		foreach(StringConfigData data in _stringDatas)
		{
			_map[data._id] = data;
		}
		foreach(IntConfigData data in _intDatas)
		{
			_map[data._id] = data;
		}
		foreach(FloatConfigData data in _floatDatas)
		{
			_map[data._id] = data;
		}
	}
	
	public void Reset()
	{
		_map.Clear();
		initMap();
	}
	
	public void SetValue(BaseConfigData data)
	{
		_map[data._id] = data;
	}
}
