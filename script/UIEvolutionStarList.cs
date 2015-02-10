using UnityEngine;
using System.Collections;

public class UIEvolutionStarList : MonoBehaviour {
	
	public string _goldStarName;
	public string _silverStarName;
	public UISprite[] _stars;
	public string _effectSoundName;
	public GameObject[] _starEffects;
	
	private int _evolutionLevel = 0;
	private int _curSoundIndex = 0;
	private bool _effectSoundOnlyOne = false;
	private const int MAX_COUNT = 3;
	
	public void Init(int level)
	{
		foreach(SoundClip sc in SoundManager.Instance._soundClipList._soundList)
		{
			if(sc._name == _effectSoundName)
			{
				_effectSoundOnlyOne = sc._onlyOne;
				sc._onlyOne = false;
			}
		}
		_curSoundIndex = 0;
		_evolutionLevel = level;
		for(int i = 0; i < MAX_COUNT; i++)
		{
			if(i < level)
			{
				_stars[i].gameObject.SetActive(true);
				_starEffects[i].SetActive(false);
				_stars[i].spriteName = _goldStarName;
			}
			else
			{
				_stars[i].gameObject.SetActive(false);
			}
		}
		animation.Play();
	}
	
	public void PlayStarSound()
	{
		if(_curSoundIndex < _evolutionLevel)
		{
			_starEffects[_curSoundIndex].SetActive(true);
			SoundManager.Instance.PlaySoundEffect(_effectSoundName);
		}
		_curSoundIndex ++;
		
		if(_curSoundIndex == MAX_COUNT)
		{
			foreach(SoundClip sc in SoundManager.Instance._soundClipList._soundList)
			{
				if(sc._name == _effectSoundName)
				{
					sc._onlyOne = _effectSoundOnlyOne;
				}
			}
		}
	}
}
