using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class FPSMemDataAbstract : MonoBehaviour
{
    public void Collect(FPSMemRecord record)
    {
        Fill(record);
    }

    protected abstract void Fill(FPSMemRecord record);
}
