using UnityEngine;
using System.Collections.Generic;

public class UINGUIGrid : UIGrid
{	
	public GameObject InsertItem(int index, GameObject prefab)
	{
		Transform myTrans = transform;
		
		bool itemAdded = false;
		int x = 0;
		int y = 0;
		
		List<Transform> list = new List<Transform>();

		for (int i = 0; i < myTrans.childCount; ++i)
		{
			Transform t = myTrans.GetChild(i);
			if (t && (!hideInactive || NGUITools.GetActive(t.gameObject))) list.Add(t);
		}
		
		GameObject o = NGUITools.AddChild(gameObject, prefab);
		Transform item = o.transform;
		
		if (index == 0)
		{
			item.localPosition = (arrangement == Arrangement.Horizontal) ?
				new Vector3(cellWidth * x, -cellHeight * y, 0f) :
				new Vector3(cellWidth * y, -cellHeight * x, 0f);
				
			if (++x >= maxPerLine && maxPerLine > 0)
			{
				x = 0;
				++y;
			}
			
			itemAdded = true;
		}

		for (int i = 0; i < list.Count; ++i)
		{
			Transform t = list[i];

			if (!NGUITools.GetActive(t.gameObject) && hideInactive) continue;
			
			if ((index>0) && (i==index))
			{		
				item.localPosition = (arrangement == Arrangement.Horizontal) ?
					new Vector3(cellWidth * x, -cellHeight * y, 0f) :
					new Vector3(cellWidth * y, -cellHeight * x, 0f);
				
				if (++x >= maxPerLine && maxPerLine > 0)
				{
					x = 0;
					++y;
				}
				
				itemAdded = true;
			}
			
			float depth = t.localPosition.z;
			t.localPosition = (arrangement == Arrangement.Horizontal) ?
				new Vector3(cellWidth * x, -cellHeight * y, depth) :
				new Vector3(cellWidth * y, -cellHeight * x, depth);			
			
			if (++x >= maxPerLine && maxPerLine > 0)
			{
				x = 0;
				++y;
			}
		}
		
		if (!itemAdded)
		{
			item.localPosition = (arrangement == Arrangement.Horizontal) ?
					new Vector3(cellWidth * x, -cellHeight * y, 0f) :
					new Vector3(cellWidth * y, -cellHeight * x, 0f);
		}


		UIDraggablePanel drag = NGUITools.FindInParents<UIDraggablePanel>(gameObject);
		if (drag != null) drag.UpdateScrollbars(true);
		
		return o;
	}
}
