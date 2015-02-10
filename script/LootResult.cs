using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class LootResult
{
	public Dictionary<ItemData, int>[] _loots = new Dictionary<ItemData, int>[5];
	
	public void toXml(XmlDocument doc, XmlElement node)
	{
		XmlElement e = doc.CreateElement("loot");
		node.AppendChild(e);
		for(int i = 0; i < _loots.Length; ++i)
		{
			XmlElement e1 = doc.CreateElement("loot" + i);
			e.AppendChild(e1);
			if(_loots[i] != null)
			{
				foreach(ItemData data in _loots[i].Keys)
				{
					XmlElement el = doc.CreateElement("item");
					e1.AppendChild(el);
					el.SetAttribute("value", data.id + ":" + _loots[i][data]);
				}
			}
		}
	}
}
