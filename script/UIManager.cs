using UnityEngine;
using System.Collections.Generic;
using InJoy.FCComm;


/// <summary>
/// Declare this delegate in logic class.
/// Register callback event in EnAble() of UI class and unregister it in DisAble() of the same class;
/// 
/// Example:
/// void OnEnable()
//	{
//		controller._notifyEvent += new NotifyEventHandler(this.ChangeButtonText);
//	}
//	
//	void OnDisable()
//	{
//		controller._notifyEvent -= new NotifyEventHandler(this.ChangeButtonText);
//	}
//
//  _notifyEvent(this);
//
/// </summary>
public delegate void NotifyEventHandler(object sender);


/// <summary>
/// User interface manager.
/// All UI will be registered here.
/// Open and Close UI by UIManager.
/// </summary>
public class UIManager : MonoBehaviour
{
	#region Initialize from cache
	[System.Serializable]
	public class UIPairElement
	{
		public string _name;
		public GameObject _UI;
	}
	
	public UIPairElement[] _UICacheList;
	#endregion
	
	public GameObject _UICamera;
	public Camera MainCamera
	{
		get
		{
			return _UICamera.GetComponent<Camera>();
		}
	}
	
	public GameObject _entryUI;
	
	public UIIndicatorManager _indicatorManager;
	
	private Dictionary<string,GameObject> _guiList = new Dictionary<string, GameObject>();
	
	// boss UI related.
	public delegate void DelegateVoid_Float(float param);
	public delegate void DelegateVoid_Int(int param);
	public delegate void DelegateVoid_Bool_Int_String(bool param, int steps, string param1);
	public DelegateVoid_Float SetBossHP = null;
	public DelegateVoid_Float SetBossShield = null;
	public DelegateVoid_Int SetBossShieldInfo = null;
	public DelegateVoid_Bool_Int_String SetBossHPDisplay = null;
	

	private void Initialize()
	{
		foreach (UIPairElement element in _UICacheList)
		{
			element._UI.SetActive(false);
			Register(element._name, element._UI);
		}
		               
		_UICacheList = null;
		
		if (_entryUI != null)
		{
			_entryUI.SetActive(true);
			_entryUI.SendMessage("OnInitialize", SendMessageOptions.DontRequireReceiver);
		}
	}
	
	
	
	#region Singleton
	private static UIManager _instance = null;
	public static UIManager Instance
	{
		get
		{
			if (_instance == null || !_instance)
			{
				_instance = FindObjectOfType(typeof(UIManager)) as UIManager;
			}
			
			return _instance;
		}
	}
	#endregion
	
	
	// Use this for initialization
	void Awake()  
	{
		Initialize();

        OpenUI("reward hint");
	}
	
	void OnDestroy() {
		SetBossHP = null;
		SetBossHPDisplay = null;
		SetBossShield = null;
		_instance = null;
	}
	
	
	#region Register&Unresigter UI
	public void Register(string uiname, GameObject go)
	{
		if (IsRegistered(uiname))
		{
			Debug.LogError("[GUI] UI<"+uiname+"> is already exist!"); 
		}
		
		_guiList[uiname] = go;
	}
	
	public void Unresigiter(string uiname)
	{
		if (!IsRegistered(uiname))
		{
			Debug.LogError("[GUI] UI<"+uiname+"> is not exist!"); 
		}
		else
		{
			_guiList.Remove(uiname);
		}
	}
	
	private bool IsRegistered(string uiname)
	{
		return _guiList.ContainsKey(uiname);
	}
	#endregion
	
	
	private string _currentUI = string.Empty;
	
	public string CurrentUI
	{
		get {return _currentUI;}
	}
	#region Open&Close UI for Scene

	// Return false if uiname has't been registered.
	public bool OpenUI(string uiname, object obj = null)
	{
		GameObject ui = GetUI(uiname);
		if (ui != null)
		{
			_currentUI = uiname;
			ui.SetActive(true);
			
			// Each root gameobject of ui can has "OnInitialize" to initialize ui data.
			if(obj != null)
			{
				ui.SendMessage("OnInitializeWithCaller", obj, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				ui.SendMessage("OnInitialize", SendMessageOptions.DontRequireReceiver);
			}
			return true;
		}
		
		return false;
	}
	
	public void CloseUI(string uiname)
	{
		GameObject ui = GetUI(uiname);
		if (ui != null)
		{
			ui.SetActive(false);
		}

		if (uiname == _currentUI)
		{
			_currentUI = string.Empty;
		}
	}

    public void CloseAllOpenUI()
    {
        foreach (string uiName in _guiList.Keys)
        {
            CloseUI(uiName);
        }
    }

	public void CloseLastOpenedUI()
	{
		CloseUI(_currentUI);
	}
	
	public bool IsUIOpened(string uiname)
	{
		GameObject ui = GetUI(uiname);
		if (ui == null)
		{
			return false;
		}

        return ui.activeInHierarchy;
	}
	
	public GameObject GetUI(string uiname)
	{
		if (!IsRegistered(uiname))
		{
			Debug.LogWarning("[GUI] UI<"+uiname+"> not found!");
			
			return null;
		}
		else
		{
			return _guiList[uiname];
		}
	}
	#endregion
}
