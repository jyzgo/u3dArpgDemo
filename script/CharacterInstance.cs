using UnityEngine;
using System.Collections;

[System.Serializable]
public class CharacterInstance
{
	public int _index;
	public GameObject _inst = null;
	public AvatarController _avatarController;
	public ActionController _actionController;
	
	public void InitCharacter(string label, string nickName)
	{
		InitCharacter(label, nickName, "");
	}
	
	public void InitCharacter(string label, string nickName, string guildName) {
		_inst = CharacterAssembler.Singleton.AssembleCharacter(label);
		_actionController = _inst.GetComponent<ActionController>();
		_avatarController = _inst.GetComponent<AvatarController>();
		_actionController.Init();
		_avatarController.Init(nickName, guildName, -1);
	}
	
	public void Unload() {
		GameObject.Destroy(_inst);
		_avatarController = null;
		_actionController = null;
	}
	
	public virtual void Active(Vector3 pos, float rotationY = 0) {
		if(!_inst.activeSelf) {
			_inst.SetActive(true);
		}
		_actionController._instanceID = _index;
		_actionController.ACSpawn(pos, rotationY);
	}
	
	public void Deactive() {
		_inst.SetActive(false);
		ActionControllerManager.Instance.UnRegister(_actionController);
	}
}

[System.Serializable]
public class EnemyInstance : CharacterInstance
{
	public EnemyInstanceInfo spawnInfo;
	public bool isActive = false;
	public bool isDead = false;

    private EnemySpot _enemySpot;

    public void Active(EnemySpot spot, float rotationY = 0)
    {
        _enemySpot = spot;
		_actionController.AIUse.HaveHpBar = _enemySpot.needHPBar;
		_actionController.AIUse.SlowTimeWhenDead = _enemySpot.needDeathCameraEffect;
		_actionController.AIUse.LockByCamera = _enemySpot.needCameraLock;
		_actionController.AIUse.HaveEnergyBar = false;
		base.Active(spot.transform.position);
		isActive = true;
	}
	
}

[System.Serializable]
public class HeroInstance : CharacterInstance
{
	public string _instLabel;
    public override void Active(Vector3 pos, float rotationY = 0)
	{
        base.Active(pos, rotationY);
		_avatarController.TakeIcon();
	}
}
