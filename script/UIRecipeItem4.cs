using UnityEngine;

using System;
using System.Collections.Generic;

//used in Sell Recipe UI
public class UIRecipeItem4 : MonoBehaviour
{
	public GameObject boardBG;
	public UILabel recipeName;
	public GameObject iconPlus;
	public GameObject iconMinus;
	public GameObject selectionEffect;

	public bool IsSummaryLine { get; set; }

	public TattooData ttData { get; set; }

	private bool _expanded;

	private UITattooDisenchant _caller;

	private int _amount;

	void Start()
	{
		_expanded = true;
	}

	public void SetData(TattooData data, int amount, UITattooDisenchant caller)
	{
		_caller = caller;

		_amount = amount;

		ttData = data;

		DisplayInfo();
	}

	void DisplayInfo()
	{
		if (IsSummaryLine)
		{
			boardBG.SetActive(true);

			recipeName.text = Localization.instance.Get("IDS_MESSAGE_TATTOO_TATTOOLEVEL_" + ttData.level.ToString());
			
			iconMinus.SetActive(true);

			iconPlus.SetActive(false);

			selectionEffect.SetActive(false);
		}
		else
		{
			boardBG.SetActive(false);

			recipeName.text = string.Format("{0} x [00ff00]{1}", ttData.GetDisplayName(), _amount);
			
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
				_caller.CollapseLevel(ttData.level);

				iconPlus.SetActive(true);

				iconMinus.SetActive(false);
			}
			else
			{
				_caller.ExpandLevel(ttData.level);

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
