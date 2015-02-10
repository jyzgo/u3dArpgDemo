using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TattooDataList : ScriptableObject
{
	public List<TattooData> dataList = new List<TattooData>();

	public void Sort()
	{
		dataList.Sort(new TattooDataComparer());
	}

	public class TattooDataComparer : IComparer<TattooData>
	{
		public int Compare(TattooData x, TattooData y)
		{
			int xx = x.level * 1000 + x.ord;

			int yy = y.level * 1000 + y.ord;

			if (xx > yy)
			{
				return 1;
			}
			else if (xx == yy)
			{
				return 0;
			}
			else
			{
				return -1;
			}
		}
	}
}