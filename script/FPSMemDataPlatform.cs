using UnityEngine;
using System.Collections;

public class FPSMemDataPlatform : FPSMemDataAbstract
{
    protected override void Fill(FPSMemRecord record)
    {
        record.Add("platform", Application.platform);
    }
}
