using UnityEngine;
using System.Collections;

public class UIRewardItemHandler : MonoBehaviour 
{
	public NGUICoverFlow _coverFlow;
	public int _itemIndex;
	
	void Awake()
	{
		_itemIndex = 0;
	}
	
	// Use this for initialization
	void Start () 
	{
	}
	
	void OnClick() 
	{
		_coverFlow.MoveSlider(_itemIndex);
	}
}
