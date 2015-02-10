using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriggerManager : MonoBehaviour
{
	public int _TrggersID = 0;
	
    public Vector3 _player1BornPoint;
    public float _player1RotationY;
    public Vector3 _player2BornPoint;
    public float _player2RotationY;
    public Vector3 _player3BornPoint;
    public float _player3RotationY;
    public Vector3 _player4BornPoint;
    public float _player4RotationY;

    public Vector3 SpawnPoint
    {
        get { return _player1BornPoint; }
    }

    public BornPoint _beginningBornPoint;
    public BornPoint _endBornPoint; //level ends when this trigger ends

    public List<BornPoint> aiTriggerList;

    private static TriggerManager _inst;
    public static TriggerManager Singleton
    {
        get
        {
            return _inst;
        }
    }

    void Awake()
    {
            _inst = this;
    }

    void Start()
    {
		//activate the first trigger. By default, the triggers will disable themselves at Awake()
        if (_beginningBornPoint != null)
        {
            _beginningBornPoint.Active();
        }
    }

    void OnDestroy()
    {
        if (_inst == this)
        {
            _inst = null;

            Debug.LogWarning("[Trigger Manager] Instance destroy ok");
        }
        else
        {
            Debug.LogError("[Trigger Manager] Invalid instance, failed to destroy.");
        }
    }

    /// <summary>
    /// Set the index range for each trigger, including inactive triggers.
    /// </summary>
    public void InitTriggerIndexRange()
    {
        foreach (Transform t in this.transform)
        {
            BornPoint bp = t.GetComponent<BornPoint>();

            Assertion.Check(bp != null);

            int startIndex, endIndex;

            LevelManager.Singleton.GetIndexRangeByTrigger(t.name, out startIndex, out endIndex);

            bp.StartEnemyIndex = startIndex;

            bp.EndEnemyIndex = endIndex;

            Debug.Log(string.Format("Trigger {0}, Start index = {1}, End index = {2}", bp.name, startIndex, endIndex));
		
			if (startIndex < 0)
			{
				break;
			}
		}

        //validate triggers to make sure they are not looped
        ValidateTriggers();
    }

    /// <summary>
    /// check if the triggers are looped
    /// </summary>
    private void ValidateTriggers()
    {
#if UNITY_EDITOR
        Debug.Log("Checking for looped triggers.");
        const int k_max_layer = 10;

        List<BornPoint>[] layerLists = new List<BornPoint>[k_max_layer];
        for (int i = 0; i < k_max_layer; i++)
        {
            layerLists[i] = new List<BornPoint>();
        }

        //fill the lists
        layerLists[0].Add(_beginningBornPoint); //first

        int layerIndex = 0;

        while (layerIndex < k_max_layer - 1)
        {
            List<BornPoint> currentList = layerLists[layerIndex];

            layerIndex++;

            foreach (BornPoint bp in currentList)
            {
                foreach (BornPoint bp1 in bp._nextTriggers)
                {
                    if (bp1 == null)
                    {
                        Debug.LogError(string.Format("BornPoint {0} has next triggers, but some of them are null."));
                    }
                    else
                    {
                        layerLists[layerIndex].Add(bp1);
                    }

                    //check if bp1 is in high layers
                    bool found = false;
                    for (int i = 0; i < layerIndex; i++)
                    {
                        if (layerLists[i].Contains(bp1))
                        {
                            found = true;
                            break;
                        }
                    }

                    Assertion.Check(!found, "Triggers are looped!");
                }
            }

            Debug.Log("Triggers in layer: " + layerIndex);
            foreach (BornPoint bp in layerLists[layerIndex])
            {
                Debug.Log("\t\t" + bp.name);
            }
        }
#endif
    }


    public BornPoint GetAITriggerByName(string name)
    {
        foreach( BornPoint bp in aiTriggerList)
        {
            if (bp.name == name)
            {
                return bp;
            }
        }

        return null;
    }
}
