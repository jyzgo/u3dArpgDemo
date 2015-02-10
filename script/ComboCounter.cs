using UnityEngine;
using System.Collections;

public class ComboCounter : MonoBehaviour {
	
	public int _maxComboCount;
	public float _counterLife;
	public float _UIShowLife;
	public float _startScale;
	public float _finaleScale;
	
	public float[] _criticalDamageIncreasCTable;
	
	public float[] _criticalDamageIncreasVTable;
	public float[] _criticalChanceIncreasVTable;
	public float[] _powerpointIncreasVTable;
	
	private static ComboCounter _instance = null;
	public static ComboCounter Instance
	{
		get
		{
			if (_instance == null || !_instance)
			{
				_instance = FindObjectOfType(typeof(ComboCounter)) as ComboCounter;
			}

			return _instance;
		}
	}
	
	
	void Awake()
	{
	}
	
	void OnDestroy()
	{
		_instance = null;
	}
	
	public int CurrentCriLevel
	{
		get
		{
			int i =0;
			while(i < _criticalDamageIncreasCTable.Length  && CurrentCount > _criticalDamageIncreasCTable[i] && _criticalDamageIncreasCTable!=null )
			{
				i++;
			}
			if(i >= _criticalDamageIncreasCTable.Length)
			{
				i = _criticalDamageIncreasCTable.Length-1;
			}
			return i;
		}
	}
	public float CurrentCriDamIncrease
	{
		get
		{
			float ccdi = 0;
			int i =0;
			while(i < _criticalDamageIncreasCTable.Length  && CurrentCount > _criticalDamageIncreasCTable[i] && _criticalDamageIncreasCTable!=null )
			{
				ccdi = _criticalDamageIncreasVTable[i];
				i++;
			}
			return ccdi;
		}
	}
	
	public float CurrentCriChaIncrease
	{
		get
		{
			float ccci = 0;
			int i =0;
			while(i < _criticalDamageIncreasCTable.Length && CurrentCount > _criticalDamageIncreasCTable[i] && _criticalDamageIncreasCTable!=null )
			{
				ccci = _criticalChanceIncreasVTable[i];
				i++;
			}
			return ccci;
		}
	}
	
	public float CurrentPowerPointIncrease
	{
		get
		{
			return _powerpointIncreasVTable[CurrentCriLevel];
		}
	}
	
	private int _currentCount;
	private float _currCounterLife;
	private float _currUIShowLife;
	private int _currShowLvl;
	private Transform _comboPanelTransform;
	private Vector3 _comboPanelScale;
	private Transform _comboBackgroundTransform;
	private Vector3 _comboBackgroundScale;
	private bool _showCombo;
	
	public UILabel _comboPanel;
	public UISpriteAnimation _comboBackground;
	
	private UISprite _mySprite;
	
	public int CurrentCount
	{
		set
		{
			_currentCount = Mathf.Clamp(value,0,_maxComboCount);
			if(_currentCount <= 0)
			{
				_currCounterLife = -1;
				_currUIShowLife = -1;
				_showCombo = false;
				ShowCombo(false);
			}
			else
			{
				// disable combo show
				_currCounterLife = _counterLife;
				_currUIShowLife = _UIShowLife;
				_showCombo = true;
			}
		}
		get
		{
			return _currentCount;
		}
	}
	
	private float _loaclScaleOffset = 0.1f;
	
	public void LateUpdate()
	{
		if(_currCounterLife >0)	
		{
			if(_showCombo)
			{
				ShowCombo(true);
			}
			_currCounterLife -= Time.deltaTime;
			if(_currCounterLife <=0)
			{
				CurrentCount = 0;
			}
		}
		if(_currUIShowLife>0)
		{
			_currUIShowLife -= Time.deltaTime;
			if(_currUIShowLife <=0)
			{
				ShowCombo(false);
			}
		}
		if(_currUIShowLife >0)
		{
			if(_startScale < _finaleScale)
			{
				if(_comboPanelTransform.localScale.x < (_comboPanelScale * _finaleScale).x)
				{
					_comboPanelTransform.localScale += _comboPanelScale * _loaclScaleOffset;
					_comboBackgroundTransform.localScale += _comboBackgroundScale * _loaclScaleOffset;
				}
			}
			else
			{
				if(_comboPanelTransform.localScale.x > (_comboPanelScale * _finaleScale).x)
				{
					_comboPanelTransform.localScale -= _comboPanelScale * _loaclScaleOffset;
					_comboBackgroundTransform.localScale -= _comboBackgroundScale * _loaclScaleOffset;
				}
			}
		}
		
		_showCombo = false;
	}
	
	void ShowCombo(bool show)
	{
		if(show)
		{
			_comboPanel.enabled = true;
			_comboPanel.text = CurrentCount.ToString();
			_comboPanel.MakePixelPerfect();
			_comboPanelScale = _comboPanelTransform.localScale;
			_comboPanelTransform.localScale = _comboPanelScale * _startScale;
			
			_mySprite.enabled = true;
			_mySprite.MakePixelPerfect();
			_comboBackgroundScale = _comboBackgroundTransform.localScale;
			_comboBackgroundTransform.localScale = _comboBackgroundScale * _startScale;
		}
		else
		{
			_comboPanel.enabled = false;
			_mySprite.enabled = false;
		}
	}
	
	// Use this for initialization
	void Start () 
	{
		_comboPanelTransform = _comboPanel.transform;
		_comboBackgroundTransform = _comboBackground.transform;
		_mySprite = _comboBackground.GetComponent<UISprite>();
		CurrentCount = 0;
	}

}
