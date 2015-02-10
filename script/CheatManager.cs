using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;
using InJoy.FCComm;
using InJoy.Utils;
using InJoy.DynamicContentPipeline;

public class CheatManager : MonoBehaviour
{
	#region Always exist, both release and development build
	//show the attack details
	public static bool showFPS;
	public static bool showAttackInfo = false;
	public static int flashMode = 0;

	public static float netDelay = -0.3f;
	#endregion

	#region Be careful! NOT in RELEASE version!
#if DEVELOPMENT_BUILD || UNITY_EDITOR
	//cheat should be disable all in release mode
	//public vars
	public static bool chargeWithNoKeyHold = false;
	public static bool lockCamera = false;

	public static bool needCheckSplashText = false;
	public static bool isShowLoadingInCheat = false;

	public static bool dpsCountEnabled = false;
	public static int hitCount;
	public static float hitTime;

	public static float damageTotal;
	public static bool cheatForCostNoEnergy = false;
	public static bool disableBladeSlide = false;
	public static bool superManHalf = false;
	public static bool superMan = false;
	public static bool superMan2 = false;
    public static bool weaponCollider = false;

	public static bool noCDTime = false;
	public static bool changeNickname = false;
	public static bool tutorialMaskDebug;

	//end public

	private string dpsInfo = "";

	private bool showBattleHud = true;

	private List<string> loadingTipList;
	private int _loadingTipIndex = 0;
	private int _curIndex = 0;
	private string[] _paramType = new string[3];
	private float[] _paramValue = new float[3];
	private string _itemId = "";
	private string _dropItemId = "";

	private int _frameRate = 30;
	private int _playerLevel = 5;

	private ItemInventory _currentEquip;

	//Define the area in the top right corner of the screen that may open the cheat menu
	private float _openCheatMenuAreaWidth = 70.0f;
	private float _openCheatMenuAreaHeight = 70.0f;
	private bool _cheatsMenuVisible = false;

	private string _cheatCommand = "Cheat Command";
	private string _cheatMessage = "";

	public bool CheatMenuVisible
	{
		set
		{
			_cheatsMenuVisible = value;
			if (_cheatsMenuVisible)
			{
				GameManager.Instance.GamePaused = true;
			}
			else
			{
				GameManager.Instance.GamePaused = false;
			}

			//add a collider mask to block all 2D clicks
			GameObject uiRoot = UIManager.Instance.gameObject;
			BoxCollider bc = uiRoot.GetComponent<BoxCollider>();
			if (bc == null)
			{
				bc = uiRoot.AddComponent<BoxCollider>();
			}
			if (value)
			{
				bc.size = new Vector3(1500, 1000, 0);
				bc.center = new Vector3(0, 0, -110);
				bc.enabled = true;
			}
			else
			{
				bc.enabled = false;
			}
		}
	}
	private float _openCheatsMenuTimer = 0.0f;

	//Time to hold the mouse or touch pressed in order to open the cheats menu
	private float _openCheatsMenuTime = 1.0f;

	//Minimum resolution the game will support is 800x480, so the menu must be smaller than that.
	private float _cheatMenuWidthPercentage = 1.0f;
	private float _cheatMenuHeightPercentage = 0.8f;
	private float _cheatMenuWidth = 0.0f;
	private float _cheatMenuHeight = 0.0f;
	private float _cheatMenuBorderX = 3.0f;
	private float _cheatMenuBorderY = 3.0f;


	//flags and values
	private float _softCurrencyIncrement = 1000;
	private float _hardCurrencyIncrement = 1000;
	private float _tokenIncrement = 100;
	private float _xpIncrement = 1000;

	private float _hitPoint = 1000;
	private float _damagePoint = 1000;
	private float _defensePoint = 1000;

	//FPS
	//private float _updateInterval = 0.5F;
	//private float _accum = 0; // FPS accumulated over the interval
	//private int _frames = 0; // Frames drawn over the interval
	//private float _timeleft = 0; // Left time for current interval	
	//private string _fpsText = "";

	private int _dailyBonusClaimTimeDelta = 0;
	private int _activeDays = 0;

	//private string _playhavenPlacementName = "announcement name";
	static public bool _displayIAPDebug = false;

	// Game Components
	// static private ParticleManager _particleManager = null;
	/*public ParticleManager ParticleMgr
	{
		get
		{
			return _particleManager;
		}
		set
		{
			_particleManager = value;
			if(_particleManager != null)
			{
				_particleManager.DisableAll = _disableParticle;
			}
		}
	}*/
	private Texture2D _boxTexture = null;
	private GUIStyle _boxStyle = null;
	private int _cheatPanel = 0;

	void Start()
	{
		_boxTexture = new Texture2D(1, 1);
		Color black = new Color(0.5f, 0.0f, 0.0f, 0.5f);
		_boxTexture.SetPixel(0, 0, black);
		_boxTexture.Apply();

		for (int i = 0; i < 3; i++)
		{
			_paramType[i] = "";
		}
	}

	void OnDestroy()
	{
		/*if (_fpsMemDisplay != null)
		{
			Destroy(_fpsMemDisplay);
		}*/
	}

	void OnEnable()
	{
		disableBladeSlide = (PlayerPrefs.GetInt("_disableBladeSlide") == 1);
	}

	void OnDisable()
	{
		PlayerPrefs.SetInt("_disableBladeSlide", disableBladeSlide ? 1 : 0);
	}

	void DisplayMagicAttack()
	{
		if (showAttackInfo)
		{
			GUILayout.Space(50);
			GUILayout.Label(DamageCounter.attacklog);
		}
	}

	void DisplayDpsInfo()
	{
		if (dpsCountEnabled)
		{
			GUILayout.Space(50);
			dpsInfo = "Time = " + hitTime + ", Hit count = " + hitCount + " , Total Damage = " + damageTotal;
			if (hitCount > 0)
			{
				dpsInfo += "\nDps = " + damageTotal / hitTime + "\n";
			}
			GUILayout.Label(dpsInfo);
		}
	}

#if DEVELOPMENT_BUILD || UNITY_EDITOR

	void OnGUI()
	{
		if (dpsCountEnabled)
		{
			//if((int)_hitTime/2 == 0)
			{
				DisplayDpsInfo();
			}
		}
		if (_boxStyle == null)
		{
			_boxStyle = new GUIStyle(GUI.skin.box);
			_boxStyle.normal.background = _boxTexture;
		}

		if (_displayMyInfo)
		{
			DisplayInfo();
		}

		if (showAttackInfo)
		{
			DisplayMagicAttack();
		}

		if (_cheatsMenuVisible)
		{
			_cheatMenuWidth = Screen.width * _cheatMenuWidthPercentage;
			_cheatMenuHeight = Screen.height * _cheatMenuHeightPercentage;

			GUI.BeginGroup(new Rect(Screen.width / 2 - (_cheatMenuWidth / 2),
									 Screen.height - _cheatMenuHeight,
									 _cheatMenuWidth, _cheatMenuHeight));

			//Make the background more opaque by combining semi-transparent layers
			GUI.Box(new Rect(0, 0, _cheatMenuWidth, _cheatMenuHeight), "", _boxStyle);

			//Define the menu border
			GUILayout.BeginArea(new Rect(_cheatMenuBorderX, _cheatMenuBorderY,
										 _cheatMenuWidth - _cheatMenuBorderX * 2,
										 _cheatMenuHeight - _cheatMenuBorderY * 2));

			GUIStyle labelStyle = GUI.skin.GetStyle("Label");
			labelStyle.alignment = TextAnchor.MiddleCenter;

			GUILayout.BeginHorizontal();
			ProcessCheatPanel();
			ProcessCloseCheatMenu();
			GUILayout.EndHorizontal();

			switch (_cheatPanel)
			{
				case 0:     //FPS, Money, Hero
					GUILayout.BeginHorizontal();
					ToggleFPS();
					ToggleDisplayInfo();
					ProcessIamCheater();
					ProcessClearPrefs();
					ProcessTargetFrameRate();
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					ProcessPlayerLevel();
					ProcessSoftCurrency();
					ProcessHardCurrency();
					ProcessToken();
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					ProcessChangeNickname();
					ProcessUnlockAllLevels();
					ProcessCrash();
					ProcessLockCamera();
					ProcessTutorialEffectDebug();

					GUILayout.EndHorizontal();
					break;

				case 1:    //Server and UI
					GUILayout.BeginHorizontal();
					ProcessSetVipLevel();
					ProcessAddVitality();
					ProcessUpdateIAP();
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					ProcessCheatCommand();
					ProcessServerCheat();
					GUILayout.EndHorizontal();
					break;

				case 2: //Item
					/*
					GUILayout.BeginHorizontal();
					ProcessUpgrade();
					ProcessDropItem();
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					ProcessItemNameList();
					GUILayout.EndHorizontal();
					*/
					GUILayout.BeginHorizontal();
					ProcessItemCheat();

					GUILayout.EndHorizontal();
					break;

				case 3:     //enemy spawn
					GUILayout.BeginHorizontal();
					ProcessSpawnOneEnemy();
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					ProcessEnemyType();
					GUILayout.EndHorizontal();
					break;

				case 4://Quests

					GUILayout.BeginHorizontal();
					ProcessQuests();
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					ProcessModifyActiveDays();
					ProcessModifyDailyBounsClaimDeltaTime();
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					ProcessLoadingText();
					GUILayout.EndHorizontal();
					break;

				case 5:      //battle info
					GUILayout.BeginHorizontal();
					ProcessXp();
					ProcessHP();
					ProcessBattleHud();
					ProcessSetHitpoint();
					ProcessSetDefensepoint();
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					ProcessSetDamagepoint();
					ProcessChargeWithKeyHold();
					ProcessDisableBladeSlide();
					ProcessCameraParam();
					ProcessRabbitDistance();
					ToggleDisplayMagicAttack();
					ToggleDisplayDps();
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					ProcessSuicide();
					ProcessSuperManHalf();
					ProcessSuperMan();
					ProcessSuperMan2();
                    ProcessWeaponCollider();
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					ProcessChangeFlashMode();
					ProcessCostNoEnergy();
					ProcessNoCDTime();
					ProcessTutorial();
					ProcessPassBattle();
					GUILayout.EndHorizontal();
					break;

				case 6:
					GUILayout.BeginVertical();
					ProcessAddSuitEquipmments();
					GUILayout.EndVertical();

					break;

				case 7:
					GUILayout.BeginVertical();
					GUILayout.BeginHorizontal();
					ReportMemUsage();
					GUILayout.EndHorizontal();
					ProcessProfiling();
					GUILayout.EndVertical();
					break;

				default:
					break;
			}
			GUILayout.EndArea();
			GUI.EndGroup();
		}
	}
#endif  //onGUI

	private void ToggleFPS()
	{
		if (GUILayout.Button("Show FPS", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			DebugManager.Instance.fpsCounter.enabled = !DebugManager.Instance.fpsCounter.enabled;
		}
	}


	private void ProcessChargeWithKeyHold()
	{
		if (GUILayout.Button("charge\nmode", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			chargeWithNoKeyHold = !chargeWithNoKeyHold;
		}
	}

	private void ProcessLockCamera()
	{
		if (GUILayout.Button("LockCamera", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			lockCamera = !lockCamera;
			Application.targetFrameRate = 30;
		}
	}


	private void ProcessTutorialEffectDebug()
	{
		if (GUILayout.Button("Tutorial Effect\n\nDebug", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			tutorialMaskDebug = !tutorialMaskDebug;
		}
	}

	//change ice armor effect flash mode
	private void ProcessChangeFlashMode()
	{
		if (flashMode == 0)
		{
			if (GUILayout.Button("Flash mode1", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
			{
				flashMode = 1;
			}
		}
		else if (flashMode == 1)
		{
			if (GUILayout.Button("Flash mode2", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
			{
				flashMode = 2;
			}
		}
		else if (GUILayout.Button("Flash mode3", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			flashMode = 0;
		}
	}

	private void ProcessModifyDailyBounsClaimDeltaTime()
	{
		if (GUILayout.Button("Modify\nDailyBonus\nDays", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			PlayerInfo.Instance.dailyBonusClaimTime -= _dailyBonusClaimTimeDelta * 86400;

			Hashtable ht = new Hashtable();
			ht.Add("_activeDays", PlayerInfo.Instance.activeDays);
			ht.Add("_dailyBonusClaimTime", PlayerInfo.Instance.dailyBonusClaimTime);
			ht.Add("_vipLevel", 0);
			ht.Add("_vipTime", 0);
			ht.Add("_totalIAPHc", 0);
			ht.Add("_totalIAPTimes", 0);
			ht.Add("_rookieGift", 0);

			ht.Add("_firstIAP", 0);
			Hashtable monthCard = new Hashtable();
			monthCard.Add("total_cards", 0);
			monthCard.Add("last_claim_time", 0);
			monthCard.Add("remain_days", 0);
			ht.Add("_monthCardInfo", monthCard);

		}

		int.TryParse(GUILayout.TextField(_dailyBonusClaimTimeDelta.ToString(), GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)), out _dailyBonusClaimTimeDelta);
	}

	private void ProcessModifyActiveDays()
	{
		if (GUILayout.Button("Modify\nActive\nDays", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			PlayerInfo.Instance.activeDays = _activeDays;

			Hashtable ht = new Hashtable();
			ht.Add("_activeDays", PlayerInfo.Instance.activeDays);
			ht.Add("_dailyBonusClaimTime", PlayerInfo.Instance.dailyBonusClaimTime);
			ht.Add("_vipLevel", 0);
			ht.Add("_vipTime", 0);
			ht.Add("_totalIAPHc", 0);
			ht.Add("_totalIAPTimes", 0);
			ht.Add("_rookieGift", 0);

			ht.Add("_firstIAP", 0);
			Hashtable monthCard = new Hashtable();
			monthCard.Add("total_cards", 0);
			monthCard.Add("last_claim_time", 0);
			monthCard.Add("remain_days", 0);
			ht.Add("_monthCardInfo", monthCard);

			//string str = FCJson.jsonEncode(ht);
		}

		int.TryParse(GUILayout.TextField(_activeDays.ToString(), GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)), out _activeDays);
	}

	private void ProcessPassBattle()
	{
		if (GUILayout.Button("PassBattle", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			//UIManager.Instance.OpenUI("BattleSummary");
			BornPoint.FinishLevel();
		}
	}

	private Vector2 _scrollPosition = Vector2.zero;

	private void ProcessItemNameList()
	{
		_scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(Screen.width / 8), GUILayout.Width(Screen.width / 8));
		GUILayout.BeginHorizontal();


		int i = 0;
		foreach (ItemData itemData in DataManager.Instance.ItemDataManager.DataBase.Values)
		{
			if (i % 15 == 0)
			{
				if (GUILayout.Button(itemData.id, GUILayout.Width(Screen.width / 8), GUILayout.MinHeight(80)))
				{
					_itemId = itemData.id;
					_dropItemId = itemData.id;
				}
			}
			i++;
		}

		GUILayout.EndHorizontal();
		GUILayout.EndScrollView();
	}
	private string[] _itemTypes = null;
	private Vector2 _itemTypeScrollPos;
	private int _itemType;
	private Vector2 _itemScrollPos;
	private string[] _items = null;
	private int _item;
	private int _itemCount = 1;

    private Rect _itemWindowRect;   // calculated bounds of the window that holds the scrolling list
    private Rect _itemListRect;  // calculated dimensions of the scrolling list placed inside the window
    public Vector2 _itemButtonSize;

	private void ProcessItemCheat()
	{
		if (_itemTypes == null)
		{
			ItemDataList[] itemDataList = DataManager.Instance._itemDataList;
			_itemTypes = new string[itemDataList.Count()];
			for (int i = 0; i < itemDataList.Count(); i++)
			{
				_itemTypes[i] = itemDataList[i].name;
			}
		}
		GUI.color = Color.yellow;
		_itemTypeScrollPos = GUILayout.BeginScrollView(_itemTypeScrollPos, GUILayout.Width(Screen.width / 5), GUILayout.Height(Screen.height * 3 / 5));
        GUIStyle myStyle = new GUIStyle(GUI.skin.button);
        myStyle.fontSize = 16;
        myStyle.fixedHeight = 32;
		int newItemType = GUILayout.SelectionGrid(_itemType, _itemTypes, 1, myStyle);
		GUILayout.EndScrollView();

		if (_itemType != newItemType)
		{
			_items = null;
			_item = 0;
		}
		_itemType = newItemType;
		GUI.color = Color.cyan;
        
		if (_items == null)
		{
			List<ItemData> itemDatas = DataManager.Instance._itemDataList[_itemType]._dataList;
			_items = new String[itemDatas.Count];
			for (int i = 0; i < itemDatas.Count; i++)
			{
				_items[i] = itemDatas[i].id + " " + Localization.instance.Get(itemDatas[i].nameIds);
			}
			_itemScrollPos = Vector2.zero;
		}
        /*
		_itemScrollPos = GUILayout.BeginScrollView(_itemScrollPos, GUILayout.Width(Screen.width / 2), GUILayout.Height(Screen.height * 3 / 5));
        _item = GUILayout.SelectionGrid(_item, _items, 1, myStyle);
		GUILayout.EndScrollView();
         * */
        GUILayout.Space(Screen.width / 2);
        
        GUILayout.BeginVertical();
        //_itemWindowRect = new Rect(10, 10, Screen.width - 100, Screen.height - 100);
        _itemWindowRect = new Rect(Screen.width / 2 - (_cheatMenuWidth / 2) + Screen.width / 5 + 10,
                                     Screen.height - _cheatMenuHeight + 70 + 10,
                                     Screen.width / 2, Screen.height * 3 / 5);

        _itemListRect = new Rect(10, 20, _itemWindowRect.width - 20, _itemWindowRect.height - 30);

        _itemButtonSize = new Vector2(_itemListRect.width - 20, 35);

        GUI.Window(0, _itemWindowRect, DoItemWindow, "Select Item");
        GUILayout.EndVertical();

        string selectedItemId = "No Selected Item";
        if (_items != null && _items.Count() > 0 && _item >= 0)
			selectedItemId = DataManager.Instance._itemDataList[_itemType]._dataList[_item].id;

		//
		GUILayout.BeginVertical();

		GUILayout.BeginHorizontal();
		GUILayout.TextField(selectedItemId, GUILayout.Width(200), GUILayout.Height(30));
		GUILayout.EndHorizontal();

		GUI.color = Color.white;

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Count: 1 ", GUILayout.Width(100), GUILayout.Height(40)))
		{
			_itemCount = 1;
		}
		if (GUILayout.Button("Count: 10 ", GUILayout.Width(100), GUILayout.Height(40)))
		{
			_itemCount = 10;
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Input Count:", GUILayout.Width(100), GUILayout.Height(30));
		GUI.color = Color.green;
		Int32.TryParse(GUILayout.TextField(_itemCount.ToString(), GUILayout.Width(100), GUILayout.Height(30)), out _itemCount);
		GUILayout.EndHorizontal();

		if (GUILayout.Button("Add Item", GUILayout.Width(200), GUILayout.Height(80)))
		{
			if (_itemCount > 0)
			{
				SendCheatCommand(string.Format("player:add_item(0, {0}, {1})", selectedItemId, _itemCount));
			}
		}
		GUILayout.EndVertical();
		GUI.color = Color.white;
	}

	void DoItemWindow(int windowID)
    {
        _itemScrollPos = GUI.BeginScrollView(_itemListRect, _itemScrollPos, new Rect( 0, 0, _itemButtonSize.x, _items.Count() * _itemButtonSize.y ), false, false);
        Rect rBtn = new Rect(0, 0, _itemButtonSize.x, _itemButtonSize.y);

        GUIStyle myStyle = new GUIStyle(GUI.skin.button);
        myStyle.fontSize = 16;
        myStyle.fixedHeight = 32;

        for (int iRow = 0; iRow < _items.Count(); iRow++)
        {
            // draw call optimization: don't actually draw the row if it is not visible
            if (rBtn.yMax >= _itemScrollPos.y &&
                 rBtn.yMin <= (_itemScrollPos.y + _itemListRect.height))
            {
                bool fClicked = false;
                string rowLabel = _items[iRow];

                if (iRow == _item)
                {
                    //fClicked = GUI.Button(rBtn, rowLabel, rowSelectedStyle);
                    GUI.color = Color.cyan;
                }
                else
                {
                    GUI.color = Color.white;
                }
				fClicked = GUI.Button(rBtn, rowLabel, myStyle);

				// Allow mouse selection, if not running on iPhone.
                if (fClicked && Application.platform != RuntimePlatform.IPhonePlayer)
                {
                    //Debug.Log("Player mouse-clicked on row " + iRow);
                    _item = iRow;
                }
            }

            rBtn.y += _itemButtonSize.y;
        }
        GUI.EndScrollView();
    }

    void UpdateItemWindow()
    {
		if (_cheatPanel != 2)
			return;

        if (Input.touchCount != 1)
        {
            return;
        }

        Touch touch = Input.touches[0];
		bool fInsideList = IsTouchInsideList(touch.position, _itemWindowRect, _itemListRect);
		
		if (touch.phase == TouchPhase.Began && fInsideList)
        {
            _item = TouchToRowIndex(touch.position, _itemScrollPos, _itemWindowRect, _itemListRect, _itemButtonSize.y, _items.Count());
        }
        else if (touch.phase == TouchPhase.Canceled || !fInsideList)
        {
            //_item = -1;
		}
        else if (touch.phase == TouchPhase.Moved && fInsideList)
        {
            // dragging
            //_item = -1;
			_itemScrollPos.y += touch.deltaPosition.y;
		}
        else if (touch.phase == TouchPhase.Ended)
        {
            // Was it a tap, or a drag-release?
            //if (_item > -1 && fInsideList)
            //{
            //    Debug.Log("Player _item row " + _item);
            //}
        }
    }

	void UpdateEnemyWindow()
	{
		if (_cheatPanel != 3)
			return;
		
		if (Input.touchCount != 1)
		{
			return;
		}
		
		Touch touch = Input.touches[0];
		bool fInsideList = IsTouchInsideList(touch.position, _enemyWindowRect, _enemyListRect);
		
		if (touch.phase == TouchPhase.Began && fInsideList)
		{
			_enemyLabelIndex = TouchToRowIndex(touch.position, _enemyScrollPos, _enemyWindowRect, _enemyListRect, _enemyButtonSize.y, _enemyLabelNames.Count());
		}
		else if (touch.phase == TouchPhase.Canceled || !fInsideList)
		{
			//_enemyLabelIndex = -1;
		}
		else if (touch.phase == TouchPhase.Moved && fInsideList)
		{
			// dragging
			//_enemyLabelIndex = -1;
			_enemyScrollPos.y += touch.deltaPosition.y;
		}
		else if (touch.phase == TouchPhase.Ended)
		{
			// Was it a tap, or a drag-release?
			//if (_item > -1 && fInsideList)
			//{
			//    Debug.Log("Player _item row " + _item);
			//}
		}
	}
	
	private int TouchToRowIndex(Vector2 touchPos, Vector2 scrollPos, Rect windowRect, Rect listRect, float rowHeight, int rowCount)
	{
		float y = Screen.height - touchPos.y;  // invert y coordinate
        y += scrollPos.y;  // adjust for scroll position
        y -= windowRect.y;    // adjust for window y offset
        y -= listRect.y;      // adjust for scrolling list offset within the window
        int irow = (int)(y / rowHeight);

		irow = Mathf.Min(irow, rowCount);  // they might have touched beyond last row
        return irow;
    }

	private bool IsTouchInsideList(Vector2 touchPos, Rect windowRect, Rect listRect)
	{
        Vector2 screenPos = new Vector2(touchPos.x, Screen.height - touchPos.y);  // invert y coordinate
        Rect rAdjustedBounds = new Rect(listRect.x + windowRect.x, listRect.y + windowRect.y, listRect.width, listRect.height);

        return rAdjustedBounds.Contains(screenPos);
    }


	private Vector2 _scrollPositionParam = Vector2.zero;

	private void ProcessParamType()
	{
		_scrollPositionParam = GUILayout.BeginScrollView(_scrollPositionParam, GUILayout.Width(Screen.width / 8), GUILayout.Width(Screen.width / 8));
		GUILayout.BeginHorizontal();

		for (int i = 0; i < (int)AIHitParams.Max; i++)
		{
			string name = ((AIHitParams)i).ToString();
			if (GUILayout.Button(name, GUILayout.Width(Screen.width / 8), GUILayout.MinHeight(60)))
			{
				_paramType[_curIndex] = name;
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.EndScrollView();
	}

	private void ProcessAddItemRandom()
	{
		if (GUILayout.Button("Add Item Random", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			PlayerInfo.Instance.AddItemRandom(_itemId);
		}
	}

	private void ProcessAddItemToInventory()
	{
		/*
		if (GUILayout.Button("Add Item", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			PlayerInfoManager.Singleton.AccountProfile.CurPlayerProfile.CheatAddItem(_itemId, _paramType, _paramValue);
		}
		*/
		_itemId = GUILayout.TextField(_itemId, GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8));
	}

	private void ProcessParam()
	{
		for (int i = 0; i < 3; i++)
		{
			GUILayout.BeginHorizontal();

			if (i == _curIndex)
			{
				GUI.color = Color.green;
			}

			if (GUILayout.Button("p" + (i + 1), GUILayout.Width(Screen.width / 8), GUILayout.MaxHeight(40)))
			{
				_curIndex = i;
			}
			_paramType[i] = GUILayout.TextField(_paramType[i], GUILayout.Width(Screen.width / 8), GUILayout.MaxHeight(40));
			GUILayout.Label("value:");
			float.TryParse(GUILayout.TextField(_paramValue[i].ToString(), GUILayout.Width(Screen.width / 8), GUILayout.MaxHeight(40)), out _paramValue[i]);
			GUILayout.EndHorizontal();
			GUI.color = Color.white;
		}
	}

	private void ProcessDropItem()
	{
		if (GUILayout.Button("Add Item to Inventory ", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			PlayerInfo.Instance.AddItemRandom(_dropItemId);
		}


		if (GUILayout.Button("Drop Item in battle", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			if (ObjectManager.Instance != null)
			{
				ActionController player = ObjectManager.Instance.GetMyActionController();

				LootObjData lootData = new LootObjData();
				lootData._lootId = _dropItemId;
				lootData._lootCount = 1;

				float x = UnityEngine.Random.Range(-3, 3);
				float z = UnityEngine.Random.Range(-3, 3);
				LootManager.Instance.LootOneForCheatDrop(lootData, player.transform.position, new Vector3(x, 3, z));
			}
		}
		_dropItemId = GUILayout.TextField(_dropItemId, GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8));
	}


	private int _curSelectSkillIndex = 0;
	private void ProcessSkillSelect()
	{
		int count = PlayerInfo.Instance.activeSkillList.Count;
		for (int i = 0; i < count; i++)
		{
			if (i == _curSelectSkillIndex)
			{
				GUI.color = Color.green;
			}

			string skillName = PlayerInfo.Instance.activeSkillList[i];
			if (GUILayout.Button("KEY_" + (i + 1).ToString() + " : " + skillName, GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
			{
				_curSelectSkillIndex = i;
			}
			GUI.color = Color.white;
		}
	}


	private void ProcessSkillName()
	{

		List<SkillData> usedSkill = DataManager.Instance.GetAllSkill();

		for (int i = 0; i < usedSkill.Count; i++)
		{
			if (GUILayout.Button(usedSkill[i].skillID, GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
			{
				PlayerInfo.Instance.activeSkillList[_curSelectSkillIndex] = usedSkill[i].skillID;


				if (ObjectManager.Instance != null)
				{
					ActionController ac = ObjectManager.Instance.GetMyActionController();
					if (ac != null)
					{
						AttackAgent attackAgent = ac.gameObject.GetComponentInChildren<AttackAgent>();
						attackAgent.InitSkillData(ac.AIUse);
						AIAgent aiAgent = ac.gameObject.GetComponentInChildren<AIAgent>();
						if (aiAgent != null)
						{
							aiAgent.CheatInitKeyMap();
						}
					}
				}

			}
		}
	}

	#region Profiling Panel
	private bool sceneEnabled = true;
	private GameObject sceneRoot = null;
	private bool characterEnabled = true;
	//private GameObject characterRoot = null;
	private bool effectEnabled = true;
	private bool gbufferEnable = true;
	private bool tbufferEnable = true;
	private bool lbufferEnable = true;
	private bool transCamEnable = true;
	private bool uirendererEnable = true;
	private bool uiEnable = true;
	private GameObject gbuffer = null;
	private GameObject tbuffer = null;
	private GameObject lbuffer = null;
	private GameObject transCam = null;
	private GameObject uirenderer = null;
	private void ProcessProfiling()
	{
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(sceneEnabled ? "Disable Scene" : "Enable Scene", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			sceneEnabled = !sceneEnabled;
			if (sceneRoot == null)
			{
				LevelLightInfo lli = GameObject.FindObjectOfType(typeof(LevelLightInfo)) as LevelLightInfo;
				sceneRoot = lli.gameObject;
			}
			sceneRoot.SetActive(sceneEnabled);
			if (sceneEnabled)
			{
				sceneRoot = null;
			}
		}
		if (GUILayout.Button(characterEnabled ? "Disable Characters" : "Enable Characters", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			characterEnabled = !characterEnabled;
			TBufferRenderer tbr = GameObject.FindObjectOfType(typeof(TBufferRenderer)) as TBufferRenderer;
			if (tbr != null)
			{
				if (characterEnabled)
				{
					tbr.camera.cullingMask |= (1 << LayerMask.NameToLayer("CHARACTER"));
				}
				else
				{
					tbr.camera.cullingMask &= ~(1 << LayerMask.NameToLayer("CHARACTER"));
				}
			}
		}
		if (GUILayout.Button(effectEnabled ? "Disable Particles" : "Enable Particles", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			effectEnabled = !effectEnabled;
			GameSettings.Instance.AllowParticleEffect = effectEnabled;
		}
		if (BubbleEffectManager.Instance != null)
		{
			if (GUILayout.Button(BubbleEffectManager.Instance._BubbleEffectEnabled ? "Disable Text" : "Enable Text", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
			{
				BubbleEffectManager.Instance._BubbleEffectEnabled = !BubbleEffectManager.Instance._BubbleEffectEnabled;
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(gbufferEnable ? "Disable GBuffer" : "Enable GBuffer", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			gbufferEnable = !gbufferEnable;
			if (gbuffer == null)
			{
				GBufferRenderer gbr = GameObject.FindObjectOfType(typeof(GBufferRenderer)) as GBufferRenderer;
				gbuffer = gbr.gameObject;
			}
			gbuffer.SetActive(gbufferEnable);
			if (gbufferEnable)
			{
				gbuffer = null;
			}
		}
		if (GUILayout.Button(tbufferEnable ? "Disable TBuffer" : "Enable TBuffer", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			tbufferEnable = !tbufferEnable;
			if (tbuffer == null)
			{
				TBufferRenderer tbr = GameObject.FindObjectOfType(typeof(TBufferRenderer)) as TBufferRenderer;
				tbuffer = tbr.gameObject;
			}
			tbuffer.SetActive(tbufferEnable);
			if (tbufferEnable)
			{
				tbuffer = null;
			}
		}
		if (GUILayout.Button(lbufferEnable ? "Disable LBuffer" : "Enable LBuffer", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			lbufferEnable = !lbufferEnable;
			if (lbuffer == null)
			{
				LBufferRenderer lbr = GameObject.FindObjectOfType(typeof(LBufferRenderer)) as LBufferRenderer;
				lbuffer = lbr.gameObject;
			}
			lbuffer.SetActive(lbufferEnable);
			if (lbufferEnable)
			{
				lbuffer = null;
			}
		}
		if (GUILayout.Button(transCamEnable ? "Disable Transparency" : "Enable Transparency", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			transCamEnable = !transCamEnable;
			if (transCam == null)
			{
				DeferredShadingRenderer dsr = GameObject.FindObjectOfType(typeof(DeferredShadingRenderer)) as DeferredShadingRenderer;
				transCam = dsr.gameObject;
			}
			transCam.camera.enabled = transCamEnable;
			if (transCamEnable)
			{
				transCam = null;
			}
		}
		if (GUILayout.Button(uirendererEnable ? "Disable UIRenderer" : "Enable UIRenderer", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			uirendererEnable = !uirendererEnable;
			if (uirenderer == null)
			{
				CustomUIRenderer uir = GameObject.FindObjectOfType(typeof(CustomUIRenderer)) as CustomUIRenderer;
				uirenderer = uir.gameObject;
			}
			uirenderer.SetActive(uirendererEnable);
			if (uirendererEnable)
			{
				uirenderer = null;
			}
		}
		if (GUILayout.Button(uiEnable ? "Disable UI" : "Enable UI", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			uiEnable = !uiEnable;
			if (CustomUIRenderer._inst != null)
			{
				Camera cam = CustomUIRenderer._inst.gameObject.GetComponent<Camera>();
				if (cam != null)
				{
					int uilayer = (1 << LayerMask.NameToLayer("2DUI"));
					cam.cullingMask = (uiEnable ? uilayer : 0);
				}
			}
			if (uirenderer == null)
			{
				CustomUIRenderer uir = GameObject.FindObjectOfType(typeof(CustomUIRenderer)) as CustomUIRenderer;
				uirenderer = uir.gameObject;
			}
			uirenderer.SetActive(uirendererEnable);
			if (uirendererEnable)
			{
				uirenderer = null;
			}
		}
		GUILayout.EndHorizontal();
	}
	#endregion


	private void ProcessUpgrade()
	{
		ItemInventory item = null;
		GameObject go = GameObject.Find("UI Root (Town)/Camera/Inventory");
		if (go != null)
		{
			item = FCUIInventory.Instance.CurrentSelectionItem;
		}

		if (item != null)
		{
			//if (GUILayout.Button("Upgrade item:\n\nFusion:" + item._fusionLevel.ToString() + "\n Evolution:" + item._evolutionLevel.ToString(), GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
			//{
			//    item.CheatFusionAndEvolution();
			//    //go.GetComponent<UIInventory>().OnShowDetail(go.GetComponent<UIInventory>().CurrentUIItem);//shield by caizilong
			//}
		}
		else
		{
			GUILayout.Button("Upgrade item:\nNo item found", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8));
		}
	}


	private string[] colorNames = new string[5]
	{
		"white","green","blue","purple","glod"	
	};

	private string[] equipNames = new string[8]
	{
		"helm","armpiece","shoulder","leggings","belt","necklace","ring","weapon"	
	};


	private void ProcessAddSuitEquipmments()
	{
		for (int i = 0; i < 9; i++)
		{
			GUILayout.BeginHorizontal();
			for (int k = 0; k < 5; k++)
			{
				if (GUILayout.Button(colorNames[k] + "_" + (i + 1).ToString(), GUILayout.Width(Screen.width / 8), GUILayout.ExpandHeight(true)))
				{
					DoSuitEquipment(k, i);
				}
			}
			GUILayout.EndHorizontal();
		}
	}

	public void DoSuitEquipment(int color, int tie)
	{
		List<string> ids = new List<string>();
		if (color == 3)
		{
			for (int i = 0; i < 8; i++)
			{
				string name = equipNames[i];
				string id = "";
				id += name;
				id += "_";
				id += (tie + 1).ToString();
				id += "_";
				id += colorNames[color];

				if (i < 5)
				{
					string id1 = "all_" + id + "_defensehp";
					ids.Add(id1);
				}
				else if (i >= 5 && i < 7)
				{
					string id1 = "all_" + id + "_criticalcritdamageeng";
					ids.Add(id1);
				}
				else
				{
					id += "_attackcriticalcritdamage";
					string id1 = "mage_" + id;
					string id2 = "warrior_" + id;
					string id3 = "monk_" + id;

					int role = (int)PlayerInfo.Instance.Role;
					if (role == 0)
					{
						ids.Add(id1);
					}
					else if (role == 1)
					{
						ids.Add(id2);
					}
					else
					{
						ids.Add(id3);
					}

				}
			}
			PlayerInfo.Instance.CheatAddItmes(ids);
		}
	}



	private string _curSkillName = "";
	private int _curSkillLevel;
	private int _changeLevel;
	private int _curSkillIndex = 0;

	private void ProcessSkillUpgrade()
	{
		GUILayout.BeginHorizontal();




		int.TryParse(GUILayout.TextField(_changeLevel.ToString(), GUILayout.Width(Screen.width / 8), GUILayout.Width(Screen.width / 8)), out _changeLevel);
		GUILayout.Label("[" + _curSkillName + "]   ->  Level:[" + _curSkillLevel.ToString() + "]");

		if (GUILayout.Button("Change Level", GUILayout.Width(Screen.width / 8), GUILayout.MinHeight(200)))
		{
			if (_curSkillName.Length > 1 && (_changeLevel >= 0 && _changeLevel < 5))
			{
				PlayerInfo.Instance.ChangeSkillLevel(_curSkillName, _changeLevel);
				_curSkillLevel = PlayerInfo.Instance.GetSkillLevel(_curSkillName);

				if (ObjectManager.Instance != null)
				{
					ActionController ac = ObjectManager.Instance.GetMyActionController();
					if (ac != null)
					{
						AttackAgent attackAgent = ac.gameObject.GetComponentInChildren<AttackAgent>();
						attackAgent.InitSkillData(ac.AIUse);
					}
				}
			}
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();


		List<SkillData> usedSkill = DataManager.Instance.GetAllSkill();

		for (int i = 0; i < usedSkill.Count; i++)
		{
			string name = usedSkill[i].skillID;
			if (i == _curSkillIndex)
			{
				GUI.color = Color.green;
				_curSkillName = name;
				_curSkillLevel = PlayerInfo.Instance.GetSkillLevel(_curSkillName);
			}

			if (GUILayout.Button(name, GUILayout.Width(Screen.width / 8), GUILayout.MinHeight(200)))
			{
				_curSkillIndex = i;
			}

			GUI.color = Color.white;
		}

		GUILayout.EndHorizontal();
	}




	private void ProcessTargetFrameRate()
	{
		if (GUILayout.Button("FPS Limit", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			Application.targetFrameRate = _frameRate;
		}
		int.TryParse(GUILayout.TextField(_frameRate.ToString(), GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)), out _frameRate);
	}

	private void ProcessPlayerLevel()
	{
		if (GUILayout.Button("Player\n\nLevel", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			SendCheatCommand(GetPlayerPropCommandString(PlayerPropKey.Level, _playerLevel));
		}

		int.TryParse(GUILayout.TextField(_playerLevel.ToString(), GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)), out _playerLevel);
	}

	private string GetPlayerPropCommandString(PlayerPropKey key, int value)
	{
		return string.Format("player:set_prop(0,{0},{1})", (int)key, value);
	}


	private void ProcessIamCheater()
	{
		if (GUILayout.Button("I\nam\nCheater", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			GameManager._cheatFlag = 1;
		}
	}

	private void ProcessClearPrefs()
	{
		if (GUILayout.Button("Clear\n\nLocal Prefs", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			PlayerPrefs.DeleteAll();
		}
	}

	private void ProcessChangeNickname()
	{
		if (GUILayout.Button("Change nickName", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			PlayerInfo.Instance.changeNickNameCount = 0;
		}
	}

	private string[] _enemyLabelNames = null;

	private int _enemyLabelIndex;

	Vector2 _enemyScrollPos;

	private Rect _enemyWindowRect;   // calculated bounds of the window that holds the scrolling list
	private Rect _enemyListRect;  // calculated dimensions of the scrolling list placed inside the window
	public Vector2 _enemyButtonSize;

	private int _spawnEnemyLevel;	
	private int _spawnEnemyArmor;

	private void ProcessSpawnOneEnemy()
	{
		if (_enemyLabelNames == null) //prepare the label names, just once
		{
			const string assetPath = "Assets/GlobalManagers/Data/Characters/AcDataList.asset";

			AcDataList acDataList = InJoy.AssetBundles.AssetBundles.Load(assetPath) as AcDataList;

			Assertion.Check(acDataList != null);

			List<AcData> list = acDataList._dataList;

			_enemyLabelNames = new string[list.Count];

            int index = 0;

            foreach (AcData data in list)
            {
                if (data.eliteType != FC_ELITE_TYPE.hero && data.nameIds != "treasure")
                {
                    _enemyLabelNames[index] = data.id;
                    index++;
                }
            }
		}
		/*
		_enemyScrollPos = GUILayout.BeginScrollView(_enemyScrollPos, GUILayout.Width(Screen.width / 4), GUILayout.Height(Screen.height * 3 / 5));
		_enemyLabelIndex = GUILayout.SelectionGrid(_enemyLabelIndex, _enemyLabelNames, 1);
		GUILayout.EndScrollView();
		*/

		GUILayout.Space(Screen.width / 2);
		
		GUILayout.BeginVertical();
		//_itemWindowRect = new Rect(10, 10, Screen.width - 100, Screen.height - 100);
		_enemyWindowRect = new Rect(Screen.width / 2 - (_cheatMenuWidth / 2) + 10,
		                           Screen.height - _cheatMenuHeight + 70 + 10,
		                           Screen.width / 2, Screen.height * 3 / 5);
		
		_enemyListRect = new Rect(10, 20, _enemyWindowRect.width - 20, _enemyWindowRect.height - 30);
		
		_enemyButtonSize = new Vector2(_enemyListRect.width - 20, 35);
		
		GUI.Window(0, _enemyWindowRect, DoEnemyWindow, "Select Enemy");
		GUILayout.EndVertical();

		
		//spawn enemy level
		GUILayout.BeginVertical();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Enemy level", GUILayout.Width(100), GUILayout.Height(30));
		Int32.TryParse(GUILayout.TextField(_spawnEnemyLevel.ToString(), GUILayout.Width(100), GUILayout.Height(30)), out _spawnEnemyLevel);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Enemy armor", GUILayout.Width(100), GUILayout.Height(30));
		Int32.TryParse(GUILayout.TextField(_spawnEnemyArmor.ToString(), GUILayout.Width(100), GUILayout.Height(30)), out _spawnEnemyArmor);
		GUILayout.EndHorizontal();

		if (GUILayout.Button("Spawn One Enemy", GUILayout.Width(200), GUILayout.Height(80)))
		{
			if (LevelManager.Singleton != null)
			{
				LevelManager.Singleton.CheatSpawnOneEnemy(_enemyLabelNames[_enemyLabelIndex], _spawnEnemyLevel, _spawnEnemyArmor);
			}
		}
		GUILayout.EndVertical();
	}

	void DoEnemyWindow(int windowID)
	{
		_enemyScrollPos = GUI.BeginScrollView(_enemyListRect, _enemyScrollPos, new Rect( 0, 0, _enemyButtonSize.x, _enemyLabelNames.Count() * _enemyButtonSize.y ), false, false);
		Rect rBtn = new Rect(0, 0, _enemyButtonSize.x, _enemyButtonSize.y);
		
		GUIStyle myStyle = new GUIStyle(GUI.skin.button);
		myStyle.fontSize = 16;
		myStyle.fixedHeight = 32;
		
		for (int iRow = 0; iRow < _enemyLabelNames.Count(); iRow++)
		{
			// draw call optimization: don't actually draw the row if it is not visible
			if (rBtn.yMax >= _enemyScrollPos.y &&
			    rBtn.yMin <= (_enemyScrollPos.y + _enemyListRect.height))
			{
				bool fClicked = false;
				string rowLabel = _enemyLabelNames[iRow];
				
				if (iRow == _enemyLabelIndex)
				{
					//fClicked = GUI.Button(rBtn, rowLabel, rowSelectedStyle);
					GUI.color = Color.cyan;
				}
				else
				{
					GUI.color = Color.white;
				}
				fClicked = GUI.Button(rBtn, rowLabel, myStyle);
				
				// Allow mouse selection, if not running on iPhone.
				if (fClicked && Application.platform != RuntimePlatform.IPhonePlayer)
				{
					//Debug.Log("Player mouse-clicked on row " + iRow);
					_enemyLabelIndex = iRow;
				}
			}
			
			rBtn.y += _enemyButtonSize.y;
		}
		GUI.EndScrollView();
	}
	
	private Vector2 _scrollPositionEnemy = Vector2.zero;
	
	private void ProcessEnemyType()
	{
		_scrollPositionEnemy = GUILayout.BeginScrollView(_scrollPositionEnemy, GUILayout.Width(Screen.width / 8), GUILayout.Width(Screen.width / 8));
		GUILayout.BeginHorizontal();
		//todo
		/*
		if (CombatController.Instance != null)
		{
			foreach (Enemy enemy in CombatController.Instance._enemyList._allEnemys)
			{
				if (GUILayout.Button(enemy.Id, GUILayout.Width(Screen.width / 8), GUILayout.MaxHeight(200)))
				{
					_spawnEnemyId = enemy.Id;
				}
			}
		}//*/

		GUILayout.EndHorizontal();
		GUILayout.EndScrollView();
	}

	private void ProcessSoftCurrency()
	{
		if (GUILayout.Button("Add\nSoft Currency", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			SendCheatCommand(string.Format("player:inc_sc(0, {0})", _softCurrencyIncrement));
		}


		float.TryParse(GUILayout.TextField(_softCurrencyIncrement.ToString(), GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)), out _softCurrencyIncrement);
	}

	private float _timeScale = 1;

	private void ProcessRabbitDistance()
	{
		if (GUILayout.Button("Time\nScale", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			GameSettings.Instance.TimeScale = _timeScale;
		}
		float.TryParse(GUILayout.TextField(_timeScale.ToString(), GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)), out _timeScale);
	}

	private string _cameraParam;
	private void ProcessCameraParam()
	{
		if (CameraController.Instance == null) return;

		FC2Camera c = CameraController.Instance.CurrentCamera;

		if (string.IsNullOrEmpty(_cameraParam))
		{
			_cameraParam = string.Format("{0:f2};{1:f2};{2:f2};{3:f2}", c.heightOffset, c.translation.x, c.translation.y, c.translation.z);
		}

		if (GUILayout.Button("Set\nCamera Param", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			string[] ss = _cameraParam.Split(';');
			try
			{
				float h = float.Parse(ss[0]);
				float x = float.Parse(ss[1]);
				float y = float.Parse(ss[2]);
				float z = float.Parse(ss[3]);

				c.heightOffset = h;
				c.translation = new Vector3(x, y, z);
			}
			catch (Exception)
			{
			}

		}
		_cameraParam = GUILayout.TextField(_cameraParam, GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8));
	}

	private void ProcessSetHitpoint()
	{
		if (GUILayout.Button("Set\nHit point", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			ObjectManager.Instance.GetMyActionController().Data.HP = (int)_hitPoint;
			ObjectManager.Instance.GetMyActionController().HitPoint = (int)_hitPoint;
		}
		float.TryParse(GUILayout.TextField(_hitPoint.ToString(), GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)), out _hitPoint);
	}

	private void ProcessSetDamagepoint()
	{
		if (GUILayout.Button("Set Damage point", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			ObjectManager.Instance.GetMyActionController().Data.physicalAttack = (int)_damagePoint;
			ObjectManager.Instance.GetMyActionController().Data.pLevelData._attack = 0;
			ObjectManager.Instance.GetMyActionController().BaseAttackPoints[0] = (int)_damagePoint;
		}
		float.TryParse(GUILayout.TextField(_damagePoint.ToString(), GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)), out _damagePoint);
	}

	private void ProcessSetDefensepoint()
	{
		if (GUILayout.Button("Set Defense point", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			ObjectManager.Instance.GetMyActionController().Data.physicalDefense = (int)_defensePoint;
            ObjectManager.Instance.GetMyActionController().Data.pLevelData._defense = 0;
			ObjectManager.Instance.GetMyActionController().BaseDefenseAllData._defensePoints[0] = (int)_defensePoint;
		}
		float.TryParse(GUILayout.TextField(_defensePoint.ToString(), GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)), out _defensePoint);
	}

	private void ProcessHardCurrency()
	{
		if (GUILayout.Button("Add\nHard Currency", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			SendCheatCommand(string.Format("player:inc_hc(0, {0})", _hardCurrencyIncrement));
		}

		float.TryParse(GUILayout.TextField(_hardCurrencyIncrement.ToString(), GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)), out _hardCurrencyIncrement);
	}

	private void ProcessToken()
	{
		if (GUILayout.Button("Add\nToken", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			string[] tokens = FCConst.k_token_names;
			Hashtable ht = new Hashtable();
			foreach (string key in tokens)
			{
				if (_tokenIncrement > 0)
				{
					PlayerInfo.Instance.addGachaToken(key, (int)_tokenIncrement);
				}
				else
				{
					PlayerInfo.Instance.reduceGachaToken(key, -(int)_tokenIncrement);
				}
				ht[key] = PlayerInfo.Instance.getGachaTokenAmount(key);
			}

		}

		float.TryParse(GUILayout.TextField(_tokenIncrement.ToString(), GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)), out _tokenIncrement);
	}

	private string _vitDiff = "120";
	private void ProcessAddVitality()
	{
		if (GUILayout.Button("Add\n\nVitality", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			SendCheatCommand(string.Format("player: inc_vitality(0, {0})", _vitDiff));
		}
		_vitDiff = GUILayout.TextField(_vitDiff, GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8));
	}

	private string _vipLevel = "1";
	private void ProcessSetVipLevel()
	{
		if (GUILayout.Button("Set Vip\n\nLevel", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			SendCheatCommand(GetPlayerPropCommandString(PlayerPropKey.Vip, Int32.Parse(_vipLevel)));
		}
		_vipLevel = GUILayout.TextField(_vipLevel, GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8));
	}

	private void ProcessUpdateIAP()
	{
		if (PlayerInfo.Instance != null)
		{
			if (GUILayout.Button("firstIap:" + PlayerInfo.Instance.firstIAP.ToString(), GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
			{
				if (PlayerInfo.Instance.firstIAP == 0)
				{
					PlayerInfo.Instance.firstIAP = 1;
				}
				else
				{
					PlayerInfo.Instance.firstIAP = 0;
				}
			}
		}


		if (GUILayout.Button("month buy", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			UIMonthCard._cheatType = 0;
			UIMonthCard.PreBuyMonthCard();
		}

		if (GUILayout.Button("month clear", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			UIMonthCard._cheatType = 1;
			UIMonthCard.PreBuyMonthCard();
		}

		if (GUILayout.Button("month request", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			UIMonthCard.CheckMonthCard();
		}
	}

	private void ProcessXp()
	{
		if (GUILayout.Button("Add Xp", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			PlayerInfo.Instance.AddXP((int)_xpIncrement);
		}

		float.TryParse(GUILayout.TextField(_xpIncrement.ToString(), GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)), out _xpIncrement);
	}

	private void ProcessHP()
	{
		if (GameManager.Instance.GameState == EnumGameState.InBattle)
		{
			ActionController player = ObjectManager.Instance.GetMyActionController();
			if (player != null && GUILayout.Button("Reduce\n10% HP", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
			{
				player.ACReduceHP(player.Data.TotalHp / 10, true, false, false, false);
			}
		}
	}

	private void ProcessSuicide()
	{
		if (GameManager.Instance.GameState == EnumGameState.InBattle)
		{
			ActionController player = ObjectManager.Instance.GetMyActionController();
			if (player != null && GUILayout.Button("Suicide", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
			{
				player.ACReduceHP(player.Data.TotalHp, true, false, false, false);
			}
		}
	}

	private void ProcessNoCDTime()
	{
		if (GUILayout.Button("No CD", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			noCDTime = !noCDTime;
			//todo
		}
	}

	private void ProcessCostNoEnergy()
	{
		if (GUILayout.Button("Cost no\nEnergy", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			cheatForCostNoEnergy = !cheatForCostNoEnergy;
			//todo
		}
	}

	private static string[] _panelNames = new string[]
    {
        "cheat 1\n\nFPS & Player",
        "cheat 2\n\nServer",
        "cheat 3\n\nInventory",
        "cheat 4\n\nEnemy spawn",
        "cheat 5\n\nQuests",
        "cheat 6\n\nBattle info",
		"cheat 7\n\nAdd Suit",
		"cheat 8\n\nProfiling",
		"cheat 9\n\nItem Editor",
    };

	private void ProcessCheatPanel()
	{
		for (int i = 0; i < _panelNames.Length; i++)
		{
			if (_cheatPanel == i)
			{
				GUI.color = Color.green;
			}

			if (GUILayout.Button(_panelNames[i], GUILayout.MaxWidth(Screen.width / (_panelNames.Length + 1)), GUILayout.MinHeight(70)))
			{
				_cheatPanel = i;
			}

			GUI.color = Color.white;
		}
	}

	bool _displayMyInfo = false;
	private void ToggleDisplayInfo()
	{
		if (_displayMyInfo)
		{
			if (GUILayout.Button("Info:ON", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
			{
				_displayMyInfo = false;
			}
		}
		else
		{
			if (GUILayout.Button("Info:OFF", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
			{
				_displayMyInfo = true;
			}
		}
	}

	void DisplayInfo()
	{
		if (ObjectManager.Instance == null)
		{
			return;
		}

		ActionController ac = ObjectManager.Instance.GetMyActionController();
		if (ac == null)
		{
			return;
		}

		GUILayout.Space(120);
		string info = "-------------------------------------------------------------------------------------\n";

        info += string.Format("Name: {0}\n", ac.Data.id);
		info += string.Format("Hp: {0} (%{1})\n", ac.HitPoint, ac.HitPointPercents * 100);
		info += string.Format("Mp: {0} (%{1})\n", ac.Energy, ac.EngergyPercents * 100);
        info += string.Format("Xp: {0} (%{1})\n", PlayerInfo.Instance.CurrentXp,PlayerInfo.Instance._currentXpPrecent * 100);// XP
        info += string.Format("Speed: {0}\n", ac.SelfMoveAgent.CurrentSpeed);
        info += string.Format("Physical:    Damage{0},  Defense{1}\n", ac.TotalAttackPoints[0], ac.DefenseAllData._defensePoints[0]);
        info += string.Format("Ice:         Damage{0},  Defense{1}\n", ac.TotalAttackPoints[1], ac.DefenseAllData._defensePoints[1]);
        info += string.Format("Fire:        Damage{0},  Defense{1}\n", ac.TotalAttackPoints[2], ac.DefenseAllData._defensePoints[2]);
        info += string.Format("Linghtting:  Damage{0},  Defense{1}\n", ac.TotalAttackPoints[3], ac.DefenseAllData._defensePoints[3]);
        info += string.Format("Posion:      Damage{0},  Defense{1}\n", ac.TotalAttackPoints[4], ac.DefenseAllData._defensePoints[4]);

		info += "-------------------------------------------------------------------------------------\n";


        List<ActionController> acList = ObjectManager.Instance.GetEnemyActionController();

        foreach (ActionController preAc in acList)
        {
            info += string.Format("Name: {0}\n", preAc.Data.id);
            info += string.Format("Hp: {0}(%{1})\n", preAc.HitPoint, preAc.HitPointPercents * 100);
            info += string.Format("Speed: {0}\n", preAc.Data.TotalMoveSpeed);
            info += string.Format("Physical:    Damage{0},  Defense{1}\n", preAc.TotalAttackPoints[0], preAc.DefenseAllData._defensePoints[0]);
            info += "-------------------------------------------------------------------------------------\n";
        }


		GUILayout.Label(info);

	}

	private void ToggleDisplayMagicAttack()
	{
		if (showAttackInfo)
		{
			if (GUILayout.Button("AttInfo:ON", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
			{
				showAttackInfo = false;
			}
		}
		else
		{
			if (GUILayout.Button("AttInfo:OFF", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
			{
				showAttackInfo = true;
			}
		}
	}

	private void ToggleDisplayDps()
	{
		if (dpsCountEnabled)
		{
			if (GUILayout.Button("Dps:ON", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
			{
				dpsCountEnabled = false;
			}
		}
		else
		{
			if (GUILayout.Button("Dps:OFF", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
			{
				dpsCountEnabled = true;
				dpsInfo = "";
				hitCount = 0;
				hitTime = 0;
				damageTotal = 0;
			}
		}
	}


	private void ProcessDisableBladeSlide()
	{
		if (disableBladeSlide)
		{
			if (GUILayout.Button("BladeSlide: true", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
			{
				disableBladeSlide = false;
				BladeSlide.isEffectOn = true;
			}
		}
		else
		{
			if (GUILayout.Button("BladeSlide: false", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
			{
				disableBladeSlide = true;
				BladeSlide.isEffectOn = false;
			}
		}
	}

	private void ProcessSuperManHalf()
	{
		if (GUILayout.Button("Super Namekian:\n\n" + superManHalf.ToString(), GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			superManHalf = !superManHalf;
			ObjectManager.Instance.GetMyActionController().ACSetCheatFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_NAMEKIAN, superManHalf);
			//if (CombatController.Instance != null)
			//{
			//    CombatController.Instance.CheatSuperManHalf(_superManHalf);
			//}
		}
	}

	private void ProcessSuperMan()
	{
		if (GUILayout.Button("Super SAIYAJIN:\n\n" + superMan.ToString(), GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			superMan = !superMan;
			ObjectManager.Instance.GetMyActionController().ACSetCheatFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN, superMan);
			//if (CombatController.Instance != null)
			//{
			//    CombatController.Instance.CheatSuperMan(_superMan);
			//}
		}
	}

	private void ProcessSuperMan2()
	{
		if (GUILayout.Button("Super SAIYAJIN2:\n\n" + superMan2.ToString(), GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			superMan2 = !superMan2;
			ObjectManager.Instance.GetMyActionController().ACSetCheatFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN2, superMan2);
			//if (CombatController.Instance != null)
			//{
			//    CombatController.Instance.CheatSuperMan2(_superMan2);
			//}
		}
	}

    private void ProcessWeaponCollider()
    {
        if (GUILayout.Button("Weapon Collider:\n\n" + weaponCollider.ToString(), GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
        {
            weaponCollider = !weaponCollider;
        }
    }

	private void ProcessCrash()
	{
		if (GUILayout.Button("*** Crash ! ***", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			Application.Quit();

		}
	}

	private void ProcessUnlockAllLevels()
	{
		if (GUILayout.Button("Unlock\nAll Levels", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			SendCheatCommand("player:unlock_all_levels(0,2)");
		}
	}

	private void ProcessBattleHud()
	{
		if (GUILayout.Button("Show Battle Hud:\n\n" + showBattleHud.ToString(), GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			showBattleHud = !showBattleHud;

			GameObject hudRoot = GameObject.Find("/CommandScene_Root/UI Root (Battle)/Camera/Anchor(Hud)");
			if (hudRoot == null)
			{
				return;
			}
			foreach (UILabel label in hudRoot.GetComponentsInChildren<UILabel>())
			{
				label.enabled = showBattleHud;
			}

			foreach (UITexture texture in hudRoot.GetComponentsInChildren<UITexture>())
			{
				texture.enabled = showBattleHud;
			}

			foreach (UISprite sprite in hudRoot.GetComponentsInChildren<UISprite>())
			{
				sprite.enabled = showBattleHud;
			}
		}
	}

	private string _questID = "1000";
	private void ProcessQuests()
	{
		if (GUILayout.Button("Finish current quest\n\nNeed quest window", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			UIQuest uiQuest = UnityEngine.Object.FindObjectOfType(typeof(UIQuest)) as UIQuest;
			if (uiQuest != null)
			{
				foreach (UIQuestItem item in uiQuest.ItemList)
				{
					if (item.glowEffect.activeInHierarchy)
					{
						foreach (QuestTargetProgress qtp in item.questProgress.target_progress_list)
						{
							qtp.actual_amount = qtp.required_amount;
						}

						item.questProgress.isCompleted = true;

						item.questProgress.dataChanged = true;
						break;
					}
				}

				QuestManager.instance.SaveQuestProgress();
				uiQuest.CheatRefreshItems();
			}
			else
			{
				Debug.LogWarning("Quest window not found.");
			}
		}

		if (GUILayout.Button("Jump to Quest", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			UIQuest uiQuest = UnityEngine.Object.FindObjectOfType(typeof(UIQuest)) as UIQuest;
			if (uiQuest != null)
			{
				SendCheatCommand(string.Format("player:add_quest({0},{1})", 0, _questID));

				uiQuest.CheatRefreshItems();
			}
		}

		_questID = GUILayout.TextField(_questID, GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8));
	}

	private void ProcessCloseCheatMenu()
	{
		GUI.color = Color.green;

		if (GUILayout.Button("Close Cheats", GUILayout.Width(Screen.width / 8), GUILayout.MinHeight(70)))
		{
			CheatMenuVisible = false;
			if (GameObject.Find("UIManager") != null)
			{
				//GameObject.Find("UIManager").GetComponent<UIManager>().blockInput = false;
			}
		}
		GUI.color = Color.white;
	}

	private void ProcessPhotonDebug()
	{
		if (GUILayout.Button("Switch Photon Debug", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			PhotonManager.Instance._enableDebug = !PhotonManager.Instance._enableDebug;
		}
	}

	private void SendCheatCommand(string cmd)
	{
		CheatRequest cheatRequest = new CheatRequest();
		cheatRequest.command = cmd;
		NetworkManager.Instance.SendCommand(cheatRequest, OnCheatCommnad);
	}

	private void ProcessCheatCommand()
	{
		_cheatCommand = GUILayout.TextField(_cheatCommand, GUILayout.Width(Screen.width * 3 / 8), GUILayout.Height(Screen.height / 8));
        _cheatMessage = GUILayout.TextField(_cheatMessage, GUILayout.Width(Screen.width * 3 / 8), GUILayout.Height(Screen.height / 8));
		if (GUILayout.Button("Send CMD", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			SendCheatCommand(_cheatCommand);
		}
	}

	private void ProcessTutorial()
	{

		if (GUILayout.Button("Finish Tutorial", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			TutorialManager.Instance.CheatFinishALlLevelTutorial();
			TutorialTownManager.Instance.CheatFinishAllTownTutorial();
		}

	}

	public void OnCheatCommnad(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			_cheatMessage = ((CheatResponse)msg).response;
			((CheatResponse)msg).UpdateData.Broadcast();
			Debug.Log("Cheat command succeeded.");
		}
		else
		{
			Debug.Log(string.Format("Cheat command failed. Error = {0} Msg = {1}", msg.errorCode, msg.errorMsg));
		}
	}

	private void ProcessCheatMenuVisibility()
	{
		if (!_cheatsMenuVisible)
		{
#if (!UNITY_IPHONE && !UNITY_ANDROID) || UNITY_EDITOR

			if (Input.GetMouseButton(0))
			{
				Vector3 inputPos = Input.mousePosition;
				inputPos.y = Screen.height - inputPos.y;

#else
				
			if( (Input.touches.Length > 0) &&
				((Input.GetTouch(0).phase != TouchPhase.Ended) || 
				 (Input.GetTouch(0).phase != TouchPhase.Canceled)) )
			{
				Vector3 inputPos = Input.GetTouch(0).position;
				inputPos.y = Screen.height - inputPos.y;
						
#endif

				float yPos = 0.0f;
				float xPos = 0.0f;

				Rect rect = new Rect(xPos, yPos, _openCheatMenuAreaWidth, _openCheatMenuAreaHeight);

				if ((inputPos.x > rect.x) &&
					(inputPos.x < rect.x + rect.width) &&
					(inputPos.y > rect.y) &&
					(inputPos.y < rect.y + rect.height))
				{
					_openCheatsMenuTimer += Time.deltaTime;


					if (_openCheatsMenuTimer > _openCheatsMenuTime)
					{
						_openCheatsMenuTimer = 0.0f;

						CheatMenuVisible = true;

						if (GameObject.Find("UIManager") != null)
						{
							//todo GameObject.Find("UIManager").GetComponent<UIManager>().blockInput = true;
						}
					}
				}
			}
			else
			{
				_openCheatsMenuTimer = 0.0f;
			}
		}
	}

	private void ProcessLoadingText()
	{
		if (GUILayout.Button("Show/Hide\n\nLoading Screen", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			if (loadingTipList == null)
			{
				loadingTipList = new List<string>();

				foreach (string curString in FCConst.LevelLoadingTips)
				{
					loadingTipList.Add(curString);
				}

				foreach (LevelData curLevelData in LevelManager.Singleton.LevelsConfig.levels)
				{
					foreach (string curString in FCConst.LevelLoadingTips)
					{
						loadingTipList.Add(curString);
					}
				}
			}
			isShowLoadingInCheat = !isShowLoadingInCheat;
			LoadingManager.Instance._loadingUI.SetActive(isShowLoadingInCheat);
			LoadingManager.Instance._tip.gameObject.SetActive(isShowLoadingInCheat);
			LoadingManager.Instance._tip.text = loadingTipList[_loadingTipIndex];
		}
		if (GUILayout.Button("Next Text", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			_loadingTipIndex = _loadingTipIndex + 1;
			if (_loadingTipIndex > loadingTipList.Count - 1)
			{
				_loadingTipIndex = loadingTipList.Count - 1;
			}

			LoadingManager.Instance._tip.text = Localization.Localize(loadingTipList[_loadingTipIndex]);
		}
		if (GUILayout.Button("Prev Text", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			_loadingTipIndex = _loadingTipIndex - 1;
			if (_loadingTipIndex < 0)
			{
				_loadingTipIndex = 0;
			}
			LoadingManager.Instance._tip.text = Localization.Localize(loadingTipList[_loadingTipIndex]);
		}
	}

	void Update()
	{
		ProcessCheatMenuVisibility();

        UpdateItemWindow();

		UpdateEnemyWindow();

		if (Input.GetKey(KeyCode.Escape))
		{
			CheatMenuVisible = false;
			if (GameObject.Find("UIManager") != null)
			{
				//todo
				//GameObject.Find("UIManager").GetComponent<UIManager>().blockInput = false;
			}
		}
	}

	int fileCounter;

	int _meshCount;
	int _textureCount;
	int _animCount;
	int _allObjectCount;
	bool _isStatWorking;    //if the stat work is still in progress, skip the stat request

	private string LogFilename
	{
		get
		{
			string filePath = Application.dataPath;

			int index = filePath.LastIndexOf('/');

#if UNITY_EDITOR
			filePath = Path.Combine(filePath.Substring(0, index), string.Format("{0:000}.log", fileCounter));
#else
#if UNITY_IPHONE
			filePath = filePath.Substring(0, index);
			
			index = filePath.LastIndexOf('/');
			
			filePath = Path.Combine(filePath.Substring(0, index), string.Format("Documents/{0:000}.log", fileCounter));
#endif
#endif
			return filePath;
		}
	}

	void ReportMemUsage()
	{
		if (!_isStatWorking && GUILayout.Button("Stat", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			StartCoroutine(Stat());
		}

		GUILayout.BeginVertical();

		GUILayout.Label("All objects: " + _allObjectCount, GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 32));
		GUILayout.Label("Mesh: " + _meshCount, GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 32));
		GUILayout.Label("Texture 2D: " + _textureCount, GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 32));
		GUILayout.Label("Anim Clip: " + _animCount, GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 32));

		GUILayout.EndVertical();
	}


	/// <summary>
	/// Stat of all the Unity objects. Report on direct descendents only. Log messages are written to a string list first, then to actual file.
	/// Sub classes are:
	///     AnimationClip
	///     AssetBundle
	///     AudioClip
	///		Component
	///		Flare
	///		Font
	///		GameObject
	///		LightProbes
	///		Material
	///		Mesh
	///		NavMesh
	///		PhysicMaterial
	///		QualitySettings
	///		ScriptableObject
	///		Shader
	///		TerrainData
	///		TextAsset
	///		Texture
	/// </summary>
	/// <returns></returns>
	IEnumerator Stat()
	{
		_isStatWorking = true;

		Debug.Log("Object stat starts...");

		List<string> log = new List<string>();

		UnityEngine.Object[] objs;

		//     AnimationClip
		objs = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.AnimationClip));
		WriteObjNamesToLog(objs, log, typeof(AnimationClip).Name);
		_animCount = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.AnimationClip)).Length;
		objs = null;

		//      AssetBundle
		WriteObjNamesToLog(Resources.FindObjectsOfTypeAll(typeof(UnityEngine.AssetBundle)), log, typeof(AssetBundle).Name);

		//      AudioClip
		WriteObjNamesToLog(Resources.FindObjectsOfTypeAll(typeof(UnityEngine.AudioClip)), log, typeof(AudioClip).Name);

		//      Component
		WriteObjNamesToLog(Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Component)), log, typeof(Component).Name);
		yield return null;

		//      Flare
		WriteObjNamesToLog(Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Flare)), log, typeof(Flare).Name);

		//      Font
		WriteObjNamesToLog(Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Font)), log, typeof(Font).Name);

		//		GameObject
		WriteObjNamesToLog(Resources.FindObjectsOfTypeAll(typeof(UnityEngine.GameObject)), log, typeof(GameObject).Name);
		yield return null;

		//		LightProbes
		WriteObjNamesToLog(Resources.FindObjectsOfTypeAll(typeof(UnityEngine.LightProbes)), log, typeof(LightProbes).Name);

		//		Material
		WriteObjNamesToLog(Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Material)), log, typeof(Material).Name);
		yield return null;

		//		NavMesh
		WriteObjNamesToLog(Resources.FindObjectsOfTypeAll(typeof(UnityEngine.NavMesh)), log, typeof(NavMesh).Name);

		//		PhysicMaterial
		WriteObjNamesToLog(Resources.FindObjectsOfTypeAll(typeof(UnityEngine.PhysicMaterial)), log, typeof(PhysicMaterial).Name);

		//		QualitySettings
		WriteObjNamesToLog(Resources.FindObjectsOfTypeAll(typeof(UnityEngine.QualitySettings)), log, typeof(QualitySettings).Name);

		//		ScriptableObject
		WriteObjNamesToLog(Resources.FindObjectsOfTypeAll(typeof(UnityEngine.ScriptableObject)), log, typeof(ScriptableObject).Name);
		yield return null;

		//		Shader
		WriteObjNamesToLog(Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Shader)), log, typeof(Shader).Name);

		//		TerrainData
		WriteObjNamesToLog(Resources.FindObjectsOfTypeAll(typeof(UnityEngine.TerrainData)), log, typeof(TerrainData).Name);

		//		TextAsset
		WriteObjNamesToLog(Resources.FindObjectsOfTypeAll(typeof(UnityEngine.TextAsset)), log, typeof(TextAsset).Name);

		//      Textures
		objs = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Texture)) as Texture[];
		WriteObjNamesToLog(objs, log, typeof(Texture).Name);
		_textureCount = objs.Length;
		objs = null;

		yield return null;

		//      Meshes. Need to get the name of the gameobject that is using the mesh
		yield return StartCoroutine(WriteMeshLog(log));

		//all objects
		_allObjectCount = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Object)).Length;

		yield return null;

		//file header
		int count = 0;
		log.Insert(count++, "================ Start of object stat ============== No. " + fileCounter);

		log.Insert(count++, "All objects count: " + _allObjectCount);
		log.Insert(count++, "Mesh count: " + _meshCount);
		log.Insert(count++, "Texture count: " + _textureCount);
		log.Insert(count++, "Anim clip count: " + _animCount);

		log.Add("================ end of object stat ==============");

		//actually write to file now
		try
		{
			string filePath = LogFilename;

			Debug.Log("Log will be written to file " + filePath);

			System.IO.StreamWriter file = new System.IO.StreamWriter(filePath);

			foreach (string s in log)
				file.WriteLine(s);

			file.Close();

			Debug.Log("Log file writing OK.");

			fileCounter++;
		}
		catch (Exception e)
		{
			System.Console.WriteLine("Wrting log failed: " + e.Message);
		}

		_isStatWorking = false;

	}


	private void WriteObjNamesToLog(UnityEngine.Object[] objs, List<string> logList, string typeName)
	{
		List<string> nameList = new List<string>();
		foreach (UnityEngine.Object obj in objs)
		{
			if (obj != null)
			{
				nameList.Add(typeName + ": " + obj.name);
			}
		}
		nameList.Sort();

		logList.Add(string.Format("==== Type: {0}\t\tCount: {1} ====", typeName, nameList.Count));
		foreach (string s in nameList)
		{
			logList.Add(s);
		}
	}

	private IEnumerator WriteMeshLog(List<string> logList)
	{
		Mesh[] meshes = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Mesh)) as Mesh[];
#if Find_Mesh_Related_GO
        MeshFilter[] mfs = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.MeshFilter)) as MeshFilter[];
#endif
		_meshCount = meshes.Length;

		int skipCount = 0;
		List<string> nameList = new List<string>();

		foreach (Mesh m in meshes)
		{
			if (m == null || !m.isReadable || m.triangles == null)
			{
				skipCount++;
				continue;
			}

			string name = string.Format("Mesh name: {0,-30}\tTriangles: {1,6:d0}", m.name, m.triangles.Length / 3);

			//find the mesh filter that uses the mesh
#if Find_Mesh_Related_GO
            IEnumerable<UnityEngine.MeshFilter> meshHolders = from mf in mfs where mf.sharedMesh == m select mf;

            foreach (MeshFilter mf in meshHolders)
            {
                if (mf != null)
                {
                    name += "\t\tGame object: " + mf.gameObject.name;
                    break;
                }
            }

            nameList.Add(name);
            yield return null;
#else
			nameList.Add(name);
#endif
		}

		logList.Add(string.Format("==== Type: Mesh\t\tCount: {0}, Null meshes skipped: {1} ====", nameList.Count, skipCount));

		nameList.Sort();
		foreach (string s in nameList) logList.Add(s);

		yield return null;
	}
	private void ProcessServerCheat()
	{
		if (GUILayout.Button("Delete\nCurrent\nAccount", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 8)))
		{
			SendCheatCommand(string.Format("account:remove({0})", NetworkManager.Instance.Account));
		}
	}
#endif //DEVELOPMENT_BUILD || UNITY_EDITOR
	#endregion //NOT in release version
}

