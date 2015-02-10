using UnityEngine;
using System.Collections;

[System.Serializable]
public class SkillTipsInfo
{
	public string _skillToShowTips;
	public int _maxSlot;
	public GameObject _tip;
	public GameObject _pressButton;
	public GameObject _spriteObject;
	public GameObject[] _sprites;
	public int _finishMaxCount = 3;
	public int _currentFinishCount = 0;
	protected bool _isActive = false;
	
	public TweenScale _animationSucc;
	public TweenScale _animationFalse;
	
	public void Init()
	{
		StopAnimation();
		_isActive = false;
	}
	
	public void StopAnimation()
	{
		_animationSucc.Reset();
		_animationSucc.enabled = false;
		_animationFalse.Reset();
		_animationFalse.enabled = false;
	}
	
	public void PlayAnimation()
	{
		if(_sprites[ _sprites.Length-1].activeSelf)
		{
			if(!_animationSucc.enabled)
			{
				_animationSucc.enabled = true;
				_animationSucc.Play(true);
			}
		}
		else
		{
			if(!_animationFalse.enabled)
			{
				_animationFalse.enabled = true;
				_animationFalse.Play(true);
			}
		}
	}
	
	public void SetActive(bool ret)
	{
		if(!ret)
		{
			_isActive = ret;
			_pressButton.SetActive(ret);
			StopAnimation();
			foreach(GameObject gb in _sprites)
			{
				gb.SetActive(ret);
			}
			_spriteObject.SetActive(ret);
		}
		else
		{
			if(_isActive)
			{
				StopAnimation();
			}
			if(_currentFinishCount < _finishMaxCount)
			{
				_pressButton.SetActive(ret);
				_currentFinishCount++;
				_spriteObject.SetActive(ret);
				PlayerPrefs.SetInt(_skillToShowTips, _currentFinishCount);
			}
		}
	}
	
	public void BurnStar(int idx)
	{
		idx = Mathf.Clamp(idx, 0 , _sprites.Length-1);
		_sprites[idx].SetActive(true);
		if(idx == _sprites.Length-1)
		{
			PlayAnimation();
		}
	}
}

public class UIBattleSkillTips : MonoBehaviour {
	
	public SkillTipsInfo[] _skillTipsInfo;
	private static UIBattleSkillTips _instance;
	
	protected int _starIndex = 0;
	protected SkillTipsInfo _currentSti;
	public static UIBattleSkillTips Instance
	{
		get
		{
			return _instance;
		}
	}
	
	void Awake()
	{
		foreach(SkillTipsInfo sti in _skillTipsInfo)
		{
			sti._currentFinishCount = PlayerPrefs.GetInt(sti._skillToShowTips);
			sti.SetActive(false);
		}
		_instance = this;
		_currentSti = null;
	}
	
	void OnDestroy()
	{
		if(_instance == this)
		{
			_instance = null;
		}
	}	
	
	public void ShowTips(string skillName)
	{
		_starIndex = 0;
		CloseTips();
		foreach(SkillTipsInfo sti in _skillTipsInfo)
		{
			if(sti._skillToShowTips == skillName)
			{
				sti.SetActive(true);
				_currentSti = sti;
				//_currentSti.BurnStar(_starIndex);
				break;
			}
		}
	}
	
	public void NextStar()
	{
		if(_currentSti != null)
		{
			_currentSti.BurnStar(_starIndex);
		}
		_starIndex++;
	}
	public void CloseTips()
	{
		foreach(SkillTipsInfo sti in _skillTipsInfo)
		{
			sti.SetActive(false);
		}
		_currentSti = null;
		_starIndex = 0;
	}
	
	public void FinishTips()
	{
		if(_currentSti != null)
		{
			_currentSti.PlayAnimation();
			_currentSti = null;
		}
	}
	
	void OnAnimationOver()
	{
		CloseTips();
	}
}
