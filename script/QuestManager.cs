using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using InJoy.FCComm;
using InJoy.Utils;

public class QuestManager : MonoBehaviour
{
    public string questListPath;

    private QuestDataList _questDataList;
	private QuestDataList _curQuestDataList;
	
    public QuestDataList CurrentQuestList
	{
		get
		{
			if(_curQuestDataList == null)
			{
				_curQuestDataList = _questDataList;
			}
			return _curQuestDataList; 
		} 
	}

    private static QuestManager _instance;

    public static QuestManager instance
    {
        get { return _instance; }
    }

    private List<int> _activeQuestIDList = new List<int>();   //a link from player profile

    private List<QuestProgress> _questProgressList = new List<QuestProgress>();

    public List<QuestProgress> UserQuestList
    {
        get
        {
            QPComparer qpc = new QPComparer();
            _questProgressList.Sort(qpc);
            return _questProgressList;
        }
    }

    //last time to acquire active quest list
	private DateTime _lastSyncTime = TimeUtils.k_epoch_time;

    public bool HasSyncedWithServer
    {
        get
        {
            TimeSpan span = NetworkManager.Instance.serverTime - _lastSyncTime;

            return span.TotalMinutes < 60;
        }
    }

    public bool HasQuestID(int questID)
    {
        return _activeQuestIDList.IndexOf(questID) >= 0;
    }

    void Awake()
    {
		if(_instance != null)
		{
			Debug.LogError("QuestProgress: detected singleton instance has existed. Destroy this one " + gameObject.name);
			Destroy(this);
			return;
		}				
		
        _instance = this;
        _questDataList = InJoy.AssetBundles.AssetBundles.Load(questListPath) as QuestDataList;

        Assertion.Check(_questDataList != null);
    }
	
	void OnDestroy() {
		if(_instance == this)
		{
			_instance = null;
		}
	}

	private Action _callbackAfterGetQuests;
    public void CheckToActivateQuests(Action callback = null)
    {
		_lastSyncTime = TimeUtils.k_epoch_time;

        _callbackAfterGetQuests = callback;

        //request latest active quest
		ConnectionManager.Instance.RegisterHandler(ServerGetActiveQuests, true);
    }

    private void ServerGetActiveQuests()
    {
		NetworkManager.Instance.SendCommand(new GetQuestRequest(), OnGetQuest);
	}

	private void OnGetQuest(FaustComm.NetResponse msg)
	{
		ConnectionManager.Instance.SendACK(ServerGetActiveQuests, true);

		if (msg.Succeeded)
		{
			_lastSyncTime = NetworkManager.Instance.serverTime;

			List<QuestProgress> qpList = (msg as GetQuestResponse).qpList;

			FillQuestProgress(qpList);

			_questProgressList = qpList;

			if (_callbackAfterGetQuests != null)
			{
				_callbackAfterGetQuests();

				_callbackAfterGetQuests = null;
			}
		}
		else
		{
			UIMessageBoxManager.Instance.ShowMessageBox(Utils.GetErrorIDS(msg.errorCode), null, MB_TYPE.MB_OK, null);
		}
	}

	//Fill the missing fields
	private void FillQuestProgress(List<QuestProgress> qpList)
	{
		foreach (QuestProgress qp in qpList)
		{
			QuestData qd = CurrentQuestList.FindQuestDataByID(qp.quest_id);

			qp.viewed = Utils.IsViewedQuest(qp.quest_id);

			int index = 0;

			foreach (QuestTargetProgress qtp in qp.target_progress_list)
			{
				if (index < qd.target_list.Count)
				{
					qtp.required_amount = qd.target_list[index].target_count;
					qtp.target_id = qd.target_list[index].target_id;
					qtp.target_type = qd.target_list[index].target_type;
					qtp.target_var1 = qd.target_list[index].target_var1;
				}
				else
				{
					qp.target_progress_list.Remove(qtp);
					break;
				}
				index++;
			}
		}
	}

    public void ApplyActiveQuestIdList(List<int> questIDList)
    {
        _activeQuestIDList.Clear();

		foreach (int questID in questIDList)
        {
            _activeQuestIDList.Add(questID);
        }

        this.ApplyActiveQuestList(_activeQuestIDList, _questProgressList);

        this.RefreshSyncTime();
    }

    //check all active quests to see if any quest can be updated by the event
    public void UpdateQuests(QuestTargetType targetType, string targetID, int targetAmount)
    {
        foreach (QuestProgress questProgress in _questProgressList)
        {
            bool completed = true;  //are all targets achieved ?

            foreach (QuestTargetProgress targetProgress in questProgress.target_progress_list)
            {
                if (targetProgress.target_type == targetType && targetProgress.actual_amount < targetProgress.required_amount)
                {
                    switch (targetProgress.target_type)
                    {
                        case QuestTargetType.complete_level:
                            if (targetID == targetProgress.target_id)
                            {
                                targetProgress.actual_amount++;

								questProgress.dataChanged = true;
                            }
                            break;

                        case QuestTargetType.fusion_up:
                            int targetLevel;
                            if (!Int32.TryParse(targetProgress.target_id, out targetLevel))
                            {
                                targetLevel = -1;
                            }

                            //-1 means an upgrade to any level counts
                            if (targetLevel < 0)
                            {
                                targetProgress.actual_amount++;
								questProgress.dataChanged = true;
							}
                            else
                            {
                                //> 0: means only upgrades to a certain level counts
                                if (targetLevel == int.Parse(targetID))
                                {
                                    targetProgress.actual_amount++;
									questProgress.dataChanged = true;
								}
                            }
                            break;

                        case QuestTargetType.level_up:
                            //target id and amount are not used here
                            int level = PlayerInfo.Instance.CurrentLevel;

                            if (level <= targetProgress.required_amount)
                            {
                                targetProgress.actual_amount = level;
								questProgress.dataChanged = true;
							}
                            else
                            {
                                targetProgress.actual_amount = targetProgress.required_amount;
								questProgress.dataChanged = true;
							}
                            break;

                        default:
                            //compare target id for other quest types
                            if (targetProgress.target_id == targetID)
                            {
                                targetProgress.actual_amount = Mathf.Clamp(targetProgress.actual_amount + targetAmount, 0, targetProgress.required_amount);
								questProgress.dataChanged = true;
							}
                            break;
                    } //switch
                    Debug.Log(string.Format("Quest updated. ID = {0}  amount = {1}/{2}", questProgress.quest_id, targetProgress.actual_amount, targetProgress.required_amount));
                }

                if (completed && targetProgress.actual_amount < targetProgress.required_amount)
                {
                    completed = false;
                }
            } //foreach

            questProgress.isCompleted = completed;
        } //foreach

		if (GameManager.Instance.GameState == EnumGameState.InTown)
		{
			SaveQuestProgress();
		}
    }

	public void SaveQuestProgress()
	{
		//prepare the progress list
		List<int> list = new List<int>();
		foreach (QuestProgress qp in _questProgressList)
		{
			if (qp.dataChanged)
			{
				list.Add(qp.quest_id);

				list.Add(qp.target_progress_list[0].actual_amount);

				if (qp.target_progress_list.Count > 1)
				{
					list.Add(qp.target_progress_list[0].actual_amount);
				}
				else
				{
					list.Add(0);
				}
			}
		}

		if (list.Count > 0)
		{
			NetworkManager.Instance.SendCommand(new SendQuestProgressRequest(list), OnSendQuestProgress);
		}
	}

	private void OnSendQuestProgress(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			foreach (QuestProgress qp in _questProgressList)
			{
				qp.dataChanged = false;
			}
		}
		else
		{
			//do nothing, leave the dataChange field unchanged.
		}

	}

    private QuestProgress GetUserQuestByID(int questID)
    {
        return _questProgressList.Find(delegate(QuestProgress qp) { return qp.quest_id == questID; });
    }


    //order: status completed, quest type, quest id
    private class QPComparer : IComparer<QuestProgress>
    {
        public int Compare(QuestProgress x, QuestProgress y)
        {
            if (x.isCompleted && !y.isCompleted)
            {
                return -1;
            }
            else if (!x.isCompleted && y.isCompleted)
            {
                return 1;
            }
            else //both completed, or both not completed, compare quest_id
            {
                if (x.isMain && !y.isMain)
                {
                    return -1;
                }
                else if (!x.isMain && y.isMain)
                {
                    return 1;
                }
                else //same quest type
                {
                    if (x.quest_id < y.quest_id)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }
        }
    }

    public int GetCompletedQuestCount()
    {
        int count = 0;
        foreach (QuestProgress qp in _questProgressList)
        {
            if (qp.isCompleted)
            {
                count++;
            }
        }
        return count;
    }


    public int GetNewQuestCount()
    {
        int count = 0;
        foreach (QuestProgress qp in _questProgressList)
        {
            if (!qp.viewed && !qp.isCompleted)
            {
                count++;
            }
        }
        return count;
    }

    private void ApplyActiveQuestList(List<int> questIDList, List<QuestProgress> questProgressList)
    {
        Debug.Log(string.Format("======== Applying active quest... Active quest count: {0}\t\tQuest progress count: {1}", questIDList.Count, questProgressList.Count));

        //search for removed quests
        List<QuestProgress> removeList = new List<QuestProgress>();

        foreach (QuestProgress qp in questProgressList)
        {
            if (questIDList.IndexOf(qp.quest_id) < 0)   //this quest has been removed on server
            {
                removeList.Add(qp);
            }
        }

        foreach (QuestProgress qp in removeList)
        {
            questProgressList.Remove(qp);
        }

        if (removeList.Count > 0)
        {
            Debug.Log(string.Format("\t\tOut of date quests found and removed: {0}", removeList.Count));
        }

        //search for new quests
        int newQuestCount = 0;
        foreach (int questID in questIDList)
        {
            QuestProgress questProgress = questProgressList.Find(delegate(QuestProgress qp) { return qp.quest_id == questID; });

            if (questProgress == null) //a new quest is found
            {
                QuestData qd = CurrentQuestList.FindQuestDataByID(questID);

                questProgress = new QuestProgress();

                questProgress.quest_id = qd.quest_id;

                questProgress.viewed = false;

                List<QuestTargetProgress> qtpList = new List<QuestTargetProgress>();

                foreach (QuestTarget target in qd.target_list)
                {
                    QuestTargetProgress qtp = new QuestTargetProgress();
                    qtp.target_type = target.target_type;
                    qtp.target_id = target.target_id;
                    qtp.actual_amount = 0;
                    qtp.required_amount = target.target_count;
                    qtp.target_var1 = target.target_var1;

                    qtpList.Add(qtp);
                }

                questProgress.target_progress_list = qtpList;

                questProgressList.Add(questProgress);
				
                Debug.Log(string.Format("\t\tNew quest added: {0}", questProgress.quest_id));

                newQuestCount++;
            }
        }

        UpdateQuests(QuestTargetType.level_up, null, 0);

        if (newQuestCount > 0)
        {
            Debug.Log(string.Format("\t\tNumber of newly added quests: {0}", newQuestCount));
        }

        Debug.Log(string.Format("======== Active quests applied. Active quest count: {0}\t\tQuest progress count: {1}", questIDList.Count, questProgressList.Count));
    }

    //record the time when active quest list is obtained
    public void RefreshSyncTime()
    {
        _lastSyncTime = NetworkManager.Instance.serverTime;
    }

    //called when hero is switched
    public void ResetSyncTime()
    {
        _lastSyncTime = TimeUtils.k_epoch_time;
    }  
    


#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public void CheatQuest(string quest_id)
    {
        Utils.CustomGameServerMessage(null, OnServerCheatQuest);
    }

    private void OnServerCheatQuest(FaustComm.NetResponse response)
    {
    }

#endif  //CHEAT 
}