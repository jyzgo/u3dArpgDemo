using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FPSMemCollector : MonoBehaviour
{
    public CollectorType CollectorTypeValue  = CollectorType.PER_SECOND;
    public int CollectPeriod                 = 5;
    public int CacheSize                     = 1;
    public List<RuntimePlatform> Platforms   = new List<RuntimePlatform>()
    {
        RuntimePlatform.WindowsEditor,
        RuntimePlatform.OSXEditor,
        RuntimePlatform.IPhonePlayer,
        RuntimePlatform.Android
    };
    
    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public enum CollectorType
    {
        PER_FRAME,
        PER_SECOND,
    };
    
    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////
    
    private FPSMemDataAbstract[]  _datas;
    private FPSMemStoreAbstract[] _stores;
    
    private FPSMemRecord[] _records;
    private int _currentCachePosition;
    
    private int _framesLeftToCollect;
    private float _lastCollectTime;
    
    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    private void Awake()
    {
        FindComponents();
        
        if(!CheckPlatform())
        {
            RemoveComponents();
            return;
        }
        
        InitCache();
        
        _framesLeftToCollect = CollectPeriod;
        _lastCollectTime = Time.realtimeSinceStartup;
    }
    
    private bool CheckPlatform()
    {
        return Platforms.Contains(Application.platform);
    }
    
    private void RemoveComponents()
    {
        GameObject.Destroy(this);
        
        foreach(FPSMemDataAbstract data in _datas)
            GameObject.Destroy(data);
        
        foreach(FPSMemStoreAbstract store in _stores)
            GameObject.Destroy(store);
    }
    
    private void FindComponents()
    {
        _datas = GetComponents<FPSMemDataAbstract>();
        if((_datas == null) || (_datas.Length <= 0))
            Debug.LogError("FpsMemCollector requires FPSMemData component on same GameObject");

        _stores = GetComponents<FPSMemStoreAbstract>();
        if((_stores == null) || (_stores.Length <= 0))
            Debug.LogError("FpsMemCollector requires FPSMemStore component on same GameObject");
    }
    
    private void InitCache()
    {
        _records = new FPSMemRecord[CacheSize];
        for(int i = 0; i < _records.Length; i++)
            _records[i] = new FPSMemRecord();
        
        _currentCachePosition = 0;
    }
    
    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    private void Update()
    {
        switch(CollectorTypeValue)
        {
        case CollectorType.PER_FRAME:
            UpdateFrames();
            break;
        case CollectorType.PER_SECOND:
            UpdateSeconds();
            break;
        }
    }    
    
    private void UpdateFrames()
    {
        _framesLeftToCollect -= 1;
        if(_framesLeftToCollect <= 0)
        {
            Collect();
            _framesLeftToCollect = CollectPeriod;
        }
    }
    
    private void UpdateSeconds()
    {
        if(Time.realtimeSinceStartup - _lastCollectTime >= CollectPeriod)
        {
            Collect();
            _lastCollectTime = Time.realtimeSinceStartup;
        }
    }
    
    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    private void OnDestroy()
    {
        // TODO: save cache
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////
    
    private void Collect()
    {
        _records[_currentCachePosition].Clear();
        foreach(FPSMemDataAbstract data in _datas)
            data.Collect(_records[_currentCachePosition]);
        
        _currentCachePosition += 1;
        if(_currentCachePosition >= CacheSize)
        {
            _currentCachePosition = 0;
            Store();
        }
    }
    
    private void Store()
    {
        foreach(FPSMemStoreAbstract store in _stores)
            store.Store(_records);
    }
}
