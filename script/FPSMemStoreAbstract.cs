using UnityEngine;
using System.Collections;

public abstract class FPSMemStoreAbstract : MonoBehaviour
{
    public abstract void Store(FPSMemRecord[] records);
}
