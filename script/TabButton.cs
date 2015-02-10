using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TabButton: MonoBehaviour {
	
	private static Dictionary<string, List<TabButton>> _groups = new Dictionary<string, List<TabButton>>();
	
	public GameObject[] _tabs;
	public string _group;
	public int _defaultTab = 0;
	
	private int _selectTab = 0;
	private List<TabButton> _groupList = null;
	
	public int SelectedTab
	{
		get {return _selectTab;}
	}
	
	void Awake()
	{
		_selectTab = _defaultTab;
		if(!string.IsNullOrEmpty(_group))
		{
			if(!_groups.ContainsKey(_group))
			{
				_groups.Add(_group, new List<TabButton>());
			}
			_groupList = _groups[_group];
		 	this.addToList();
		}
	}
	
	void OnDestroy()
	{
		if(!string.IsNullOrEmpty(_group))
		{
			this.removeFromList();
		}
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void onSwitchTo(int tab)
	{
		_selectTab = tab;
		foreach(GameObject go in _tabs)
		{
			go.SetActive(false);
		}
		_tabs[_selectTab].SetActive(true);
	}
	
	public void onSwitch()
	{
		_selectTab = _selectTab == 0? 1: 0;
		onSwitchTo(_selectTab);
		if(_groupList != null)
		{
			int other = _selectTab == 0? 1: 0;
			foreach(TabButton tb in _groupList)
			{
				if(tb != this)
				{
					tb.onSwitchTo(other);
				}
			}
		}
	}
	
	private void addToList()
	{
		foreach(TabButton tb in _groupList)
		{
			if(tb == this)
			{
				return;
			}
		}
		_groupList.Add(this);
	}
	
	private void removeFromList()
	{
		_groupList.Remove(this);
	}
}
