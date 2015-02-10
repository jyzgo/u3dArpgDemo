using UnityEngine;
using System.Collections.Generic;

public class UIPlayerPreview : MonoBehaviour 
{
	public GameObject _root;
	public bool _self = false;

	GameObject _previewModel;
	public GameObject PreviewModal
	{
		get
		{
			return _previewModel;
		}
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	public void Initialize()
	{
		InitializePreview();
	}

    public void InitializePreview(int playerClass)
    {
		string label = GameSettings.Instance.roleSettings[playerClass].previewLabel;
        
		_previewModel = CharacterAssembler.Singleton.AssembleCharacterWithoutAI(label);

        _previewModel.GetComponent<AvatarController>().Init("", "", playerClass);
        _previewModel.transform.parent = _root.transform;
        _previewModel.transform.localPosition = Vector3.zero;
        _previewModel.transform.localRotation = Quaternion.identity;
        _previewModel.transform.localScale = Vector3.one;
        _previewModel.layer = _root.layer;
    }

	public void InitializePreview()
	{
		FC_AI_TYPE playerClass;

		//todo: show other player preview
		//if (!_self)
		//{
		//    playerClass = (FC_AI_TYPE)TownPlayerManager.Singleton.ActivedPlayerInfo._class;
		//}
		//else
		//{
		//    playerClass = (FC_AI_TYPE)PlayerInfoManager.Singleton.AccountProfile._curRole;
		//}
		playerClass = FC_AI_TYPE.PLAYER_WARRIOR;//by caizilong

		_previewModel = CharacterAssembler.Singleton.AssembleCharacterWithoutAI(GameSettings.Instance.roleSettings[(int)playerClass].previewLabel);

		_previewModel.GetComponent<AvatarController>().Init("", "", (int)playerClass);
		_previewModel.transform.parent = _root.transform;
		_previewModel.transform.localPosition = Vector3.zero;
		_previewModel.transform.localRotation = Quaternion.identity;
		_previewModel.transform.localScale = Vector3.one;

		if (!_self)
		{
			UpdateEquipments(_previewModel);
		}
	}
	
	void UpdateEquipments(GameObject previewModal)
	{
		// refresh data.
		List<GameObject> EquipInstances = new List<GameObject>();

        if (!_self)
        {
			//OLD_PlayerInfo playerInfo = TownPlayerManager.Singleton.ActivedPlayerInfo;
			//PlayerProfile.GetOtherEquipmentInstanceWithIds(EquipInstances, playerInfo._equipIds, playerInfo._class);
        }
        else
		{
			int role = (int)PlayerInfo.Instance.Role;
			PlayerInfo.Instance.GetSelfEquipmentInstance(EquipInstances);
		}
		
		
		List<FCEquipmentsBase> equipments = new List<FCEquipmentsBase>();
		// get equipment data array
		foreach(GameObject g in EquipInstances) {
			FCEquipmentsBase []es = g.GetComponentsInChildren<FCEquipmentsBase>();
			equipments.AddRange(es);
			// destroy ai game object
			GameObject.Destroy(g);
		}
		// equip models.
		AvatarController avatar = previewModal.GetComponent<AvatarController>();
		avatar.RemoveWeapons();
		foreach (FCEquipmentsBase eeb in equipments)
		{
			EquipmentAssembler.Singleton.Assemble(eeb, avatar);
		}
		
		Renderer []renderers = _previewModel.GetComponentsInChildren<Renderer>();
		foreach(Renderer r in renderers) {
			if(r.gameObject.layer != LayerMask.NameToLayer("TransparentFX")) {
				r.gameObject.layer = LayerMask.NameToLayer("2DUI");
			}
		}
	}
	
	// Clean
	public void Clean()
	{	
		if(_previewModel != null)
		{
			Destroy(_previewModel);
			_previewModel = null;
		}

	}
}
