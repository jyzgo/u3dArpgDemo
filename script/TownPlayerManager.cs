using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InJoy.Utils;
using InJoy.FCComm;

/// <summary>
/// Town player manager.
/// handle players walking in town, player for reviewing in store, born players.
/// </summary>
public class TownPlayerManager : MonoBehaviour
{
	public Vector3 _heroBornPoint;
	[HideInInspector]
	public Vector3[] _friendBornPoints;

	public string _dynamicAssetPath;
	public Transform _dynamicAssetParent;

	// key is GameSpy ID
	private Dictionary<string, HeroInstance> _playersInTown;
	int _friendBornSeed; // for borning players randomly in map.
	Transform _charactersRoot;

	string _activedPlayer; // id of current clicked player.
	public string ActivedPlayerID
	{
		get { return _activedPlayer; }
		set { _activedPlayer = value; }
	}
	// help method. get information of clicked player directly.
	public PlayerInfo ActivedPlayerInfo
	{
		get { return PlayerInfo.Instance; }
	}

	//the main character instance
	private HeroInstance _heroInfo;
	public HeroInstance HeroInfo
	{
		get { return _heroInfo; }
	}

	static TownPlayerManager _inst;
	static public TownPlayerManager Singleton
	{
		get { return _inst; }
	}

	void LoadCharacter(HeroInstance info, List<EquipmentIdx> equipmentIdList, string id, string nickName, string guildName, Vector3 pos, string path)
	{
		CharacterFactory.Singleton.BornPoint = pos;
		info.InitCharacter(info._instLabel, nickName, guildName);
		info._actionController.Data.patrolPath = path;

		if (string.IsNullOrEmpty(path))
		{
			info._actionController.IsPlayerSelf = true;
		}
		else //NPC player
		{
			info._actionController.Data.equipList = equipmentIdList;
		}
		info.Active(pos);

		Transform t = info._inst.transform;

		t.parent = _charactersRoot;

		if (id != null)
			_playersInTown[id] = info;
		else
			Debug.LogWarning("I am a single mode");

		// setup UI connection.
		OnTapEvent[] cps = info._inst.GetComponentsInChildren<OnTapEvent>();
		foreach (OnTapEvent c in cps)
		{
			c.PlayerID = id;
		}
	}

	void Awake()
	{
		if (_inst != null)
		{
			Debug.LogError("Duplicated TownManager");
		}
		else
		{
			_inst = this;
		}
		_playersInTown = new System.Collections.Generic.Dictionary<string, HeroInstance>();
		_friendBornSeed = 0;

		// TownPlayerManger itself is path manager as well.
		LevelManager.Singleton.PathManager = gameObject;

		// create root node for players.
		_charactersRoot = Utils.NewGameObjectWithParent("Characters");

		// disable camera before initialize completed.
		//_townCamera.gameObject.SetActive(false);
		DynamicAssetLoader daloader = InJoy.AssetBundles.AssetBundles.Load(_dynamicAssetPath, typeof(DynamicAssetLoader)) as DynamicAssetLoader;
		if (daloader != null)
		{
			daloader.LoadAllAssets(_dynamicAssetParent);
		}
	}

	void Start()
	{
		// 1. load characters.
		GameSettings gs = GameSettings.Instance;

		// 1.1 load local player.
		PlayerInfo localPlayer = PlayerInfo.Instance;
		_heroInfo = new HeroInstance();
		_heroInfo._instLabel = GameSettings.Instance.roleSettings[PlayerInfo.Instance.RoleID].townLabel;
		LoadCharacter(_heroInfo, null, localPlayer.UID.ToString(), localPlayer.DisplayNickname, localPlayer.GuildName, _heroBornPoint, "");
		HeroIconRenderer.Singleton.AddIconExternally(localPlayer, _heroInfo._avatarController._icon);

		CameraController.Instance.SetTarget(_heroInfo._actionController.ThisTransform);

		// 1.2 load other players.
		PlayerInfo.Instance.loadedPlayerInfoList = null;
		StartCoroutine(LoadOtherPlayers());
	}

	IEnumerator LoadOtherPlayers()
	{
		while (true)
		{
			if (PlayerInfo.Instance.loadedPlayerInfoList != null && PlayerInfo.Instance.loadedPlayerInfoList.Count > 0)
			{
				break;
			}
			yield return new WaitForSeconds(0.1f);
		}


		FCPath[] paths = GetComponentsInChildren<FCPath>();
		activedPath = UnityEngine.Random.Range(0, paths.Length);

		foreach (PlayerInfo playerInfo in PlayerInfo.Instance.loadedPlayerInfoList)
		{
			//playerInfo.GetSelfInformation();
			SpawnOneOtherPlayer(playerInfo);
			yield return new WaitForSeconds(1.0f);
		}

	}


	private int activedPath = 0;
	public void SpawnOneOtherPlayer(PlayerInfo p)
	{
		if (GameManager.Instance.GameState != EnumGameState.InTown)
		{
			return;
		}

		if (!gameObject.activeSelf)
		{
			return;
		}


		if (p.haveSpawned)
		{
			return;
		}


		p.haveSpawned = true;
		GameSettings gs = GameSettings.Instance;
		FCPath[] paths = GetComponentsInChildren<FCPath>();

		p.GetSelfEquipmentIds(p.equipIds);

		HeroInstance hi = new HeroInstance();

		hi._instLabel = GameSettings.Instance.roleSettings[p.RoleID].townNPCLabel;

		LoadCharacter(hi, p.equipIds, p.UID.ToString(), p.DisplayNickname, p.GuildName, _friendBornPoints[_friendBornSeed], paths[activedPath].gameObject.name);
		HeroIconRenderer.Singleton.AddIconExternally(p, hi._avatarController._icon);
		activedPath = (activedPath + 1) % paths.Length;
		_friendBornSeed = (_friendBornSeed + 1) % _friendBornPoints.Length;
	}



	void OnDestroy()
	{
		if (_inst == this)
		{
			_inst = null;
		}
		_playersInTown.Clear();
	}


	// methods used for custom inspector.
#if UNITY_EDITOR
	public void ResetFriendBornPoints(int length)
	{
		_friendBornPoints = ResizeArray(_friendBornPoints, length);
	}

	Vector3[] ResizeArray(Vector3[] old, int newLength)
	{
		Vector3[] newArray = new Vector3[newLength];
		int copylength = 0;
		if (old != null)
		{
			copylength = Mathf.Min(old.Length, newLength);
			for (int i = 0; i < copylength; ++i)
			{
				newArray[i] = old[i];
			}
		}
		for (int i = copylength; i < newLength; ++i)
		{
			newArray[i] = transform.position + Vector3.forward * 2;
		}
		return newArray;
	}
#endif
}
