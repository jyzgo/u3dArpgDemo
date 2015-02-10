using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(FPSMemChecker))]
public class FPSMemDataBase : FPSMemDataAbstract
{
    protected override void Fill(FPSMemRecord record)
    {
        record.Add("timestamp", System.DateTime.Now);
        record.Add("fps",       _memChecker.FPS);
        record.Add("mem",       _memChecker.Memory);
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////
    
    private FPSMemChecker _memChecker;

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    private void Awake()
    {
        _memChecker = GetComponent<FPSMemChecker>();
        if(_memChecker == null)
            Debug.LogError("FpsMemDataSimple requires FPSMemChecker component on same GameObject");
    }
}
