using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UIUtils : MonoBehaviour {
	
	
	
	public static int GetScreenWidth()
	{
		int screenW = (int)(Screen.width * 1.0/ Screen.height * 640);
		return 	screenW;
	}
	
	
	public static void AutoFitScreen(UIPanel _panel, UIGrid _uiGrid, int count)
	{
		_panel.transform.localPosition = new Vector3(0,0,0);
		
		int screenW = UIUtils.GetScreenWidth();
		int w = screenW;
		
		int maxW = (int)(count * _uiGrid.cellWidth);
		if( w > maxW)
		{
			w = maxW;
		}
		
		_panel.clipRange = new Vector4(0,0,w,640);
		float xOffset  = -(w/2 - _uiGrid.cellWidth/2);
		
		Vector3 oldPos  = _uiGrid.transform.localPosition;
		_uiGrid.transform.localPosition = new Vector3(xOffset ,oldPos.y,oldPos.z);
	}
	
	public static void IconScale(bool useWidth, UISprite sprite)
	{
		Vector3 t = sprite.transform.localScale;
		float scale = useWidth? t.x: t.y;
		UIAtlas.Sprite s = sprite.atlas.GetSprite(sprite.spriteName);
		float size = useWidth? s.inner.width: s.inner.height;
		scale /= size;
		scale *= (useWidth? s.inner.height: s.inner.width);
		if(useWidth)
		{
			t.y = scale;
		}
		else
		{
			t.x = scale;
		}
		sprite.transform.localScale = t;
	}
	
	
	
	public static void IconLeftOfLabel(Transform icon, UILabel text)
	{
		Vector3 oldPos = icon.localPosition;
		float w = text.relativeSize.x * text.cachedTransform.localScale.x;
		Vector3 pos = text.cachedTransform.localPosition;
		pos = new Vector3(pos.x-w,oldPos.y,oldPos.z);
		icon.localPosition = pos;
	}
	
	
	public static void LoadTexture(UITexture icon ,ItemData itemData)
	{
		int role = (int)PlayerInfo.Instance.Role;
		LoadTexture(icon, itemData,role);
	}
	
	public static void LoadTexture(UITexture icon ,ItemData itemData, int role)
	{
		string path = itemData.iconPath;
		LoadTexture(icon, path);
	}
	
	public static void LoadTexture(UITexture icon ,string path)
	{
		if(icon.mainTexture == null)
		{
			Texture2D tex = InJoy.AssetBundles.AssetBundles.Load(path) as Texture2D;
			if(tex == null) {
				string defaultIconPath = EquipmentAssembler.Singleton.GetDefaultIcons(path);
				if(defaultIconPath != null) {
					tex = InJoy.AssetBundles.AssetBundles.Load(defaultIconPath, typeof(Texture2D)) as Texture2D;
				}
			}
			if (tex != null)
			{
				icon.mainTexture = tex;				
			}else{
				Debug.LogError("can't find icon of " + path);	
			}
		}
		icon.alpha = 1;
	}
	
	public static Texture2D LoadTexture(ItemData itemData)
	{
		int role = (int)PlayerInfo.Instance.Role;
		string path = itemData.iconPath;
		Texture2D tex = InJoy.AssetBundles.AssetBundles.Load(path) as Texture2D;
		if(tex == null) {
			string defaultIconPath = EquipmentAssembler.Singleton.GetDefaultIcons(path);
			if(defaultIconPath != null) {
				tex = InJoy.AssetBundles.AssetBundles.Load(defaultIconPath, typeof(Texture2D)) as Texture2D;
			}
		}
		return tex;
	}
	
	public static void UnloadTexture(UITexture icon)
	{
		if(icon.mainTexture != null)
		{
			icon.mainTexture = null;
		}
		icon.alpha = 0.002f;
	}
	
	
	public static string[] BorderNames = new string[5]
	{
		"border0",
		"border1",
		"border2",
		"border3",
		"border4"
	};
	
	public static void SetBorder(UISprite border, int evolutionLevel)
	{
		if(evolutionLevel >=BorderNames.Length)
		{
			evolutionLevel = BorderNames.Length-1;
		}
		border.spriteName = BorderNames[evolutionLevel];
	}
	
	public static void SetBorder(UISprite border, int evolutionLevel, ItemType itemType )
	{
		if(IsCanEvolution(itemType))
		{
			SetBorder(border, evolutionLevel);
		}
		else
		{
			border.spriteName = BorderNames[4];
		}
	}

	public static bool IsCanEvolution(ItemType itemType)
	{
		return itemType == ItemType.weapon || itemType == ItemType.armor || itemType == ItemType.ornament;
	}
	
	public static void DrawText(UILabel[] attributes, List<string> texts)
	{
		for(int i = 0; i< attributes.Length; i++)
		{
			if(i<texts.Count)
			{
				attributes[i].text = texts[i];
			}else{
				attributes[i].text = "";
			}
		}
	}
	
	
	public static void DrawTextChangeLine(UILabel[] attributes, List<string> texts)
	{
		List<string> newTexts = new List<string>();
		char[] splits = new char[]{'\n'};
		
		for(int i = 0; i< texts.Count; i++)
		{
			attributes[0].text = texts[i];
			
			string afterNewLineText = attributes[0].processedText;
			
			string[] nTexts = afterNewLineText.Split(splits);
			
			for(int k = 0; k< nTexts.Length ; k++)
			{
				if(k==0)
				{
					newTexts.Add(nTexts[k]);
				}else{
					newTexts.Add("    "+nTexts[k]);
				}
			}
		}
		
		
		for(int i = 0; i< attributes.Length; i++)
		{
			if(i < newTexts.Count)
			{
				attributes[i].text = newTexts[i];
			}else{
				attributes[i].text = "";
			}
		}
	}
	
	public static string GetBattleTimerString(float timeConsumed)
	{
		if(timeConsumed  < 0)
		{
		   timeConsumed = 0;
		}
		
		System.TimeSpan span = System.TimeSpan.FromSeconds(timeConsumed);
		
		if (span.Hours == 0)
		{
			return string.Format("{0:00}:{1:00}", span.Minutes, span.Seconds);
		}
		else
		{
			return string.Format("{0:00}:{1:00}:{2:00}", span.Hours, span.Minutes, span.Seconds);
		}		
	}

}
