using UnityEngine;
using System.Collections.Generic;
using InJoy.FCComm;

public class UIProfileHandler : MonoBehaviour
{
	private class NameLength
	{
		public int _min;
		public int _max;
		public int _length;
		
		public NameLength(int min, int max, int length)
		{
			_min = min;
			_max = max;
			_length = length;
		}
	}
	
	public UILabel _accountName;
	public GameObject _bg;
	public GameObject _logo;
	public GameObject _popup;
	
	public GameObject _yes;
	public GameObject _no;
	public GameObject _ok;
	public TweenAlpha _alpha;
	public UIInput _input;
	public UILabel _nickname;
	public TweenPosition _tween;
	
    //private bool _isMoveUp = false;
	
	private const int MAX_NAME_LENGTH = 6;
	private const int MIN_NAME_LENGTH = 2;
	
	public static bool _isFirstChangeName = false;
	
	private List<UIProfileHandler.NameLength> _nameLengths = new List<UIProfileHandler.NameLength>();
	
	void Awake()
	{
		_nameLengths.Add(new UIProfileHandler.NameLength(0x3400, 0xFAFF, 2));
	}
	
	private void OnClickOK(ID_BUTTON btn)
	{
		_isFirstChangeName = false;
		OnCancel();
	}
	
	void OnAlphaDone()
	{
		if(_logo != null)
		{
			//_logo.SetActive(true);
			_popup.SetActive(true);
		}
	}
	
	public void OnInitialize()
	{
		if(_isFirstChangeName)
		{
			_ok.SetActive(true);
			_alpha.gameObject.SetActive(true);
			_alpha.Play(true);
		}
		else
		{
			_yes.SetActive(true);
			_no.SetActive(true);
            _nickname.text = "tmp";
		}
		_input.maxChars = MAX_NAME_LENGTH * 2;
	}
	
	void OnSelect(object o)
	{
		bool isSelected = _input.selected;
		_tween.enabled = true;
		_tween.Play(isSelected);
	}
	
	void OnApplicationPause(bool pause)
	{
		if(!pause)
		{
			_input.selected = false;
			OnSelect(null);
		}
	}

	// Use this for initialization
	void Start () 
	{
		_accountName.text = "";
	}
	
	// Update is called once per frame
//	void Update ()
//	{
//		string text = _accountName.text;
//		if(text.EndsWith(_input.caratChar))
//		{
//			text = text.Substring(0, text.Length - 1);
//		}
//		bool isAllEnglish = Utils.IsAllNumberOrEnglish(text);
//		_input.maxChars = isAllEnglish ? MAX_NAME_LENGTH * 2 : MAX_NAME_LENGTH;
//	}
	
	void OnClickCommitAccountName()
	{
		_tween.Play(false);
		if (_accountName.text.Length > 0)
		{
			string text = _accountName.text.Trim();
			if(checkName(text))
			{
				if(PlayerInfo.Instance.first_tutorial_start_PH)
				{
                    PlayerInfo.Instance.first_tutorial_start_PH = false;
				}
			}
			else
			{
				_accountName.text = text;
			}
		}
	}
	
	
	void OnCancel()
	{
		_tween.Play(false);
		if(_logo != null)
		{
			UIManager.Instance.CloseUI("First");
		}
		else
		{
			UIManager.Instance.CloseUI("Nickname");
		}
		UIManager.Instance.OpenUI("TownHome");
		//UIManager.Instance.OpenUI("UIGameSettings");
	}

	private bool checkName(string text)
	{
		//int len = getNameLength(text);
		int len = text.Length;
		bool isAllEnglish = Utils.IsAllNumberOrEnglish(text);
		if(
#if !ENABLE_OTHER_LETTER_FOR_NAME
			!isAllEnglish ||
			len < MIN_NAME_LENGTH * 2 || len > MAX_NAME_LENGTH * 2 ||
#endif
			Utils.IsContainIllegalSymbols(text))
		{
			string message = Localization.instance.Get("IDS_CONTAIN_SPECIAL_CHARACTERS");
			message = string.Format(message, MIN_NAME_LENGTH * 2, MAX_NAME_LENGTH * 2);
			UIMessageBoxManager.Instance.ShowMessageBox(message, Localization.instance.Get("IDS_MESSAGE_TITLE_GLOBAL"), MB_TYPE.MB_OK, null);
			return false;
		}
		
		if(
#if !ENABLE_OTHER_LETTER_FOR_NAME
			!isAllEnglish ||
#endif
			(isAllEnglish
			? (len < MIN_NAME_LENGTH * 2 || len > MAX_NAME_LENGTH * 2)
			: (len < MIN_NAME_LENGTH || len > MAX_NAME_LENGTH))
			)
			
		{
			string message = Localization.instance.Get("IDS_MESSAGE_NICKNAME_LENGTH");
			message = string.Format(message,
				isAllEnglish ? MIN_NAME_LENGTH * 2 : MIN_NAME_LENGTH,
				isAllEnglish ? MAX_NAME_LENGTH * 2 : MAX_NAME_LENGTH);
			UIMessageBoxManager.Instance.ShowMessageBox(message, Localization.instance.Get("IDS_MESSAGE_TITLE_GLOBAL"), MB_TYPE.MB_OK, null);
			return false;
		}
		if(Utils.FilterWords(text))
		{
			return true;
		}
		UIMessageBoxManager.Instance.ShowMessageBox(Localization.instance.Get("IDS_MESSAGE_NICKNAME_ILLEGAL"), Localization.instance.Get("IDS_MESSAGE_TITLE_GLOBAL"), MB_TYPE.MB_OK, null);
		return false;
	}
	
	private int getNameLength(string text)
	{
		//int sum = 0;
		foreach(char ch in text)
		{
			if(!((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z')))
			{
				return -1;
			}
			/*foreach(UIProfileHandler.NameLength nl in _nameLengths)
			{
				if(ch >= nl._min && ch <= nl._max)
				{
					sum += nl._length - 1;
					break;
				}
			}*/
		}
		return text.Length;
	}
}
