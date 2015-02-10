using UnityEngine;

using System;
using System.Collections.Generic;

public class UITattooSuiteItem : MonoBehaviour
{
	public UILabel labelName;
	public GameObject boardBG;
	public GameObject iconPlus;
	public GameObject iconMinus;
	public GameObject selectionEffect;

	public TattooSuiteData suiteData { get; set; }

	private UITattooSuiteQuery _caller;

	public bool IsSummaryLine { get; set; }

	private bool _expanded;

	public void SetData(TattooSuiteData data, UITattooSuiteQuery uiMake)
	{
		_caller = uiMake;

		suiteData = data;

		DisplayInfo();
	}

	void DisplayInfo()
	{
		if (IsSummaryLine)
		{
			boardBG.SetActive(true);

			labelName.text = Localization.instance.Get("IDS_MESSAGE_TATTOO_SUITELEVEL_" + suiteData.level.ToString());
			
			iconMinus.SetActive(true);

			iconPlus.SetActive(false);

			selectionEffect.SetActive(false);
		}
		else
		{
			boardBG.SetActive(false);

			labelName.text = suiteData.GetDisplayName();

			iconMinus.SetActive(false);

			iconPlus.SetActive(false);

			selectionEffect.SetActive(false);
		}
	}

	void OnClick()
	{
		if (IsSummaryLine)
		{
			if (_expanded)
			{
				_caller.CollapseLevel(suiteData.rareLevel);

				iconPlus.SetActive(true);

				iconMinus.SetActive(false);
			}
			else
			{
				_caller.ExpandLevel(suiteData.rareLevel);

				iconPlus.SetActive(false);

				iconMinus.SetActive(true);
			}
			_expanded = !_expanded;
		}
		else
		{
			_caller.SetFocus(this);
		}
	
	}

	public void OnFocused(bool focused)
	{
		selectionEffect.SetActive(focused);
	}
}
