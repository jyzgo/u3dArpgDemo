using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AgentKeyControl : MonoBehaviour,FCAgent {
	
	public class KeyAndAction
	{
		public bool _keyPress = false;
		public int _skillIdx = -1;
		public int _curPriority = -1;
		public int _orgPriority = 0;
		public int[] _energyCost = null;
		public bool _beActive = false;
		// _beEnable > _beActive, _beActive is used for energy and cd system, beEnalbe used to enable or disable key
		public bool _beEnable = true;
	}
	protected AIAgent _owner;
	
	protected KeyAndAction[] _keyCache = new KeyAndAction[FCConst.FC_KEY_MAX];
	
	
	protected bool _keyMapIsInit = false;
	protected FC_KEY_BIND _activeKey = FC_KEY_BIND.NONE;
	
	public Vector3 _directionWanted;
	
	public FC_KEY_BIND ActiveKey
	{
		get
		{
			return _activeKey;
		}
	}
	
	public bool KeyMapIsInit
	{
		get
		{
			return _keyMapIsInit;
		}
	}
	
	public void Init(FCObject owner)
	{
		_owner  = owner as AIAgent;
		for(int i =0; i< _keyCache.Length;i++)
		{
			_keyCache[i] = new KeyAndAction();
			_keyCache[i]._beActive = true;
			_keyCache[i]._beEnable = true;
		}
	}
	
	public void InitKeyMap(FCSkillConfig[] escs)
	{
		if(escs != null && escs.Length>0)
		{
			//for attack key
			for(int i =0; i< escs.Length ; i++)
			{
				if(escs[i].KeyBind >0)
				{
					_keyCache[escs[i].KeyBind]._skillIdx = i;
					_keyCache[escs[i].KeyBind]._orgPriority = escs[i].SkillModule._priority;
					_keyCache[escs[i].KeyBind]._curPriority = -1;
					FCAttackConfig[] eac = escs[i].SkillModule._attackConfigs;
					_keyCache[escs[i].KeyBind]._energyCost = new int[eac.Length];
					for(int j =0;j<_keyCache[escs[i].KeyBind]._energyCost.Length;j++)
					{
						_keyCache[escs[i].KeyBind]._energyCost[j] = eac[j].AttackModule._energyCost;
					}
				}
			}
		}
		//for direction key
		_keyCache[0]._skillIdx =-1;
		_keyCache[0]._orgPriority = 1;
		_keyCache[0]._curPriority = -1;
		_directionWanted = _owner.ACOwner.ThisTransform.forward;
	}
	
	/*public bool GetKeyEnabled(int attackCombo,AttackBase ab)
	{
		return (_keyCache[(int)ab.CurrentBindKey]._energyCost[attackCombo]<=_owner.ACOwner.Energy || _keyCache[(int)ab.CurrentBindKey]._energyCost[attackCombo] == 0);
	}*/
	
	public void ClearKeyPress(int keyIndex)
	{
		if(keyIndex < 0)
		{
			for(int i = 0; i< FCConst.FC_KEY_FOR_TOUCH; i++)
			{
				if(_keyCache[i]._keyPress && _keyCache[i]._beEnable)
				{
					_keyCache[i]._keyPress = false;	
				}
			}
		}
		else
		{
			if(_keyCache[keyIndex]._keyPress && _keyCache[keyIndex]._beEnable)
			{
				_keyCache[keyIndex]._keyPress = false;	
			}	
		}
	}
	
	
	public void SetKeyState(FC_KEY_BIND ekb,bool enableKey)
	{
		if(ekb != FC_KEY_BIND.NONE)
		{
			if(ekb == FC_KEY_BIND.MAX)
			{
				_keyCache[0]._beEnable = enableKey;
				ClearKeyPress(0);
				if(_keyCache != null && _keyCache.Length >0)
				{
					for(int i = 1; i< FCConst.FC_KEY_FOR_TOUCH; i++)
					{
						_owner.ACOwner._enableInputKey((FC_KEY_BIND)i, enableKey);
						_keyCache[i]._beEnable = enableKey;
						ClearKeyPress(i);
					}
				}

			}
			else
			{
				_keyCache[(int)ekb]._beEnable = enableKey;
				ClearKeyPress((int)ekb);
				if(ekb > FC_KEY_BIND.DIRECTION)
				{
					_owner.ACOwner._enableInputKey(ekb, enableKey);
				}
			}
			
		}
		RefreshActiveKey();
	}
	public void SetToHurtState(bool beIn)
	{
		if(_keyCache != null && _keyCache.Length >0)
		{
			for(int i =1; i< FCConst.FC_KEY_FOR_TOUCH; i++)
			{
				int v = 0;
				if(!beIn)
				{
					//enable key for skill
					if( _keyCache[i]._energyCost != null &&( _keyCache[i]._energyCost[v] <= _owner.ACOwner.Energy
					|| _keyCache[i]._energyCost[v] <=0) && _owner.AttackCountAgent.SkillIsCD(_keyCache[i]._skillIdx))
					{
						_owner.ACOwner._enableInputKey((FC_KEY_BIND)i, true);
					}
				}
				else
				{
					_owner.ACOwner._enableInputKey((FC_KEY_BIND)i, false);
				}
			}
		}
	}
	//if skill in cd,we should disable key press for player
	public void SetKeyEnabledBySkillID(int skillIdx, bool toEnabled)
	{
		if(_keyCache != null && _keyCache.Length >0)
		{
			for(int i =1; i< FCConst.FC_KEY_FOR_TOUCH; i++)
			{
				int v = 0;
				if(_keyCache[i]._skillIdx == skillIdx && _keyCache[i]._beEnable)
				{
					if(toEnabled)
					{
						//enable key for skill
						
						if( (_keyCache[i]._energyCost != null
						&& _keyCache[i]._energyCost[v] <= _owner.ACOwner.Energy)
						|| _keyCache[i]._energyCost[v] <=0)
						{
							_keyCache[i]._beActive = true;
							_owner.ACOwner._enableInputKey((FC_KEY_BIND)i, true);
							if(_keyCache[i]._keyPress && i > 0)
							{
								_owner.HandleKeyPress(i, Vector3.zero);
							}
						}
					}
					else
					{
						_keyCache[i]._beActive = false;
						_owner.ACOwner._enableInputKey((FC_KEY_BIND)i, false);
					}
					break;
				}
			}
		}
	}
	public void UpdateKeyState()
	{
		UpdateKeyState(false);
	}
	public void UpdateKeyState(bool forceEnd)
	{
		if(_keyCache != null && _keyCache.Length >0)
		{
			for(int i =1; i< FCConst.FC_KEY_FOR_TOUCH; i++)
			{
				int v = 0;
				if(_owner.CurrentAttack != null && _owner.CurrentAttack.CurrentBindKey == (FC_KEY_BIND)i)
				{
					v = _owner.NextCurSkillComboID;

				}
				if(forceEnd)
				{
					v = 0;
				}
				
				{
					if( _keyCache[i]._energyCost != null
					&& _keyCache[i]._energyCost[v] > _owner.ACOwner.Energy
					&& _keyCache[i]._energyCost[v] >0
					&& _keyCache[i]._skillIdx != -1)
					{
						_keyCache[i]._beActive = false;
						_owner.ACOwner._enableInputKey((FC_KEY_BIND)i,false);
						
						//disable key press
					}
					else
					{
						if(_keyCache[i]._skillIdx != -1 && _owner.ACOwner._enableInputKey != null && _owner.AttackCountAgent.SkillIsCD(_keyCache[i]._skillIdx))
						{
							if(_keyCache[i]._beEnable)
							{
								_keyCache[i]._beActive = true;
								_owner.ACOwner._enableInputKey((FC_KEY_BIND)i,true);
							}
						}
					}
				}
				
			}
		}
	}
	
	//update every frame ,and told ui ,if a skill is in cd, the cd time last
	public void UpdateKeyAndSkillCDState()
	{
		if(_owner.ACOwner.IsPlayerSelf)
		{
			if(_keyCache != null && _keyCache.Length >0)
			{
				for(int i =1; i< FCConst.FC_KEY_FOR_TOUCH;i++)
				{
					if(_keyCache[i]._beEnable
						&& _keyCache[i]._skillIdx != -1 
						&& _owner.ACOwner._updateInputKeyState != null 
						&& !_owner.AttackCountAgent.SkillIsCD(_keyCache[i]._skillIdx))
					{
						_owner.ACOwner._updateInputKeyState((FC_KEY_BIND)i, 
							_owner.AttackCountAgent.GetSkillCDTimeLast(_keyCache[i]._skillIdx),
							_owner.AttackCountAgent.GetSkillCDTimeLastPercent(_keyCache[i]._skillIdx));
					}
				}
			}
		}
	}
	
	public FC_KEY_BIND GetAttackKeyBind(int attackID)
	{
		if(attackID >=0)
		{
			for(int i = 0; i< _keyCache.Length ; i++)
			{
				if(_keyCache[i]._skillIdx == attackID)
				{
					return (FC_KEY_BIND)i;
				}
			}
		}
		return FC_KEY_BIND.NONE;
	}
	
	public void RefreshActiveKey()
	{
		int key = -1;
		int keyP = 0;
		int i = 0;
		for(; i< _keyCache.Length ; i++)
		{
			if(_keyCache[i]._beActive 
				&& _keyCache[i]._beEnable
				&& _keyCache[i]._keyPress == true
				&& _keyCache[i]._curPriority > keyP)
			{
				key = i;
				keyP = _keyCache[i]._curPriority;
			}
		}
		_activeKey = (FC_KEY_BIND)key;
	}
	
	public int GetNextAttackID()
	{
		if(_activeKey != FC_KEY_BIND.NONE)
		{
			return _keyCache[(int)_activeKey]._skillIdx;
		}
		return -1;
	}
	public void PressKey(FC_KEY_BIND keyb)
	{
		int keyValue = (int)keyb;
		
		if(keyValue == 0 ||  (_keyCache[keyValue]._skillIdx >=0 
			&& _owner.AttackCountAgent.SkillIsCD(_keyCache[keyValue]._skillIdx)))
		{
			_keyCache[keyValue]._keyPress = true;
			if(_keyCache[keyValue]._curPriority < _keyCache[keyValue]._orgPriority)
			{
				_keyCache[keyValue]._curPriority = _keyCache[keyValue]._orgPriority;
			}
			RefreshActiveKey();
		}
	}
	
	public bool KeyIsEnabled(int keyIdx)
	{
		if(keyIdx >=0 && keyIdx <= _keyCache.Length)
		{
			return _keyCache[keyIdx]._beEnable;
		}
		return false;
			
	}
	public void ReleaseKey(FC_KEY_BIND keyb)
	{
		int keyValue = (int)keyb;
		_keyCache[keyValue]._keyPress = false;
		_keyCache[keyValue]._curPriority = -1;
		RefreshActiveKey();
	}
	
	public int CompareTwoSkillPriority(FC_KEY_BIND keya, FC_KEY_BIND keyb)
	{
		int keyValuea = (int)keya;
		int keyValueb = (int)keyb;
		if(keyValuea >=0 && keyValueb >=0)
		{
			return _keyCache[keyValuea]._orgPriority - _keyCache[keyValueb]._orgPriority;
		}
		return 0;
	}
	public bool keyIsPress(FC_KEY_BIND keyb)
	{
		int keyValue = (int)keyb;
		if (keyValue < 0)
		{
			return false;
		}
		return _keyCache[keyValue]._keyPress && _keyCache[keyValue]._beActive;
		
	}
}
