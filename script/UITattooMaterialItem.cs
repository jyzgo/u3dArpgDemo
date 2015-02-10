using UnityEngine;

using System;
using System.Collections.Generic;

public class UITattooMaterialItem : UICommonDisplayItem
{
	public override void  UpdateDisplay()
	{
		base.UpdateDisplay();

		this.amount.text = string.Format("{1}/{0}", _amount, PlayerInfo.Instance.PlayerInventory.GetItemCount(_itemData.id));
	}
}
