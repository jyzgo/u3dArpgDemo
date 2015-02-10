using UnityEngine;
using System.Collections;

public class HUDMemberHPController : MonoBehaviour 
{
	// Portrait
	public UITexture _portrait;
	
	public UISlider _HP;
	
	// We need map from [0.0,1.0] to [_HPStart,_HPEnd], because we only use a arc as blood view. 
	public float _HPStart;
	public float _HPEnd;
	
	ActionController _ac;
	
    int _playerIndex = 0;
	public int PlayerIndex
	{
		set
		{
			_playerIndex = value;
		}
		get
		{
			return _playerIndex;
		}
	}
	
	// Use this for initialization
	void Start () 
	{
		_HP.sliderValue = _HPEnd;
	}
	
	void OnDestroy()
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (_ac == null)
		{
			OBJECT_ID objectID = ObjectManager.Instance.GetObjectByNetworkID(_playerIndex+FCConst.k_network_id_hero_start);
			if (objectID != null)
			{
				_ac = objectID.fcObj as ActionController;
				
				_ac._hpChangeMessage += OnHPChanged;
				
				_portrait.mainTexture = _ac._avatarController._icon;
			}
		}
	}
	
	void OnHPChanged(float deltaPercent)
	{
		_HP.sliderValue = _HPStart + (_ac.HitPointPercents-deltaPercent)*(_HPEnd-_HPStart);
	}
}
