using UnityEngine;
using System.Collections;

[AddComponentMenu("NGUI/UI/LabelFont")]
[RequireComponent(typeof(UILabel))]
//[RequireComponent(typeof(UILocalize))]


[System.Serializable]
public class UILabelSize
{
	public string _language = "";
	public float  _characterSizeX = 1f;
	public float  _characterSizeY = 1f;
}

public class UILabelFont : MonoBehaviour 
{
	public string _fontName = "big30";
	
	public UILabelSize[] _multiTextSize;
	
	
	UILabel _label;
	
	void Awake()
	{
		_label = GetComponent<UILabel>();
		
		ApplyMutliTextSize();
	}
	
	// Use this for initialization
	void Start () 
	{
		LocalizationFontDataSet fs = LocalizationContainer.Instance.Locale;
		if (fs == null) return;
		
		UIFont font = fs.Fonts.GetFont(_fontName);
		if (font != null)
		{
			UILabel label = GetComponent<UILabel>();
			label.font = font;
		}
	}
	
	void ApplyMutliTextSize()
	{
		if (_multiTextSize != null)
		{
			string language = LocalizationContainer.CurSystemLang;
			
			foreach (UILabelSize s in _multiTextSize)
			{
				if (s._language == language)
				{
					_label.cachedTransform.localScale = new Vector3(s._characterSizeX, s._characterSizeY, 1f);
					
					break;
				}
			}
		}
	}
}
