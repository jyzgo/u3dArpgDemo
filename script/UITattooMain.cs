using UnityEngine;

using System;
using System.Collections.Generic;

public class UITattooMain : MonoBehaviour
{
	public UILabel labelFightScore;
	public UILabel labelBearingPoint;

	public GameObject attributesPanel;

	public GameObject splitter;
	public UILabel labelAttribute1;
	public UILabel labelAttribute2;	//suite

	private Dictionary<EnumTattooPart, TattooBodySlot> _slotDict;

	private PlayerTattoos _playerTattoos;

	private UITattoo _uiTattoo;

	private EnumTattooPart _selectedPart;
	public EnumTattooPart SelectedTattooPart { get { return _selectedPart; } }

	void Awake()
	{
		_playerTattoos = PlayerInfo.Instance.playerTattoos;

		_uiTattoo = transform.parent.GetComponent<UITattoo>();
	}

	private void UpdateBodySlots()
	{
		_slotDict = new Dictionary<EnumTattooPart, TattooBodySlot>();

		TattooBodySlot[] slots = this.GetComponentsInChildren<TattooBodySlot>();

		int playerLevel = PlayerInfo.Instance.CurrentLevel;

		Dictionary<EnumTattooPart, ItemInventory> dict = PlayerInfo.Instance.playerTattoos.tattooDict;

		foreach (TattooBodySlot slot in slots)
		{
			_slotDict.Add(slot.part, slot);

			ItemInventory ii = null;

			if (dict.ContainsKey(slot.part))
			{
				ii = dict[slot.part];
			}
			slot.SetData(ii, _uiTattoo);
		}
	}

	public void RefreshDisplay()
	{
		UpdateBodySlots();

		labelBearingPoint.text = string.Format("[ffbe00]{2}:[ff0000]{1}[beaa82]/{0}",
			PlayerInfo.Instance.BearingPoint, PlayerInfo.Instance.ConsumedBearingPoint,
			Localization.instance.Get("IDS_MESSAGE_TATTOO_BEARPOINT"));
		
		labelFightScore.text = PlayerInfo.Instance.FightScore.ToString();

		//attributes display
		UpdateAttributesInfo();
	}

	void OnEnable()
	{
		RefreshDisplay();
	}

	private void UpdateAttributesInfo()
	{
		labelAttribute1.text = GetTattooAttributes1();

		int lines = labelAttribute1.processedText.Split('\n').Length;

		string text2 = GetTattooAttributes2();

		if (string.IsNullOrEmpty(text2)) //do we have a suite?
		{
			splitter.SetActive(false);
			labelAttribute2.gameObject.SetActive(false);
		}
		else
		{
			splitter.SetActive(true);
			labelAttribute2.gameObject.SetActive(true);

			float offsetY = 115 - 20 * lines - 25;  //attribute1 start; 20 = line height; 29 = splitter offset

			splitter.transform.localPosition = Vector3.up * offsetY + Vector3.back * 4;

			labelAttribute2.text = "[ffbe00]" + Localization.instance.Get("IDS_MESSAGE_TATTOO_SUITATTRIBUTE") + "[-]" + GetTattooAttributes2();

			lines = labelAttribute2.processedText.Split('\n').Length;

			offsetY -= 29;  //29 = attribute text 2 start

			labelAttribute2.transform.localPosition = Vector3.up * offsetY + Vector3.right * labelAttribute1.transform.localPosition.x;
		}
	}

	//called when clicking on a slot
	public void OnClickOnSlot(TattooBodySlot slot)
	{
		_selectedPart = slot.part;

		if (slot.IsLocked)
		{
			UIMessageBoxManager.Instance.ShowMessageBox(string.Format(Localization.instance.Get("IDS_TATTOO_WILL_UNLOCK_AT_LEVEL"),
				Localization.instance.Get(Utils.k_tattoo_part_names[(int)slot.part]), slot.UnlockLevel), null, MB_TYPE.MB_OK, null);
		}
		else if (slot.item == null) //empty slot
		{
			_uiTattoo.OpenTattooSelectionWindow(slot);
		}
		else //occupied
		{
			_uiTattoo.ShowTattooDetailWindow(slot, null);
		}
	}

	//singles
	private string GetTattooAttributes1()
	{
		Dictionary<AIHitParams, float> dict = new Dictionary<AIHitParams, float>();

		foreach (ItemInventory ii in _playerTattoos.tattooDict.Values)
		{
			HandleEquipmentAttribute(ii, dict);
		}

		//sort by order
		string attributeText = string.Empty;

		foreach (AIHitParams param in DataManager.Instance.tattooAttributeList.dataList)
		{
			if (dict.ContainsKey(param))
			{
				attributeText += DataManager.Instance.GetAttriubteDisplay(param, dict[param]) + "\n";
			}
		}

		if (string.IsNullOrEmpty(attributeText))
		{
			return attributeText;
		}
		else
		{
			return attributeText.Substring(0, attributeText.Length - 1);
		}
	}

	//suite
	private string GetTattooAttributes2()
	{
		List<ItemInventory> itemList = new List<ItemInventory>();

		itemList.AddRange(_playerTattoos.tattooDict.Values);

		Dictionary<AIHitParams, float> dict = new Dictionary<AIHitParams, float>();

		foreach (TattooSuiteData tsd in DataManager.Instance.tattooSuiteDataList.dataList)
		{
			bool included = true;
			foreach (string tattooID in tsd.tdList)
			{
				bool found = false;
				foreach (ItemInventory item in itemList) //at most 1 unique ID
				{
					if (item.ItemID == tattooID)
					{
						found = true;
						break;
					}
				}
				included = included && found;

				if (!included)
				{
					break;
				}
			}

			if (!included)
			{
				continue;
			}

			//a suite is found
			HandleTattooSuiteAttribute(tsd, dict);

			//remove the items from list
			foreach (string tattooID in tsd.tdList)
			{
				ItemInventory item = itemList.Find(delegate(ItemInventory ii) { return ii.ItemID == tattooID; });

				itemList.Remove(item);
			}
		}

		//sort by order
		string attributeText = string.Empty;

		foreach (AIHitParams param in DataManager.Instance.tattooAttributeList.dataList)
		{
			if (dict.ContainsKey(param))
			{
				attributeText += DataManager.Instance.GetAttriubteDisplay(param, dict[param]) + "\n";
			}
		}

		if (attributeText.Length > 0)
		{
			return attributeText.Substring(0, attributeText.Length - 1);
		}
		else
		{
			return attributeText;
		}
	}


	private void HandleEquipmentAttribute(ItemInventory ii, Dictionary<AIHitParams, float> dict)
	{
		AddAttribute(ii.ItemData.attrId0, ii.ItemData.attrValue0, dict);
		AddAttribute(ii.ItemData.attrId1, ii.ItemData.attrValue1, dict);
		AddAttribute(ii.ItemData.attrId2, ii.ItemData.attrValue2, dict);
	}

	private void AddAttribute(AIHitParams param, float value, Dictionary<AIHitParams, float> dict)
	{
		if (dict.ContainsKey(param))
		{
			dict[param] += value;
		}
		else
		{
			dict.Add(param, value);
		}
	}

	private void HandleTattooSuiteAttribute(TattooSuiteData tsd, Dictionary<AIHitParams, float> dict)
	{
		AddAttribute(tsd.attribute0, tsd.value0, dict);
		AddAttribute(tsd.attribute1, tsd.value1, dict);
		AddAttribute(tsd.attribute2, tsd.value2, dict);
	}
}

