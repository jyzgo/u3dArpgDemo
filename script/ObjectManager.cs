using UnityEngine;
using System.Collections.Generic;

public class ObjectManager : MonoBehaviour
{

    //optimized, replace List to Dictionary , copy network object into network objects dictionary
    private Dictionary<int, OBJECT_ID> _objectsDic = new Dictionary<int, OBJECT_ID>(); // key = ID , values include network objects
    private Dictionary<int, OBJECT_ID> _netObjectsDic = new Dictionary<int, OBJECT_ID>(); // key = networkID , value just include network objects

    public int ObjectCount
    {
        get { return _objectsDic.Count; }
    }

    public int NetObjectCount
    {
        get { return _netObjectsDic.Count; }
    }

    static ObjectManager _inst;
    static public ObjectManager Instance
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

    void OnDestroy()
    {
        _objectsDic.Clear();
        _netObjectsDic.Clear();
        _objectsDic = null;
        _netObjectsDic = null;
        _inst = null;
    }

    public void SaveObject(OBJECT_ID thisObject)
    {
        _objectsDic[(int)thisObject] = thisObject;
    }

    //this function should only be called in OBJECT_ID set network id property
    public void SaveNetObject(OBJECT_ID thisObject)
    {
        _netObjectsDic[thisObject.NetworkId] = thisObject;
    }

    //this function should be called when object is dead or is destroyed 
    public void RemoveObject(OBJECT_ID thisObject)
    {
        if (_objectsDic.ContainsKey((int)thisObject))
        {
            _objectsDic.Remove((int)thisObject);
        }

        if (thisObject.NetworkId != -1)
        {
            if (_netObjectsDic.ContainsKey(thisObject.NetworkId))
            {
                _netObjectsDic.Remove(thisObject.NetworkId);
            }
        }
    }

    public OBJECT_ID GetObjectByID(int objID)
    {
        //optimized by dictionary for each search
        if (!_objectsDic.ContainsKey(objID))
        {
            return null;
        }
        return _objectsDic[objID];
    }

    public OBJECT_ID GetObjectByNetworkID(int networkID)
    {
        //optimized by dictionary for each search
        if (!_netObjectsDic.ContainsKey(networkID))
        {
            return null;
        }
        return _netObjectsDic[networkID];
    }

    //get ac of the player under my control
    public ActionController GetMyActionController()
    {
        int myIndex = MatchPlayerManager.Instance.GetPlayerIndex();
        OBJECT_ID objectID = ObjectManager.Instance.GetObjectByNetworkID(myIndex + FCConst.k_network_id_hero_start);

        if (objectID != null)
        {
            ActionController ac = objectID.fcObj as ActionController;
            Assertion.Check(ac.IsPlayer && ac.IsPlayerSelf, "get a bad ac (not player or under my control)");
            return ac;
        }

        return null;
    }


    public List<ActionController> GetEnemyActionController()
    {
        int myIndex = MatchPlayerManager.Instance.GetPlayerIndex();
        OBJECT_ID objectID = ObjectManager.Instance.GetObjectByNetworkID(myIndex + FCConst.k_network_id_hero_start);

        List<ActionController> enemyAcList = new List<ActionController>();

        foreach (KeyValuePair<int, OBJECT_ID> pre in _netObjectsDic)
        {
            if (pre.Value != objectID)
            {
                ActionController ac = pre.Value.fcObj as ActionController;

                if(null != ac)
                {
                    enemyAcList.Add(ac);
                }
            }
        }

        return enemyAcList;
    }

}