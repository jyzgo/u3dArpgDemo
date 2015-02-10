using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class FCAnimationInfoDetails
{
	public string _animationName;
	public int _nameHashCode;
	public float _speed;
	public bool _haveJumpCondition = false;
	public int _jumpCondition = -1;
	public string _uidString = "";
    public bool _canUseHaste = false;    //true if an animation will be speeded up by the haste stat
}

[System.Serializable]
public class FCAnimationInfo : ScriptableObject {
	
	public int z =0;

	public List<FCAnimationInfoDetails> _animationInfo;
	static public int SortByName (FCAnimationInfoDetails a, FCAnimationInfoDetails b) { return string.Compare(a._animationName, b._animationName); }
	
	public void RefreshList()
	{
		_animationInfo.Sort(SortByName);
	}
	
}
