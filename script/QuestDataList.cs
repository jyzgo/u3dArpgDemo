using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class QuestDataList : ScriptableObject
{
	public List<QuestData> dataList = new List<QuestData>();

    public QuestData FindQuestDataByID(int questID)
    {
        return dataList.Find(delegate(QuestData data) { return data.quest_id == questID; });
    }
}

[Serializable]
public class QuestData
{
	public int quest_id;
	public string quest_name;
	public string giver;
	public string description;
    public QuestStatus status;
    public QuestType quest_type;
    public QuestCycleType cycle_type;
    public int start_offset;    //in hours
    public int end_offset;      //in hours
	//public bool guild_only;
	//public int visible_player_level_low;
	//public int visible_player_level_high;
	//public int visible_difficulty_low;
	//public int visible_difficulty_high;
	//public int prequest_id;
    //public int disable_prequest_id;
    public string recommended_level_name;
    //public int recommended_level_difficulty;
    public List<QuestTarget> target_list = new List<QuestTarget>();
	public int reward_exp;
	public int reward_sc;
	public int reward_hc;
    public List<QuestRewardItem> reward_item_list = new List<QuestRewardItem>();
}

public enum QuestType
{
    main,
    side,
}

[Serializable]
public class QuestTarget
{
    public static string[] typeNames = new string[] 
    {
        "IDS_MESSAGE_QUEST_TARGET_PASSLEVEL",      //complete level
        "IDS_MESSAGE_QUEST_TARGET_KILLMONSTER",      //kill monster
        "IDS_MESSAGE_QUEST_TARGET_GETITEM",      //find loot
        "IDS_MESSAGE_QUEST_TARGET_LEVELUP",      //level up
        "IDS_MESSAGE_QUEST_TARGET_FUSIONUP",      //fusion up
        "IDS_MESSAGE_QUEST_TARGET_PURCHASEITEM",      //puchase_item
		"IDS_QUEST_TARGET_TYPE_8",      //join guild
		"IDS_QUEST_TARGET_TYPE_9",      //add a friend	
    };

	public QuestTargetType target_type;
	public string target_id;
    public string target_var1;  //all-purpose field, for target type "complete_level", it means the difficulty level
	public int target_count;
}


[Serializable]
public class QuestRewardItem
{
	public string reward_item_id;
    public EnumRole reward_role;
	public int reward_item_count;
}


public enum QuestTargetType
{
	complete_level,
	kill_monster,
	find_loot,
	level_up,
    fusion_up,
	purchase_item,
	facebook_login,
	join_guild,
	add_friend,
    max  //for check input
}

public enum QuestStatus
{
    //for quest data
    inactive,
    active,

    //for quest progress
    inprogress,
    completed,  //finished but rewards not claimed yet
    closed,     //finished and rewards claimed
}

public enum QuestCycleType
{
    once,
    daily,
    weekly
}

[Serializable]
public class QuestProgress
{
    public int quest_id;

    public List<QuestTargetProgress> target_progress_list;

    public bool viewed; //if user has expanded the quest for details

    private bool? _isCompleted;

	private bool _dataChanged = false;
	public bool dataChanged
	{
		get
		{
			return _dataChanged;
		}
		set
		{
			_dataChanged = value;
		}
	}

    public bool isCompleted
    {
        get
        {
            if (_isCompleted.HasValue) return (bool)_isCompleted;

            _isCompleted = true;

            foreach (QuestTargetProgress qtp in target_progress_list)
            {
                if (qtp.required_amount > qtp.actual_amount)
                {
                    _isCompleted = false;
                    break;
                }
            }
            return (bool)_isCompleted;
        }
        set
        {
            _isCompleted = value;
        }
    }

    private bool? _isMain;

    public bool isMain
    {
        get
        {
            if (_isMain.HasValue)
            {
                return (bool)_isMain;
            }

            QuestData qd = QuestManager.instance.CurrentQuestList.FindQuestDataByID(quest_id);
            _isMain = qd.quest_type == QuestType.main;

            return (bool)_isMain;
        }
    }
}

[Serializable]
public class QuestTargetProgress
{
    public QuestTargetType target_type;
    public string target_id;
    public int required_amount;
    public int actual_amount;
    public string target_var1;      //all purpose field, for type "complete-level", it means the difficulty level required
}