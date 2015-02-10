using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MessageBoxPopupTattooInfo : MonoBehaviour
{
	public UITexture icon;

	public UILabel labelName;
	public UILabel labelBearPoint;
	public UILabel labelPos;
	public UILabel labelEffect;
	public UILabel labelSuiteEffect;

	public void DisplayTattooInfo(string tattooID)
	{
		ItemData itemData = DataManager.Instance.GetItemData(tattooID);

		TattooData td = DataManager.Instance.GetTattooData(itemData.id);

		icon.mainTexture = InJoy.AssetBundles.AssetBundles.Load(itemData.iconPath) as Texture2D;

		labelName.text = td.GetDisplayName();

		labelBearPoint.text = td.bearingPoint.ToString();

		labelPos.text = Utils.GetTattooApplicablePositions(td.applicableParts);

		labelEffect.text = DataManager.Instance.GetItemAttributesDisplay(itemData);

		if (string.IsNullOrEmpty(td.suiteID))
		{
			labelSuiteEffect.text = string.Empty;
		}
		else
		{
			labelSuiteEffect.text = Localization.instance.Get(DataManager.Instance.GetTattooSuiteData(td.suiteID).descIDS);
		}
	}
}
