
using System.Collections;
using UnityEngine;

public class FCWorldmapRegionSelect : MonoBehaviour
{
    public UITexture backgroundTexture;

    public GameObject closeButton;

    public GameObject enterRegionButton;

    public UITexture faLuoTexture;

    public UITexture lvSenTexture;

    public UITexture xiuLanTexture;

    void Awake()
    {
        UIEventListener.Get(enterRegionButton).onClick = OnEnterWorldmapRegion;
        UIEventListener.Get(closeButton).onClick = OnCloseWorldMap;
    }

	void Start()
	{
        faLuoTexture.mainTexture = InJoy.AssetBundles.AssetBundles.Load("Assets/UI/bundle/Worldmap/Faluo.png") as Texture;
        lvSenTexture.mainTexture = InJoy.AssetBundles.AssetBundles.Load("Assets/UI/bundle/Worldmap/LvSen.png") as Texture;
        xiuLanTexture.mainTexture = InJoy.AssetBundles.AssetBundles.Load("Assets/UI/bundle/Worldmap/XiuLan.png") as Texture;
        backgroundTexture.mainTexture = InJoy.AssetBundles.AssetBundles.Load("Assets/UI/bundle/Worldmap/Universe.png") as Texture;
	}
	
	void OnInitialize()
	{
		TownHUD.Instance.TempHide();
	}

	void OnCloseWorldMap(GameObject go)
	{
		UIManager.Instance.CloseUI("FCWorldmapRegionSelect");
		UIManager.Instance.OpenUI("TownHome");
        TownHUD.Instance.ResumeShow();
	}
	
	public void OnEnterWorldmapRegion(GameObject go)
	{
        UIManager.Instance.OpenUI("FCWorldmapLevelSelect", WorldmapRegion.Faluo);
        UIManager.Instance.CloseUI("FCWorldmapRegionSelect");
	}
}