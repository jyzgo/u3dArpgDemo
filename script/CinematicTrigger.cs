using UnityEngine;
using System.Collections.Generic;
using System;

public class CinematicTrigger : MonoBehaviour
{
	public Animation[] cameraAnimations;

	public EventBroker eventBroker;	//will send messages to this gameObject

	private Transform _cameraParent;

	private Transform _cameraTrans;

    private ActionController _player;

	private void StartCinematic()
	{
		GameManager.Instance.GameState = EnumGameState.InBattleCinematic;

		StopAllCharacters();

		StartCoroutine(DisableBattleUI());

		this.animation.Play();

		if (eventBroker != null)
		{
			eventBroker.OnCinematicStart();
		}
	}

	private void EndCinematic()
	{
		StopCameraMove();
		EnableAllCharacters();
		StartCoroutine(EnableBattleUI());

		this.collider.enabled = false;
		this.enabled = false;
	
		GameManager.Instance.GameState = EnumGameState.InBattle;

		if (eventBroker != null)
		{
			eventBroker.OnCinematicEnd();
		}
	}


    private void StopAllCharacters()
    {
        List<FCObject> clients = InputManager.Instance.Clients;

        foreach (FCObject client in clients)
        {
            CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
                    FCConst.FC_KEY_DIRECTION, FC_PARAM_TYPE.INT,
                    client.ObjectID, FCCommand.STATE.RIGHTNOW, true);

            CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
                    FCConst.FC_KEY_ATTACK_1, FC_PARAM_TYPE.INT,
                    client.ObjectID, FCCommand.STATE.RIGHTNOW, true);

            CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
                    FCConst.FC_KEY_ATTACK_2, FC_PARAM_TYPE.INT,
                    client.ObjectID, FCCommand.STATE.RIGHTNOW, true);

            CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
                    FCConst.FC_KEY_ATTACK_3, FC_PARAM_TYPE.INT,
                    client.ObjectID, FCCommand.STATE.RIGHTNOW, true);

            CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
                    FCConst.FC_KEY_ATTACK_4, FC_PARAM_TYPE.INT,
                    client.ObjectID, FCCommand.STATE.RIGHTNOW, true);

            CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
                    FCConst.FC_KEY_ATTACK_5, FC_PARAM_TYPE.INT,
                    client.ObjectID, FCCommand.STATE.RIGHTNOW, true);

            CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
                    FCConst.FC_KEY_ATTACK_6, FC_PARAM_TYPE.INT,
                    client.ObjectID, FCCommand.STATE.RIGHTNOW, true);

            CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
                    FCConst.FC_KEY_ATTACK_7, FC_PARAM_TYPE.INT,
                    client.ObjectID, FCCommand.STATE.RIGHTNOW, true);

            CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
                    FCConst.FC_KEY_ATTACK_8, FC_PARAM_TYPE.INT,
                    client.ObjectID, FCCommand.STATE.RIGHTNOW, true);

            CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
                    FCConst.FC_KEY_ATTACK_9, FC_PARAM_TYPE.INT,
                    client.ObjectID, FCCommand.STATE.RIGHTNOW, true);

            CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
                    FCConst.FC_KEY_ATTACK_10, FC_PARAM_TYPE.INT,
                    client.ObjectID, FCCommand.STATE.RIGHTNOW, true);
        }
    }

	private void EnableAllCharacters()
	{
	}

	void StartCameraMove(int index)
	{
		_cameraTrans = Camera.main.transform;

		_cameraParent = _cameraTrans.parent;

		_cameraTrans.parent = cameraAnimations[index].transform;

		_cameraTrans.localPosition = Vector3.zero;

		_cameraTrans.localRotation = Quaternion.identity;

		cameraAnimations[index].Play();
	}

	void StopCameraMove()
	{
		//resume camera
		_cameraTrans.parent = _cameraParent;

		_cameraTrans.localRotation = Quaternion.identity;

		_cameraTrans.localPosition = Vector3.zero;
	}

	void StartDialog(string id)
	{
		UIBattleDialogManager.Instance.ShowDialog(id);
	}

	void StopDialog(string id)
	{
		UIBattleDialogManager.Instance.CloseDialog(id);
	}

	void StartTutorial(int id)
	{
	}

	void StopTutorial(int id)
	{
	}

	void OnTriggerEnter()
	{
		StartCinematic();
	}

	void OnGUI()
	{
		//if (GUI.Button(new Rect(10, 10, 100, 40), "Test event"))
		//{
		//    this.animation.Play();
		//}
	}

	private System.Collections.IEnumerator DisableBattleUI()
	{
		UIManager.Instance.OpenUI("CutSceneUI");
		yield return new WaitForEndOfFrame();
		UIManager.Instance.CloseUI("HomeUI");
		UIManager.Instance.CloseUI("BossUI");
	}

	private System.Collections.IEnumerator EnableBattleUI()
	{
		UIManager.Instance.OpenUI("HomeUI");
		UIManager.Instance.OpenUI("BossUI");
		yield return new WaitForEndOfFrame();
		UIManager.Instance.CloseUI("CutSceneUI");
	}


    private void PlayerEnterDummyTaskChange()
    {
        _player = ObjectManager.Instance.GetMyActionController();

        if (null != _player)
        {
            _player.AIUse.SetNextState(AIAgent.STATE.DUMMY);
        }
    }

    private void PlayerQuitDummyTaskChange()
    {
        if (null != _player)
        {
            _player.AIUse.DummyTaskChange(FCCommand.CMD.STATE_QUIT);
        }
    }

    private void EnemyQuitDummyTaskChange()
    {
        List<ActionController> enemyList = ObjectManager.Instance.GetEnemyActionController();

        foreach (ActionController enemy in enemyList)
        {
            if (null != enemy && null != enemy.AIUse && enemy.AIUse.InState(AIAgent.STATE.DUMMY))
            {
                enemy.AIUse.DummyTaskChange(FCCommand.CMD.STATE_QUIT);
            }
        }

        enemyList.Clear();
    }
}
