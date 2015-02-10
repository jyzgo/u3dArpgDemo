using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class InitItem
{
	public string _itemId;
	public int _count = 1;
}

public class InitInformation : ScriptableObject
{

    public AccountProfile _accountProfile = new AccountProfile();

    public List<string> _defaultEquipment_mage = new List<string>();
    public List<string> _defaultEquipment_warrior = new List<string>();

    public List<string> _loginEquipment_mage = new List<string>();
    public List<string> _loginEquipment_warrior = new List<string>();


    public List<InitItem> _defaultItems_mage = new List<InitItem>();
    public List<InitItem> _defaultItems_warrior = new List<InitItem>();
}
