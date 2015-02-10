using UnityEngine;
using System.Collections;


/// <summary>
/// World map controller.
/// 
/// Create and destory worldmap gameobject.
/// </summary>
public class WorldMapController : MonoBehaviour 
{
    public string []worldMapPaths;
	private GameObject []_worldmap = null;
	private GameObject _activeWorldmap = null;
	
	public string _bgMusic = "bgm_map.mp3";

    public static string LevelName;

    public static int DifficultyLevel;
	
	#region Singleton
	private static WorldMapController _instance = null;
	public static WorldMapController Instance
	{
		get
		{			
			return _instance;
		}
	}
	#endregion

	// Use this for initialization
	void Awake () 
	{		
		_instance = this;
		
		//load world map object
		_worldmap = new GameObject[worldMapPaths.Length];
		for(int i = 0;i < worldMapPaths.Length;++i) {
        	GameObject prefab = InJoy.AssetBundles.AssetBundles.Load(worldMapPaths[i]) as GameObject;
        
        	_worldmap[i] = GameObject.Instantiate(prefab) as GameObject;

			_worldmap[i].SetActive(false);
		}
	}
	
	void OnDestroy()
	{
		if(_instance == this)
		{
			_instance = null;
		}
	}	
	
	// Create world map.
	public void CreateWorldMap(int worldmapIdx)
	{
		ActiveWorldMap(worldmapIdx);
		
		CameraController.Instance.gameObject.SetActive(false);
		CustomUIRenderer.SetEnable(false);
		
		SoundManager.Instance.PlayBGM(_bgMusic, 1.0f);
		
		UpdateLevelButtonsState();
	}
	
	public void ActiveWorldMap(int worldmapIdx)
	{
		if(_activeWorldmap != null) {
			_activeWorldmap.SetActive(false);
		}
		_activeWorldmap = _worldmap[worldmapIdx];
		_activeWorldmap.SetActive(true);
		_activeWorldmap.transform.localPosition = new Vector3(2000.0f, 0.0f, 0.0f);
	}
	
	public void UpdateLevelButtonsState()
	{
		WorldMapChessPieceHandler[] worldLevels = _activeWorldmap.GetComponentsInChildren<WorldMapChessPieceHandler>();
		foreach(WorldMapChessPieceHandler level in  worldLevels)
		{
			level.UpdateState();
		}
	}
	
	// Destroy world map
	public void DestroyWorldMap()
	{
		if(_activeWorldmap != null) {
			_activeWorldmap.SetActive(false);
		}
		SoundManager.Instance.PlayBGM("town_theme.mp3", 1.0f);

		CameraController.Instance.gameObject.SetActive(true);
		CustomUIRenderer.SetEnable(true);
	}
}
