using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StaticObjectLoot : MonoBehaviour
{
	public string soundEffect = "wood_hit";

    public List<Vector3> lootOffest = new List<Vector3>();

    public LOOTTYPE lootType = LOOTTYPE.RANDOM;

    public List<LootObjData> lootTable;

    public void Loot()
    {
		TutorialManager.Instance.ReceiveFinishTutorialEvent(EnumTutorial.Battle_Attack);
		
		SoundManager.Instance.PlaySoundEffect(soundEffect);
        LootManager.Instance.Loot(lootTable, transform.position,transform.forward, lootType, lootOffest);
    }
}
