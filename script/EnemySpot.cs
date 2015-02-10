using UnityEngine;
using System;


/// <summary>
/// helper for BornPoint to remember the positions of possibile born places
/// </summary>
public class EnemySpot : MonoBehaviour
{
    public BornPoint.SpawnPointType acceptedSpawnType;

    public bool needHPBar = false;      //need to display a huge HP bar for it?

    public bool needDeathCameraEffect = false;      //need to play the slow motion camera effect when killed?

    public bool needCameraLock;     //need to lock camera to this enemy?
}
