using UnityEngine;
using System.Collections;


/// <summary>
/// Localization font data set.
/// </summary>
public class LocalizationFontDataSet : ScriptableObject
{
	/// <summary>
	/// Font data.
	/// </summary>
	[System.Serializable]
	public class FontData
	{
		public string _name;
		
		public UIFont _font;
	}
	
	/// <summary>
	/// Font data list.
	/// </summary>
	[System.Serializable]
	public class FontDataList
	{
		public FontData[] _fonts;
		
		public UIFont GetFont(string name)
		{
			foreach (FontData fd in _fonts)
			{
				if (name.ToLower() == fd._name.ToLower())
				{
					return fd._font;
				}
			}
			
			return null;
		}
	}
	
	
	public string _langName;
	public string LangName
	{
		get
		{
			return _langName;
		}
	}
	
	public FontDataList _fonts;
	public FontDataList Fonts
	{
		get
		{
			return _fonts;
		}
	}
}
