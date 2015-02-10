using UnityEngine;

using System;
using System.Collections.Generic;

public class UIRecipeItem : MonoBehaviour
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

	private UITattooMake _uiMake;

	void Start()
	{
		_expanded = true;
	}

	public void SetData(TattooData data, UITattooMake uiMake)
	{
		_uiMake = uiMake;

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
			//"IDS_TATTOO_READY_LEANRED"
			if (HasLearned)
			{
				recipeName.text = string.Format("{0}({1})", ttData.GetDisplayName(), Localization.instance.Get("IDS_TATTOO_HAVE_LEARNED"));
			}
			else
			{
				if (PlayerInfo.Instance.PlayerInventory.GetItemCount(ttData.recipeID) > 0)	//do I have the recipe?
				{
					recipeName.text = string.Format("{0}({1})", ttData.GetDisplayName(), Localization.instance.Get("IDS_TATTOO_READY_LEANRED"));
				}
				else
				{
					recipeName.text = string.Format("{0}({1})", ttData.GetDisplayName(), Localization.instance.Get("IDS_TATTOO_NOT_LEANRED"));
				}
			}

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
				_uiMake.CollapseLevel(ttData.level);

				iconPlus.SetActive(true);

				iconMinus.SetActive(false);
			}
			else
			{
				_uiMake.ExpandLevel(ttData.level);

				iconPlus.SetActive(false);

				iconMinus.SetActive(true);
			}
			_expanded = !_expanded;
		}
		else
		{
			_uiMake.SetFocus(this);
		}
	}

	public void OnFocused(bool focused)
	{
		selectionEffect.SetActive(focused);
	}
}
