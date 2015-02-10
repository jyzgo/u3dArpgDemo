using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ActivedTriggerInfo
{
    public BornPoint _trigger;
    public int _alived;

    public void OnEnemyDie(int index)
    {
        if (index >= _trigger.StartEnemyIndex && index < _trigger.EndEnemyIndex)
        {
            --_alived;
        }
    }
}

public enum TriggerActiveType
{
	TriggerActiveType_KillAll
}

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class BornPoint : MonoBehaviour
{
    public int _ticketForMelee = 4;
    public int _ticketForRanger = 3;
    public int _ticketForElite = 2;

    public TriggerActiveType _nextTriggerCondition;

    public BornPoint[] _nextTriggers;
    public BornPoint[] summonTriggers;

    public AirGate[] _activeBlockades;
    public AirGate[] _deactiveBlockades;

    private int _startEnemyIndex, _endEnemyIndex;  //index range for this trigger, acquired from level manager

    public int StartEnemyIndex
    {
        get
        {
            return _startEnemyIndex;
        }
        set 
        {
            _startEnemyIndex = value;
        }
    }

    public int EndEnemyIndex
    {
        get
        {
            return _endEnemyIndex;
        }
        set
        {
            _endEnemyIndex = value;
        }
    }

    private BornPointCallback _callback;
    private bool _triggered = false; //for prevent from multi-triggering
    private bool _doorTriggered = false;
    public bool Triggered 
    { 
        get 
        {
            return _triggered; 
        }
    }

    private bool _shielding = false; //shielding the trigger in some cases.
    public bool Shielding
    {
        get 
        { 
            return _shielding;
        }
        set 
        { 
            _shielding = value;
        }
    }

    private Collider _myCollider;
    public Collider MyCollider
    {
        get 
        { 
            return _myCollider; 
        }
    }

    private bool _isAllDead = false;
    public bool IsAllDead
    {
        set 
        {
            _isAllDead = value;
        }
        get 
        {
            return _isAllDead;
        }
    }

    public enum SpawnPointType
    {
        Type1 = 0,
        Type2,
        Type3,
        Type4,
        Type5,
        Count
    }

#if UNITY_EDITOR        //for Editor mode
    private GameObject _hiddenRoot;

    private Dictionary<Transform, Transform> _dict = new Dictionary<Transform, Transform>();    //actual -- hidden

    private const string k_mat_path_root = "Assets/Editor/Level/Materials/";
	private Material[] _indicatorMats = new Material[(int)BornPoint.SpawnPointType.Count];
#endif

    void Awake()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) //running in design mode
        {
            for (int i = 0; i < _indicatorMats.Length; ++i)
            {
                _indicatorMats[i] =
					AssetDatabase.LoadAssetAtPath(k_mat_path_root + ((BornPoint.SpawnPointType)i).ToString() + ".mat", typeof(Material)) as Material;
            }
        }
        else
#endif
        {
            _myCollider = GetComponent<Collider>();

            _myCollider.enabled = false;        //default state

            // init next trigger condition.
            switch (_nextTriggerCondition)
            {
                case TriggerActiveType.TriggerActiveType_KillAll:
                    _callback = new KillAllBornPoint();
                    break;
                default:
                    Debug.LogError(gameObject.name + ": wrong trigger active type setting.");
                    break;
            }
        }
    }
	

    public void Active()
    {
        _myCollider.enabled = true;
        if (UIManager.Instance != null && UIManager.Instance._indicatorManager != null)
        {
            UIManager.Instance._indicatorManager.ActiveTriggerIndicator(transform.position);
        }
    }
    public void Deactive()
    {
        _myCollider.enabled = false;
        _triggered = true;
    }

    void OnTriggerEnter(Collider other)
    {
        ActionController ac = ActionControllerManager.Instance.GetACByCollider(other);

        // some hero steps into the trap.
        if (ac != null)
        {
            if (!_triggered)
            {
                ActivedTriggerInfo info = new ActivedTriggerInfo();
                info._trigger = this;

                info._alived = _endEnemyIndex - _startEnemyIndex;

                Debug.Log(string.Format("Trigger activated. Name = {0}, enemy index range = {1}, {2} live enemy count = {3}", this.name, _startEnemyIndex, _endEnemyIndex, info._alived));

                LevelManager.Singleton.AddActivedTrigger(info);

                _triggered = true;

                StartCoroutine(ActivateEnemy());

                if (UIManager.Instance != null && UIManager.Instance._indicatorManager != null)
                {
                    UIManager.Instance._indicatorManager.DeactiveTriggerIndicator();
                }
            }

            if (ac.IsPlayerSelf && !_doorTriggered)
            {
                SetTickets();

                // Active the air gate.
                foreach (AirGate ag in _activeBlockades)
                {
                    ag.Active();
                }

                _doorTriggered = true;
                Deactive();
            }
        }
    }

    private void SetTickets()
    {
        FCTicketManager.Instance._ticketForMelee = _ticketForMelee;
        FCTicketManager.Instance._ticketForRanger = _ticketForRanger;
        FCTicketManager.Instance._ticketForElite = _ticketForElite;
    }

    private IEnumerator ActivateEnemy()
    {
        // first enemy waiting time.
        float waitingTime = Random.Range(0, 101) / 100.0f;

        yield return new WaitForSeconds(waitingTime);

        //record which spot index has been used and must be avoided being used again until there is no vacancy
		List<int>[] mapping = new List<int>[(int)SpawnPointType.Count];

        for (int i = _startEnemyIndex; i < _endEnemyIndex; ++i)
        {
            //randomly choose a spawn point for this enemy, the enemy type must fit
            //require: spawn points with the same enemy type must stay together

			SpawnPointType spawnType = LevelManager.Singleton.GetEnemySpawnPointTypeByIndex(i);

            List<int> spotList = mapping[(int)spawnType];

            //available spot list not built yet or spots have been used up, in both cases, need to rebuild
            if (spotList == null)
            {
                spotList = new List<int>();

                mapping[(int)spawnType] = spotList;

                int count = this.transform.childCount;

				while (spawnType >= SpawnPointType.Type1)
				{
					for (int childIndex = 0; childIndex < count; childIndex++)
					{
						EnemySpot spot = this.transform.GetChild(childIndex).GetComponent<EnemySpot>();
						if (spot.acceptedSpawnType == spawnType)
						{
							spotList.Add(childIndex);
						}
					}

					if (spotList.Count > 0)
					{
						break;	//found a suitable spawn point
					}
					else
					{
						if (spawnType == SpawnPointType.Type1)	//the bottom has been reached, report error.
						{
							Assertion.Check(spotList.Count > 0, string.Format("Spawn point type {0} not found for trigger {1}", spawnType.ToString(), this.name));
						}
						else
						{
							spawnType--;
							Debug.LogWarning(string.Format("Required spawn point {0} not found, degrad it and search again.", spawnType.ToString()));
						}
					}
				}

                //randomize the elements
                for (int k = 0; k < spotList.Count - 1; k++)
                {
                    int rdm = Random.Range(k + 1, spotList.Count);

                    int a = spotList[rdm];

                    spotList[rdm] = spotList[k];

                    spotList[k] = a;

                }

                //Debug show
                foreach (int spotIndex in spotList)
                {
                    Debug.Log(string.Format("Spot index list for enemy type [{0}]: {1}", spawnType.ToString(), spotIndex));
                }
            }

            //Get delay time of an ememy group, which is attached to the first enemy of the group
            float delayTime = LevelManager.Singleton.GetDelayTimeByEnemyIndex(i);
            if (delayTime > 0)
            {
                Debug.Log(string.Format("Waiting {0} seconds on enemy index {1}", delayTime, i));
                yield return new WaitForSeconds(delayTime);
            }

            LevelManager.Singleton.ActivateEnemyAtSpot(i, this.transform.GetChild(spotList[0]).GetComponent<EnemySpot>());

            Debug.Log(string.Format("\t\tEnemy created. Trigger name: {0}  Spawn point: {1}  Enemy index: {2}", this.name, spotList[0], i));

            //move the 1st to last
            spotList.Add(spotList[0]);

            spotList.RemoveAt(0);
			
            waitingTime = Random.Range(0, 101) / 100.0f;
            yield return new WaitForSeconds(waitingTime);
        }
    }

    public bool CallbackPerFrame(ActivedTriggerInfo info)
    {
        IsAllDead = _callback.OnCallback(info);

        bool isSummonPass = true;

        if (null != summonTriggers)
        {
            foreach (BornPoint bp in summonTriggers)
            {
                if ((bp.Triggered && !bp.IsAllDead))
                {
                    isSummonPass = false;
                }
            }
        }

        if (IsAllDead && isSummonPass)
        {
            _myCollider.enabled = false;

            //broadcast myself to clients, remove myself and active next triggers

            foreach (BornPoint bp in _nextTriggers)
            {
                bp.Active();

            }

            // Deactive the next gates.
            foreach (AirGate ag in _deactiveBlockades)
            {
                ag.Deactive();
            }

#if PVP_ENABLED
            if (GameManager.Instance.IsPVPMode)
            {
            }
            else
            {
#endif
            // Next trigger is null, so the player passes the level.



            if (TriggerManager.Singleton._endBornPoint == this)
            {
                FinishLevel();
            }

#if PVP_ENABLED
            }
#endif
            return true;
        }
        return false;
    }

    public static void FinishLevel()
    {
        LevelManager.Singleton.LevelFinishFlag = true;

        ActionController ac = ObjectManager.Instance.GetMyActionController();

        if (LevelLightInfo.s_completeCamera != null)
        {
            CameraController.Instance.SetFeatureCamera(LevelLightInfo.s_completeCamera, 2.0f, null, null);
        }

        UIManager.Instance.OpenUI("BattleSummeryTimerUI");

        //update quests for level completion				
        QuestManager.instance.UpdateQuests(QuestTargetType.complete_level, LevelManager.Singleton.CurrentLevel, 1);
    }



#if UNITY_EDITOR
    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            InstantiateHiddenPrefabs();
        }
    }

    private void OnDisable()
    {
        if (!Application.isPlaying)
        {
            //Remove all the hidden objects created before since they will remain even after the scene is closed.
            if (_hiddenRoot)
            {
                _dict.Clear();
                DestroyImmediate(_hiddenRoot);
            }
        }
    }

    private const string k_hidden_root_name = "Hidden Root";
    /// <summary>
    /// When in Editor, create hidden game objects to show all possibile spawn points.
    /// </summary>
    private void InstantiateHiddenPrefabs()
    {
        if (!_hiddenRoot)
        {
            _hiddenRoot = EditorUtility.CreateGameObjectWithHideFlags(k_hidden_root_name + '_' + this.name, HideFlags.HideAndDontSave);
        }

        foreach (Transform t in transform)
        {
            AddHiddenChild(t);
        }
    }

	private Material GetMaterialByEnemyType(SpawnPointType enemyType)
    {
        return _indicatorMats[(int)enemyType];
    }

    private void AddHiddenChild(Transform spawnPointTrans)
    {
        EnemySpot sourceSpot = spawnPointTrans.GetComponent<EnemySpot>();

        Assertion.Check(sourceSpot != null, "Each child of trigger must have an EnemySpot component!");

        if (sourceSpot == null)
        {
            return;
        }

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        go.transform.renderer.sharedMaterial = GetMaterialByEnemyType(sourceSpot.acceptedSpawnType);

        go.transform.parent = _hiddenRoot.transform;
        go.transform.position = spawnPointTrans.position;
        go.transform.rotation = spawnPointTrans.rotation;
        go.transform.localScale = spawnPointTrans.localScale;
        go.hideFlags = HideFlags.HideAndDontSave;
        go.name = spawnPointTrans.name;

        EnemySpot hiddenSpot = go.AddComponent<EnemySpot>();
        hiddenSpot.acceptedSpawnType = sourceSpot.acceptedSpawnType;

        _dict.Add(spawnPointTrans, go.transform);
    }

    private void Update()
    {
        if (!Application.isPlaying) //run in design mode only
        {
            //add new nodes
            foreach (Transform t in transform)
            {
                if (!_dict.ContainsKey(t))
                {
                    AddHiddenChild(t);
                }
            }

            //remove redundant nodes
            List<Transform> removeList = new List<Transform>();
            foreach (Transform t in _dict.Keys)
            {
                bool found = false;
                foreach (Transform tt in transform)
                {
                    if (t == tt)
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    DestroyImmediate(_dict[t].gameObject);
                    removeList.Add(t);
                }
            }

            foreach (Transform t in removeList)
            {
                _dict.Remove(t);
            }

            //align the spawn points and display objects
            foreach (KeyValuePair<Transform, Transform> kv in _dict)
            {
                if (kv.Key.position != kv.Value.position)
                {
                    kv.Value.position = kv.Key.position;
                }
                if (kv.Key.rotation != kv.Value.rotation)
                {
                    kv.Value.rotation = kv.Key.rotation;
                }
                if (kv.Key.localScale != kv.Value.localScale)
                {
                    kv.Value.localScale = kv.Key.localScale;
                }

                //check the change of enemy type
                EnemySpot sourceSpot = kv.Key.GetComponent<EnemySpot>();
                EnemySpot hiddenSpot = kv.Value.GetComponent<EnemySpot>();

                if (sourceSpot.acceptedSpawnType != hiddenSpot.acceptedSpawnType)
                {
                    hiddenSpot.transform.renderer.sharedMaterial = GetMaterialByEnemyType(sourceSpot.acceptedSpawnType);
                    hiddenSpot.acceptedSpawnType = sourceSpot.acceptedSpawnType;
                }
            }
        }
    }
#endif
}

public class BornPointCallback
{
	virtual public bool OnCallback(ActivedTriggerInfo info) 
	{
		return false;
	}
}

public class KillAllBornPoint : BornPointCallback
{
	public override bool OnCallback (ActivedTriggerInfo info)
	{
		return info._alived == 0;
	}
}
