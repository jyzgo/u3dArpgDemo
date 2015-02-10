using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UIMessageBoxManager : MonoBehaviour
{
	public GameObject mbOKPrefab;
	public GameObject mbOKWithItemsPrefab;
	public GameObject mbOKCancelPrefab;
	public GameObject mbWaitingPrefab;
	public GameObject mbInputPrefab;
	public GameObject mbShortcutIAPPrefab;
	public GameObject mbWaitingDelayPrefab;
	public GameObject mbFloatingPrefab;

	private bool _iapIsCreated = false;

	private List<GameObject> _messageBoxList = new List<GameObject>();

	private Dictionary<MB_TYPE, GameObject> _singletonMessageboxMapping = new Dictionary<MB_TYPE, GameObject>();  //all singleton message boxes

	#region Singleton
	private static UIMessageBoxManager _instance = null;
	public static UIMessageBoxManager Instance
	{
		get
		{
			if (_instance == null || !_instance)
			{
				_instance = FindObjectOfType(typeof(UIMessageBoxManager)) as UIMessageBoxManager;
			}

			return _instance;
		}
	}
	#endregion

	void Awake()
	{
		DontDestroyOnLoad(transform.gameObject);
	}

	public void OnDestroy()
	{
		_instance = null;
	}

	//is there any box opening?
	public bool hasBoxOpening()
	{
		return (_messageBoxList.Count > 0);
	}


	public void CloseAllMessageBox()
	{
		for (int i = _messageBoxList.Count - 1; i >= 0; i++)
		{
			CloseMessageBox(_messageBoxList[i]);
		}
	}

	public void CloseMessageBox(GameObject messageBox)
	{
		_messageBoxList.Remove(messageBox);
		NGUITools.Destroy(messageBox);
	}

	public void HideMessageBox(MB_TYPE mbType)
	{
		//this must be a singleton messagebox
		if (_singletonMessageboxMapping.ContainsKey(mbType))
		{
			_singletonMessageboxMapping[mbType].SetActive(false);
		}
	}

	public GameObject ShowMessageBox(string text, string caption, MB_TYPE type, UIMessageBox.OnClickCallback clickCallback, params System.Object[] args)
	{
		return ShowMessageBox(text, caption, null, null, type, clickCallback, args);
	}

	public GameObject ShowMessageBox(string text, string caption, string leftStrIDS, string rightStrIDS, MB_TYPE type, UIMessageBox.OnClickCallback clickCallback, params System.Object[] args)
	{
		GameObject messageBox;

		GameObject parent = MessageBoxCamera.Instance.uiCamera.gameObject;

		switch (type)
		{
			case MB_TYPE.MB_OK:
				messageBox = NGUITools.AddChild(parent, mbOKPrefab);
				messageBox.GetComponent<UIMessageBox>().LocalizeButtonText(leftStrIDS, null);
				break;

			case MB_TYPE.MB_OK_WITH_ITEMS:
				messageBox = NGUITools.AddChild(parent, mbOKWithItemsPrefab);
				messageBox.GetComponent<UIMessageBox>().LocalizeButtonText(leftStrIDS, null);
				break;

			case MB_TYPE.MB_WAITING_DELAY:
				messageBox = NGUITools.AddChild(parent, mbWaitingDelayPrefab);
				break;

			case MB_TYPE.MB_OKCANCEL:
				messageBox = NGUITools.AddChild(parent, mbOKCancelPrefab);
				messageBox.GetComponent<UIMessageBox>().LocalizeButtonText(leftStrIDS, rightStrIDS);
				break;

			case MB_TYPE.MB_WAITING:
				messageBox = NGUITools.AddChild(parent, mbWaitingPrefab);
				break;

			case MB_TYPE.MB_INPUT:
				messageBox = NGUITools.AddChild(parent, mbInputPrefab);
				break;

			case MB_TYPE.MB_FLOATING:
				//check singletons
				if (_singletonMessageboxMapping.ContainsKey(MB_TYPE.MB_FLOATING))
				{
					messageBox = _singletonMessageboxMapping[MB_TYPE.MB_FLOATING];

					if (!messageBox.activeSelf)
					{
						messageBox.SetActive(true);
					}
				}
				else
				{
					messageBox = NGUITools.AddChild(parent, mbFloatingPrefab);
					_singletonMessageboxMapping.Add(MB_TYPE.MB_FLOATING, messageBox);
				}
				break;

			default:
				messageBox = null;
				Assertion.Check(false);
				break;

		} //end switch

		if (!_singletonMessageboxMapping.ContainsKey(type))
		{
			_messageBoxList.Add(messageBox);
		}

		int layer = _messageBoxList.Count;
		if (type == MB_TYPE.MB_FLOATING)
		{
			layer += _singletonMessageboxMapping.Count;
		}

		messageBox.transform.localPosition = Vector3.back * layer * 5;

		UIMessageBox messageboxScript = messageBox.GetComponent<UIMessageBox>();
		messageboxScript.SetParams(text, caption, clickCallback, args);

		return messageBox;
	}

	private string _uiToClose;	//the ui name to close if user chooses to expand bag, buy sc, hc, vitality
	private int _errorCode;
	public void ShowErrorMessageBox(int errorCode, string uiNow)
	{
		if (GameManager.Instance.GameState == EnumGameState.InTown ||
			GameManager.Instance.GameState == EnumGameState.InBattle && (errorCode == 5003 || errorCode == 5004))
		{
			_errorCode = errorCode;
			
			_uiToClose = uiNow;
			
			string rightButtonText = null;

			switch (_errorCode)
			{
				case 5001: //buy hc
					rightButtonText = "IDS_BUTTON_GLOBAL_BUY";
					break;

				case 5002: //buy sc
					rightButtonText = "IDS_BUTTON_GLOBAL_EXCHANGE";
					break;

				case 5003: //expand bag
					rightButtonText = "IDS_TITLE_INVENTORYADDSPACE";
					break;

				case 5004: //buy vitality
					rightButtonText = "IDS_BUTTON_GLOBAL_BUY";
					break;
			}//end switch

			this.ShowMessageBox(Utils.GetErrorIDS(errorCode), null,
				"IDS_BUTTON_GLOBAL_BACK",
				rightButtonText,
				MB_TYPE.MB_OKCANCEL, OnErrorMessageboxChoice);
		}
		else
		{
			UIMessageBox.OnClickCallback callback = null;

			if (errorCode == 7001 || errorCode == 7003 || errorCode == 7004)
			{
				callback = OnFatalError;
			}

			this.ShowMessageBox(Utils.GetErrorIDS(errorCode), null, MB_TYPE.MB_OK, callback);
		}
	}

	private void OnErrorMessageboxChoice(ID_BUTTON buttonID)
	{
		if (buttonID == ID_BUTTON.ID_OK)
		{
			switch (_errorCode)
			{
				case 5001: //buy hc
                    UIManager.Instance.CloseAllOpenUI();
					//UIManager.Instance.CloseUI(_uiToClose);
					UIManager.Instance.OpenUI("Store", 1);
					break;
				
				case 5002: //buy sc
					UIManager.Instance.CloseUI(_uiToClose);
					UIManager.Instance.OpenUI("Store", 2);
					break;
				
				case 5003: //expand bag
					FCUIInventoryItemList.OnClickIncrementButton();
					break;

				case 5004: //buy vitality
					UIManager.Instance.CloseUI(_uiToClose);
					if (FCTopBar.Instance != null)
					{
						FCTopBar.Instance.BuyVitality();
					}
					break;
			}//end switch
		}

		_errorCode = 0;
		_uiToClose = null;
	}

	private void OnFatalError(ID_BUTTON buttonID)
	{
		Application.LoadLevel("PreBoot");
	}
}
