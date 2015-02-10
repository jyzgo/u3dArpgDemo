using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class UIGlobalSettings : MonoBehaviour 
{
	public Color[] _colors;
	public string[] _iconBackground;
	public string[] _consumableBgSprites;


    public static Dictionary<ItemQuality, string> QualityNamesMap = new Dictionary<ItemQuality, string>
    { 
        {ItemQuality.white, "71"},
        {ItemQuality.green, "72"},
        {ItemQuality.blue, "73"},
        {ItemQuality.purple, "74"},
        {ItemQuality.golden, "75"}
    };

	public static string[] _colorSprites = new string[5]{"White_item", "Green_item", "Blue_item", "Purple_item", "Glod_item"};
	
	static private UIGlobalSettings _instance;
	static public UIGlobalSettings Instance
	{
		get 
		{
			return _instance;
		}
	}
	
	void OnDestroy() 
	{
		if (_instance == this) 
		{
			_instance = null;
		}
	}
	
	void Awake () 
	{		
		_instance = this;
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	public Color GetColorByEnum(int index)
	{
		return _colors[index];
	}
}
