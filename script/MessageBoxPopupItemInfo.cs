using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MessageBoxPopupItemInfo : MonoBehaviour
{
	public UILabel labelName;

	public UITexture icon;

	public UISprite iconBg;

	public UILabel description;

	public void DisplayItemInfo(ItemData itemData)
	{
		labelName.text = itemData.DisplayNameWithRareColor;

		icon.mainTexture = InJoy.AssetBundles.AssetBundles.Load(itemData.iconPath) as Texture2D;

		iconBg.spriteName = UIGlobalSettings.QualityNamesMap[(ItemQuality)itemData.rareLevel];

		if (itemData.type == ItemType.armor || itemData.type == ItemType.weapon || itemData.type == ItemType.ornament)
		{
			description.text = DataManager.Instance.GetItemAttributesDisplay(itemData);
		}
		else
		{
			description.text = Localization.instance.Get(itemData.descriptionIds);
		}
	}
}
