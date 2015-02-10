using UnityEngine;
using System.Collections.Generic;

public class UIBattleRewardsCoverFlow : NGUICoverFlow
{
	public GameObject _itemPrefab;

	public string[] _itemBgSprites;
	public string[] _consumableBgSprites;
	public Shader _transparentShader;

	private List<ItemInventory> _itemList;

	// Use this for initialization
	public override void Initialize()
	{
		_itemList = PlayerInfo.Instance.CombatInventory.itemList;

		photosCount = _itemList.Count;

		base.Initialize();
	}

	public override void Clear()
	{
		foreach (Transform t in transform)
		{
			GameObject.Destroy(t.gameObject);
		}

		base.Clear();
	}

	protected override void LoadImages()
	{
		for (int i = 0; i < photosCount; i++)
		{
			GameObject photo = new GameObject("Item");
			ItemData itemData = _itemList[i].ItemData;

			Texture icon_tex = UIUtils.LoadTexture(itemData);
			GameObject icon = GameObject.CreatePrimitive(PrimitiveType.Plane);
			
			icon.renderer.material.mainTexture = icon_tex;
			icon.renderer.material.shader = _transparentShader;
			icon.transform.parent = photo.transform;
			icon.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
			icon.transform.localPosition = new Vector3(0f, 1f, 0f);

			string bgName = _itemBgSprites[_itemList[i].DisplayRareLevel];
			if (!UIUtils.IsCanEvolution(_itemList[i].ItemData.type))
			{
				bgName = _consumableBgSprites[_itemList[i].DisplayRareLevel];
			}

			Texture bg_tex = InJoy.AssetBundles.AssetBundles.Load(bgName) as Texture;
			GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Plane);
			bg.renderer.material.mainTexture = bg_tex;
			bg.renderer.material.shader = _transparentShader;
			bg.transform.parent = photo.transform;
			BoxCollider collider = bg.AddComponent<BoxCollider>();
			collider.center = new Vector3(0f, 2f, 0f);
			UIRewardItemHandler item = bg.AddComponent<UIRewardItemHandler>();
			item._coverFlow = this;
			item._itemIndex = i;

			photos.Add(photo);
			NGUITools.SetLayer(photo, gameObject.layer);
			photo.transform.eulerAngles = new Vector3(-90f, 0f, 0f);
			photo.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			photo.transform.localPosition = new Vector3(0f, 0f, 0f);
			photo.transform.parent = gameObject.transform;
		}

		base.LoadImages();
	}

	protected override void OnCoverFlowed(int index)
	{
		base.OnCoverFlowed(index);
	}
}