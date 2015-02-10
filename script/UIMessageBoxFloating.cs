using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIMessageBoxFloating : UIMessageBox
{
	public MessageBoxPopupItemInfo itemInfoPanel;

	public MessageBoxPopupTattooInfo tattooInfoPanel;

	public override void SetParams(string text, string caption, OnClickCallback callback, params System.Object[] args)
	{
		if (args[0] is ItemData)
		{
			itemInfoPanel.gameObject.SetActive(true);
			tattooInfoPanel.gameObject.SetActive(false);
			
			itemInfoPanel.DisplayItemInfo(args[0] as ItemData);
		}
		else if (args[0] is string)
		{
			itemInfoPanel.gameObject.SetActive(false);
			tattooInfoPanel.gameObject.SetActive(true);

			tattooInfoPanel.DisplayTattooInfo(args[0] as string);
		}
	}
}
