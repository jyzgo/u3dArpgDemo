using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[Serializable]
public class SocketNeed{
	public int _sc;
	public int _hc;
	public string _itemId;
}


[Serializable]
public class SocketData
{
	public int _level;
	public int _rareLevel;
	
	public List<SocketNeed> _need = new List<SocketNeed>();
}

public class SocketDataList : ScriptableObject {
	public List<SocketData> _dataList = new List<SocketData>();	
}
