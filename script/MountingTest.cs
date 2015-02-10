using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//This file is for test only and is to be removed later on.
public class MountingTest : MonoBehaviour
{
    public GameObject[] _equipList1;
    public GameObject[] _equipList2;
    public GameObject[] _equipList3;

    private EquipmentsList _equipList;

    private AvatarController _avatarController;

    private EquipmentsAgent _agent;

    private Transform _equipmentRoot;

    void Start()
    {
        _agent = GetComponent<EquipmentsAgent>();

        _equipList = _agent._equipmentsAgent.GetComponent<EquipmentsList>();

        _avatarController = transform.parent.GetComponent<AvatarController>();

        _equipmentRoot = _agent._equipmentsAgent.transform;
    }

    public IEnumerator ChangeSuit()
    {
        GameObject[] equipList = null;

        if (_equipList._equipList == _equipList3)
        {
            equipList = _equipList1;
        }
        else if (_equipList._equipList == _equipList1)
        {
            equipList = _equipList2;
        }
        else if (_equipList._equipList == _equipList2)
        {
            equipList = _equipList3;
        }
        else
        {
            equipList = _equipList1;
        }

        _equipList._equipList = equipList;

        foreach (Transform t in _equipmentRoot)
        {
            Destroy(t.gameObject);
        }

        yield return null;

        foreach (GameObject go in equipList)
        {
            Utils.InstantiateGameObjectWithParent(go, _equipmentRoot);
        }

        _avatarController.RefreshEquipments(_equipmentRoot);
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width - 200, 50, 150, 60));
        if (GUILayout.Button("Change suit"))
		{
			StartCoroutine(ChangeSuit());
		}
        GUILayout.EndArea();
    }
}