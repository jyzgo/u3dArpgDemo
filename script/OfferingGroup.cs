using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class SingleGroup
{
    public int count;
    public string item;
}

[Serializable]
public class OfferingGroup
{
    public string groupId;
    public List<SingleGroup> groupList = new List<SingleGroup>();
}

