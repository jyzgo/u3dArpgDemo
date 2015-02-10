using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIMessageBoxWithItems : UIMessageBox
{
	public GameObject commonItemPrefab;

	private List<UICommonDisplayItem> _itemList;

	public override void SetParams(string text, string caption, OnClickCallback callback, params System.Object[] args)
	{
		base.SetParams(text, caption, callback, args);

		List<ItemInventory> itemList = args[0] as List<ItemInventory>;
		
		//find the grid
		GameObject goGrid = this.transform.FindChild("Panel/ItemContainer/Grid").gameObject;

		_itemList = new List<UICommonDisplayItem>();

		foreach (ItemInventory ii in itemList)
		{
			GameObject go = NGUITools.AddChild(goGrid, commonItemPrefab);

			UICommonDisplayItem cdt = go.GetComponent<UICommonDisplayItem>();

			cdt.SetData(ii.ItemID, ii.Count);

			_itemList.Add(cdt);
		}
	}
}
