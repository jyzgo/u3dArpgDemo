using UnityEngine;
using System.Collections;


/// <summary>
/// UIHP controller.
/// 
/// UI Controller for 3D character's Head
/// </summary>
public class UIHPController : MonoBehaviour 
{
	public UISlider _bloodBar;
	public UILabel _displayText;
	public UISprite _icon;
	public GameObject _chantBar;
	public UILabel _guildName;
	
	public GameObject _bubbles;
	public UISlider _energyBar;
	
	void Awake()
	{
		_displayText.text = "";
	}
	
	// Use this for initialization
	void Start () 
	{		
		if (_chantBar != null)
		{
			_chantBar.SetActive(false);
		}
		if(_energyBar != null && GameManager.Instance.IsPVPMode)
		{
			_energyBar.gameObject.SetActive(false);
		}
	}
	
	public void SetHPBarVisible(bool val)
	{
		if (_bloodBar != null)
		{
			_bloodBar.gameObject.SetActive(val);
		}
		
		if (_icon != null)
		{
			_icon.gameObject.SetActive(val);
		}		
	}
	
	// Blood
	public float HP
	{
		set
		{
			if (_bloodBar != null)
			{
				_bloodBar.sliderValue = value;
			}
		}
	}
	
	public void ChangeHP(int delta, bool critical, FC_ELITE_TYPE type, bool isFromSkill)
	{
		if (delta > 0)
		{
			BubbleUpHurtEffect(delta.ToString(), critical, type, isFromSkill);
			
			if ((type!=FC_ELITE_TYPE.hero) && (_bloodBar.sliderValue<=0f))
			{
				_bloodBar.gameObject.SetActive(false);
				_displayText.gameObject.SetActive(false);
				if(_icon != null)
				{
					_icon.gameObject.SetActive(false);
				}
			}
		}
	}
	
	// character Name
	public string DisplayText
	{
		set
		{
			_displayText.text = value;
		}
	}
	
	public string GuildName
	{
		set 
		{
			if(_guildName == null)
			{
				return;
			}
			if(string.IsNullOrEmpty(value))
			{
				_guildName.text = "";
			}
			else
			{
				_guildName.text = "<" + value + ">";
			}
		}
	}
	
	public void BubbleUpEffect(string text, float size_factor, Color color, bool symbol)
	{
		BubbleUpEffect(text, "", size_factor, color, symbol);
	}
	
	// Bubble text from head of the character
	public void BubbleUpEffect(string text, string text2, float size_factor, Color color, bool symbol)
	{		
		//BubbleEffectManager.Instance.BubbleUpEffect(text, color, _bubbles.transform.position, size_factor);
		BubbleEffectManager.Instance.BubbleUpDynamicFont(text, text2, _bubbles.transform.position, symbol);
	}
	
	// Hurt effect
	public void BubbleUpHurtEffect(string text, bool critical, FC_ELITE_TYPE type, bool isFromSkill)
	{		
		BubbleEffectManager.Instance.BubbleUpHurtEffect(text, critical, type, isFromSkill, _bubbles.transform.position);
	}
	
	// Chant skill
	public void ChantProgress(float progress)
	{
		if (Mathf.Abs(progress) < Mathf.Epsilon)
		{
			_chantBar.SetActive(true);
		}
		else if (progress > 0.0f)
		{
			UISlider slider = _chantBar.GetComponent<UISlider>();
			slider.sliderValue = progress;
		}
		else if (Mathf.Abs(progress-1.0f) < Mathf.Epsilon)
		{
			UISlider slider = _chantBar.GetComponent<UISlider>();
			slider.sliderValue = progress;
			
			//
			UIFilledSprite sprite = _chantBar.GetComponentInChildren<UIFilledSprite>();
			sprite.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		}
		else if (progress < -0.9f)
		{
			_chantBar.SetActive(false);
		}
	}
	
	public void SetEnergyBarVisible(bool val)
	{
		if (_energyBar != null)
		{
			_energyBar.gameObject.SetActive(val);
		}
	}
	
	// Blood
	public float Energy
	{
		set
		{
			if (_energyBar != null)
			{
				_energyBar.sliderValue = value;
			}
		}
	}
}
