using UnityEngine;

using System;
using System.Collections.Generic;

//used in Sell Recipe UI
public class UIRecipeItem3 : MonoBehaviour
{
	public GameObject boardBG;
	public UILabel recipeName;
	public GameObject iconPlus;
	public GameObject iconMinus;
	public GameObject selectionEffect;

	public bool IsSummaryLine { get; set; }

	public bool HasLearned { get; set; }

	public TattooData ttData { get; set; }

	private bool _expanded;

	private UITattooSellRecipe _caller;

	private int _amount;

	void Start()
	{
		_expanded = true;
	}

	public void SetData(TattooData data, int amount, UITattooSellRecipe caller)
	{
		_caller = caller;

		_amount = amount;

		ttData = data;

		HasLearned = PlayerInfo.Instance.playerMasteredTattoos.Contains(data.tattooID);

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

			recipeName.text = string.Format("{0} x [00ff00]{1}", Localization.instance.Get(DataManager.Instance.GetItemData(ttData.recipeID).nameIds), _amount);

			recipeName.color = FCConst.RareColorMapping[(EnumRareLevel)ttData.level];

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
