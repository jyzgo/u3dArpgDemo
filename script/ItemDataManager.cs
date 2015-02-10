using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemDataManager
{

	private Dictionary<string, ItemData> _dataBase = new Dictionary<string, ItemData>();
	public Dictionary<string, ItemData> DataBase
	{
		get { return _dataBase; }
	}

	public void SetData(ItemDataList[] itemDataList)
	{
		foreach (ItemDataList list in itemDataList)
		{
			foreach (ItemData itemData in list._dataList)
			{
				try
				{
					_dataBase.Add(itemData.id, itemData);
				}
				catch
				{
					Debug.LogError("Duplicated item found in database: " + itemData.id);
				}
			}
		}
	}

	public ItemData GetItemData(string itemId)
	{
		ItemData val = null;
		if (_dataBase.TryGetValue(itemId, out val))
		{
			return val;
		}
		else
		{
			Debug.LogError(string.Format("[ItemDataManager] Item id not found:\"{0}\"", itemId));
		}
		return null;
	}

	public void SetData(string itemId, ItemData itemData)
	{
		if (_dataBase.ContainsKey(itemId))
		{
			_dataBase[itemId] = itemData;
		}
		else
		{
			_dataBase.Add(itemId, itemData);
		}
	}

}
