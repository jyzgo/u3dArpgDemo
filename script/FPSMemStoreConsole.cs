using UnityEngine;
using System.Collections;

public class FPSMemStoreConsole : FPSMemStoreAbstract
{
    public override void Store(FPSMemRecord[] records)
    {
        foreach(FPSMemRecord record in records)
            Log(record);
    }
    
    private void Log(FPSMemRecord record)
    {
        string logString = "";
        foreach(var pair in record.Data())
        {
            logString += pair.Key + ": " + pair.Value.ToString() + ", ";
        }
        Debug.Log(logString.Substring(0, logString.Length-2));
    }
}
