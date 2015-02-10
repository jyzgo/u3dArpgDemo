using UnityEngine;
using System.Collections;

public class LocalizationContainer : MonoBehaviour 
{
	private static LocalizationContainer _instance;
	public static LocalizationContainer Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType(typeof(LocalizationContainer)) as LocalizationContainer;
				
				if (_instance != null)
				{
				}
			}
			
			return _instance;
		}
	}
	
	public LocalizationFontDataSet[] _languageFonts;
	
	private LocalizationFontDataSet _locale;
	public LocalizationFontDataSet Locale
	{
		get
		{
			// We don't need to switch language in game.
			if (_locale == null)
			{
				string lang = Localization.instance.currentLanguage;
				
				foreach (LocalizationFontDataSet fs in _languageFonts)
				{
					if (lang != null && fs.LangName.ToLower() == lang.ToLower())
					{
						_locale = fs;
					}
				}
			}
			
			return _locale;
		}
	}
	
	void Awake()
	{
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public static string CurSystemLang
	{
		get
		{
			string language = LocalizationInfo.GetLanguage();
			
			if(language.ToUpper().Equals("zh-Hans".ToUpper()))
			{
				language = "zh-Hans";
			}
			else if(language.ToUpper().Equals("zh-Hant".ToUpper()))
			{
				language = "zh-Hant";
			}
			else if(language.ToUpper().Equals("ja".ToUpper()))
			{
				language = "ja";
			}
			
			else if(language.ToUpper().Equals("ko".ToUpper()))
			{
				language = "ko";
			}
			else if(language.ToUpper().Equals("th".ToUpper()))
			{
				language = "th";
			}
			else if(language.ToUpper().Equals("de".ToUpper()))
			{
				language = "de";
			}
			else if(language.ToUpper().Equals("fr".ToUpper()))
			{
				language = "fr";
			}
			else if(language.ToUpper().Equals("pt".ToUpper()))
			{
				language = "br";
			}
			else if(language.ToUpper().Equals("ru".ToUpper()))
			{
				language = "ru";
			}
			else if(language.ToUpper().Equals("es".ToUpper()))
			{
				language = "es";
			}
//			else if(language.ToUpper().Equals("it".ToUpper()))
//			{
//				language = "it";
//			}
			else
			{
				language = "en";
			}
			
		
			language = "zh-Hans";
			//language = "zh-Hant";			
			//language = "ja";
			//language = "ko";
			//language = "th";
			//language = "de";
			//language = "fr";
			//language = "br";
			//language = "ru";
			//language = "es";
			//language = "it";
			return language;
		}
	}
}
