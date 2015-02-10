using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

using FaustComm;

[NetCmdID(5001, name = "Login Cmd Send to Server")]
public class LoginRequest : NetRequest
{
	public string account;
	public string password;
	public string os = string.Empty;

	public override void Encode(BinaryWriter writer)
	{
		WriteString(writer, account);  //login request does not have AccountId and SessionId
		WriteString(writer, password);
		WriteString(writer, os);
	}

	public override string ToString()
	{ 
		return "account=\"" + account + "\", password=\"" + password + "\"";
	}
}

[NetCmdID(10001, name = "Login Msg Get from Server")]
public class LoginResponse : NetResponse
{
	public int accountId;
	public int session;
	
	public override void Decode(BinaryReader reader)
	{
		errorCode = reader.ReadInt16();
		
		if (errorCode == 0)
		{
			accountId = reader.ReadInt32();
			session = reader.ReadInt32();
		}
	}
	
}

[NetCmdID(5008, name = "Begin battle")]
public class BeginBattleRequest : NetRequest
{
	public int levelID;

	public override void Encode(BinaryWriter writer)
	{
		writer.Write(levelID);
	}

	public override string ToString()
	{
		return "levelID= " + levelID;
	}
}

[NetCmdID(10008, name = "Begin battle, load enemy config for this level")]
public class BeginBattleResponse : NetResponse
{
	public Dictionary<string, List<EnemyInstanceInfo>> triggerEnemyMapping;
	public Dictionary<StaticObjectType, List<StaticObjectInfo>> staticObjMapping;

	public override void Decode(BinaryReader reader)
	{
		InJoyDebugBase.DebugCommandResponse(this,ref reader);
		errorCode = reader.ReadInt16();

		if (errorCode > 0)
		{
			return;
		}

		triggerEnemyMapping = new Dictionary<string, List<EnemyInstanceInfo>>();

		//enemy
		int count = reader.ReadInt16();

		for (int i = 0; i < count; i++)
		{
			string triggerName = ReadString(reader);

			List<EnemyInstanceInfo> enemyList = new List<EnemyInstanceInfo>();

			int enemyCount = reader.ReadInt16();

			for (int j = 0; j < enemyCount; j++)
			{
				EnemyInstanceInfo info = new EnemyInstanceInfo();

				info.spawnAt = (BornPoint.SpawnPointType)reader.ReadByte();

				info.enemyLabel = ReadString(reader);

				info.delayTime = reader.ReadByte();

				//read loot table

				info.lootTable = Utils.ParseLootList(reader);

				enemyList.Add(info);
			}

			triggerEnemyMapping.Add(triggerName, enemyList);
		}

		//static object
		staticObjMapping = new Dictionary<StaticObjectType, List<StaticObjectInfo>>();

		count = reader.ReadInt16();
		for (int i = 0; i < count; i++)
		{
			StaticObjectType type = (StaticObjectType)reader.ReadByte();

			List<StaticObjectInfo> list;

			if (staticObjMapping.ContainsKey(type))
			{
				list = staticObjMapping [type];
			}
			else
			{
				list = new List<StaticObjectInfo>();
				staticObjMapping.Add(type, list);
			}

			StaticObjectInfo info = new StaticObjectInfo();

			info.type = type;

			info.lootTable = Utils.ParseLootList(reader);

			list.Add(info);
		}
	}
}

[NetCmdID(5012, name = "Get level states")]
public class LevelStatesRequest : NetRequest
{
	public override void Encode(BinaryWriter writer)
	{
	}

	public override string ToString()
	{
		return "Get levels state from server.";
	}
}

[NetCmdID(10012, name = "Get level states")]
public class LevelStatesResponse : NetResponse
{
	public Dictionary<int, int> levelStates;

	public override void Decode(BinaryReader reader)
	{
		InJoyDebugBase.DebugCommandResponse(this,ref reader);
		errorCode = reader.ReadInt16();
		
		if (errorCode > 0)
		{
			return;
		}

		levelStates = new Dictionary<int, int>();

		int count = reader.ReadInt32();

		for (int i = 0; i < count; i++)
		{
			int levelID = reader.ReadInt32();
			int state = reader.ReadByte();
			levelStates.Add(levelID, state);
		}
	}
}

[NetCmdID(5009, name = "Battle end request")]
public class BattleEndRequest : NetRequest
{
	public int levelID;
	public int timeConsumed;
	public EnumLevelState levelState;
	public int exp;
	public List<ItemInventory> itemList;

	public override void Encode(BinaryWriter writer)
	{
		writer.Write(levelID);
		writer.Write(timeConsumed);
		writer.Write((byte)levelState);
		writer.Write(exp);

		writer.Write((byte)itemList.Count);

		foreach (ItemInventory item in itemList)
		{
			WriteString(writer, item.ItemID);
			writer.Write(item.Count);
		}
	}
	
	public override string ToString()
	{
		return "End battle";
	}
}

[NetCmdID(10009, name = "Battle end response")]
public class BattleEndResponse : NetResponse
{
	public Dictionary<int, int> levelStates;

    public UpdateInforResponseData updateData;
	
	public override void Decode(BinaryReader reader)
	{
		InJoyDebugBase.DebugCommandResponse(this,ref reader);
		errorCode = reader.ReadInt16();

		if (errorCode == 0)
		{
			reader.ReadInt32();//level id
			reader.ReadByte(); //level state

			updateData = UpdateInforResponseHandler.Instance.UpdateOperation(reader);
		}
	}
}

[NetCmdID(5015, name = "Abort battle")]
public class BattleAbortRequest : NetRequest
{
	public int levelID;
	public int exp;

	public override void Encode(BinaryWriter writer)
	{
		writer.Write(levelID);
		writer.Write(exp);
	}
	
	public override string ToString()
	{
		return "Abort battle";
	}
}

[NetCmdID(10015, name = "Battle abort response")]
public class BattleAbortResponse : NetResponse
{
    public UpdateInforResponseData updateData;

	public override void Decode(BinaryReader reader)
	{
		InJoyDebugBase.DebugCommandResponse(this,ref reader);
		errorCode = reader.ReadInt16();
		
		if (errorCode == 0)
		{
			updateData = UpdateInforResponseHandler.Instance.UpdateOperation(reader);
		}
	}
}

[NetCmdID(5013, name = "Use item in battle")]
public class BattleUseItemRequest : NetRequest
{
	public int levelID;
	public string itemID;
	public byte count;
	public byte useHC;

	public override void Encode(BinaryWriter writer)
	{
		writer.Write(levelID);
		WriteString(writer, itemID);
		writer.Write(count);
		writer.Write(useHC);
	}

	public override string ToString()
	{
		return "Use item in battle";
	}
}

[NetCmdID(10013, name = "Battle use item response")]
public class BattleUseItemResponse : NetResponse
{
	public UpdateInforResponseData updateData;

	public override void Decode(BinaryReader reader)
	{
		errorCode = reader.ReadInt16();

		if (errorCode == 0)
		{
			updateData = UpdateInforResponseHandler.Instance.UpdateOperation(reader);
		}
	}
}

[NetCmdID(5016, name = "Revive in battle")]
public class BattleUseReviveRequest : NetRequest
{
	public int levelID;
	public string itemID;
	public byte count;
	public byte useHC;

	public override void Encode(BinaryWriter writer)
	{
		writer.Write(levelID);
		WriteString(writer, itemID);
		writer.Write(count);
		writer.Write(useHC);
	}

	public override string ToString()
	{
		return "Revive in battle";
	}
}

[NetCmdID(10016, name = "Battle revive response")]
public class BattleUseReviveResponse : NetResponse
{
	public UpdateInforResponseData updateData;

	public override void Decode(BinaryReader reader)
	{
		errorCode = reader.ReadInt16();

		if (errorCode == 0)
		{
			updateData = UpdateInforResponseHandler.Instance.UpdateOperation(reader);
		}
	}
}

[NetCmdID(5018, name = "Get skill data")]
public class GetSkillRequest : NetRequest
{
	public override void Encode(BinaryWriter writer)
	{
	}

	public override string ToString()
	{
		return "Get skill data";
	}
}

[NetCmdID(10018, name = "Get skill response")]
public class GetSkillResponse : NetResponse
{
	public List<SkillData> skillDataList;

	public override void Decode(BinaryReader reader)
	{
		InJoyDebugBase.DebugCommandResponse(this,ref reader);
		errorCode = reader.ReadInt16();

		if (errorCode == 0)
		{
			skillDataList = new List<SkillData>();

			int count = reader.ReadInt16();

			for (int i = 0; i < count; i++)
			{
				SkillData skillData = new SkillData();

				skillData.skillID = ReadString(reader);

				skillData.level = reader.ReadByte();

				skillDataList.Add(skillData);
			}
		}
	}
}

[NetCmdID(5019, name = "Skill upgrade")]
public class SkillUpgradeRequest : NetRequest
{
	public string skillID;
	public byte useHC;

	public override void Encode(BinaryWriter writer)
	{
		WriteString(writer, skillID);
		writer.Write(useHC);
	}

	public override string ToString()
	{
		return "Skill upgrade";
	}
}

[NetCmdID(10019, name = "Skill upgrade response")]
public class SkillUpgradeResponse : NetResponse
{
	public SkillData skillData;
	public UpdateInforResponseData updateData;

	public override void Decode(BinaryReader reader)
	{
		errorCode = reader.ReadInt16();

		if (errorCode == 0)
		{
			skillData = new SkillData();

			skillData.skillID = ReadString(reader);

			skillData.level = reader.ReadByte();

			updateData = UpdateInforResponseHandler.Instance.UpdateOperation(reader);
		}
	}
}

//tattoos
[NetCmdID(5020, name = "Tattoo data request")]
public class PlayerTattooRequest : NetRequest
{
	public override void Encode(BinaryWriter writer)
	{
	}

	public override string ToString()
	{
		return "Tattoo of player";
	}
}

[NetCmdID(10020, name = "Tattoo data response")]
public class PlayerTattooResponse : NetResponse
{
	public PlayerTattoos playerTattoos;

	public List<string> recipeList;

	public override void Decode(BinaryReader reader)
	{
		InJoyDebugBase.DebugCommandResponse(this,ref reader);
		errorCode = reader.ReadInt16();

		if (errorCode == 0)
		{
			//tattoos bunred on body
			playerTattoos = new PlayerTattoos();

			playerTattoos.Parse(reader);

			//tattoo recipes mastered
			recipeList = new List<string>();

			int count = reader.ReadInt16();
			for (int i = 0; i < count; i++)
			{
				recipeList.Add(ReadString(reader));
			}
		}
	}
}

[NetCmdID(5021, name = "tattoo burn request")]
public class TattooEquipRequest : NetRequest
{
	private EnumTattooPart _part;
	private long _guid;
	private byte _op;	//1: burn on   2: remove

	public TattooEquipRequest(EnumTattooPart part, long guid, byte op)
	{
		_part = part;
		_guid = guid;
		_op = op;
	}

	public override void Encode(BinaryWriter writer)
	{
		writer.Write((byte)_part);
		writer.Write(_guid);
		writer.Write(_op);
	}

	public override string ToString()
	{
		return "Burn tattoo on body";
	}
}

[NetCmdID(10021, name = "Burn tattoo response")]
public class TattooEquipResponse : NetResponse
{
	public UpdateInforResponseData updateData;

	public override void Decode(BinaryReader reader)
	{
		errorCode = reader.ReadInt16();

		if (errorCode == 0)
		{
			updateData = UpdateInforResponseHandler.Instance.UpdateOperation(reader);
		}
	}
}


[NetCmdID(5022, name = "tattoo recipe request")]
public class TattooRecipeRequest : NetRequest
{
	private string _tattooID;
	private byte _op;	//1: make   2: activate
	private byte _useHC;

	public TattooRecipeRequest(string tattooID, byte op, byte useHC)
	{
		_tattooID = tattooID;
		_op = op;
		_useHC = useHC;
	}

	public override void Encode(BinaryWriter writer)
	{
		WriteString(writer, _tattooID);
		writer.Write(_op);
		writer.Write(_useHC);
	}

	public override string ToString()
	{
		return "Tattoo making and learning";
	}
}

[NetCmdID(10022, name = "Tattoo making and learning response")]
public class TattooRecipeResponse : NetResponse
{
	public UpdateInforResponseData updateData;

	public string tattooID;

	public byte op;

	public override void Decode(BinaryReader reader)
	{
		errorCode = reader.ReadInt16();

		if (errorCode == 0)
		{
			tattooID = ReadString(reader);

			op = reader.ReadByte();

			updateData = UpdateInforResponseHandler.Instance.UpdateOperation(reader);
		}
	}
}

[NetCmdID(5024, name = "Get quest")]
public class GetQuestRequest : NetRequest
{
	public override void Encode(BinaryWriter writer)
	{
	}

	public override string ToString()
	{
		return "Get quest";
	}
}

[NetCmdID(10024, name = "Get quest response")]
public class GetQuestResponse : NetResponse
{
	public List<QuestProgress> qpList;

	public override void Decode(BinaryReader reader)
	{
		InJoyDebugBase.DebugCommandResponse(this,ref reader);
		errorCode = reader.ReadInt16();

		if (errorCode == 0)
		{
			qpList = new List<QuestProgress>();

			int count = reader.ReadByte();

			for (int i = 0; i < count; i++)
			{
				QuestProgress qp = new QuestProgress();

				qp.quest_id = reader.ReadInt32();

				qp.target_progress_list = new List<QuestTargetProgress>();

				QuestTargetProgress qtp = new QuestTargetProgress();

				qtp.actual_amount = reader.ReadInt32();

				qp.target_progress_list.Add(qtp);

				qtp = new QuestTargetProgress();

				qtp.actual_amount = reader.ReadInt32();

				qp.target_progress_list.Add(qtp);

				qpList.Add(qp);
			}
		}
	}
}

[NetCmdID(5025, name = "Send quest progresses")]
public class SendQuestProgressRequest : NetRequest
{
	private List<int> _progressList;

	public SendQuestProgressRequest(List<int> list)
	{
		_progressList = list;
	}

	public override void Encode(BinaryWriter writer)
	{
		writer.Write((byte)(_progressList.Count / 3));

		foreach (int i in _progressList)
		{
			writer.Write(i);
		}
	}

	public override string ToString()
	{
		return "Send quest progresses";
	}
}

[NetCmdID(10025, name = "Send quest progress response")]
public class SendQuestProgressResponse : NetResponse
{
	public override void Decode(BinaryReader reader)
	{
		errorCode = reader.ReadInt16();
	}
}

[NetCmdID(5026, name = "Claim quest rewards")]
public class ClaimQuestRewardRequest : NetRequest
{
	private int _questID;

	public ClaimQuestRewardRequest(int questID)
	{
		_questID = questID;
	}

	public override void Encode(BinaryWriter writer)
	{
		writer.Write(_questID);
	}

	public override string ToString()
	{
		return "Claim quest rewards";
	}
}

[NetCmdID(10026, name = "Claim quest reward response")]
public class ClaimQuestRewardResponse : NetResponse
{
	public List<int> questIDList;

	public UpdateInforResponseData updateData;

	public override void Decode(BinaryReader reader)
	{
		errorCode = reader.ReadInt16();

		if (errorCode == 0)
		{
			questIDList = new List<int>();
			int count = reader.ReadByte();

			for (int i = 0; i < count; i++)
			{
				questIDList.Add(reader.ReadInt32());
			}

			updateData = UpdateInforResponseHandler.Instance.UpdateOperation(reader);
		}
	}
}

[NetCmdID(5035, name = "Get players in town")]
public class GetTownPlayersRequest : NetRequest
{
	private byte _maxNum;
	public GetTownPlayersRequest(byte maxNum)
	{
		_maxNum = maxNum;
	}

	public override void Encode(BinaryWriter writer)
	{
		writer.Write(_maxNum);
	}

	public override string ToString()
	{
		return "Get town players";
	}
}

[NetCmdID(10035, name = "Get town players response")]
public class GetTownPlayersResponse : NetResponse
{
	public List<PlayerInfo> playerInfoList;

	public override void Decode(BinaryReader reader)
	{
		errorCode = reader.ReadInt16();

		if (errorCode == 0)
		{
			playerInfoList = new List<PlayerInfo>();
			int count = reader.ReadByte();

			for (int i = 0; i < count; i++)
			{
				PlayerInfo pi = new PlayerInfo();

				pi.Parse(reader);

				playerInfoList.Add(pi);
			}
		}
	}
}

[NetCmdID(5036, name = "Buy vitality")]
public class BuyVitalityRequest : NetRequest
{
	public override void Encode(BinaryWriter writer)
	{
	}

	public override string ToString()
	{
		return "Buy vitality";
	}
}

[NetCmdID(10036, name = "Get town players response")]
public class BuyVitalityResponse : NetResponse
{
	public UpdateInforResponseData updateData;

	public override void Decode(BinaryReader reader)
	{
		errorCode = reader.ReadInt16();

		if (errorCode == 0)
		{
			updateData = UpdateInforResponseHandler.Instance.UpdateOperation(reader);
		}
	}
}

[NetCmdID(5038, name = "Get tutorial states")]
public class GetTutorialStateRequest : NetRequest
{
	public override void Encode(BinaryWriter writer)
	{
	}

	public override string ToString()
	{
		return "Get tutorial state";
	}
}

[NetCmdID(10038, name = "Get town players response")]
public class GetTutorialStateResponse : NetResponse
{
	public Dictionary<EnumTutorial, EnumTutorialState> tutorialState;
	public override void Decode(BinaryReader reader)
	{
		errorCode = reader.ReadInt16();

		if (errorCode == 0)
		{
			int count = reader.ReadInt16();
	
			tutorialState = new Dictionary<EnumTutorial, EnumTutorialState>();

			for (int i = 0; i < count; i++)
			{
				EnumTutorial id = (EnumTutorial)reader.ReadInt32();

				EnumTutorialState state = (EnumTutorialState)reader.ReadInt32();

				tutorialState.Add(id, state);
			}
		}
	}
}

[NetCmdID(5037, name = "Save tutorial states")]
public class SaveTutorialStateRequest : NetRequest
{
	private Dictionary<EnumTutorial, EnumTutorialState> _tutorialState;
	public SaveTutorialStateRequest(Dictionary<EnumTutorial, EnumTutorialState> tutorialState)
	{
		_tutorialState = tutorialState;
	}

	public override void Encode(BinaryWriter writer)
	{
		writer.Write((short)_tutorialState.Count);
		foreach (KeyValuePair<EnumTutorial, EnumTutorialState> kvp in _tutorialState)
		{
			writer.Write((int)kvp.Key);
			writer.Write((int)kvp.Value);
		}
	}

	public override string ToString()
	{
		return "Save tutorial state";
	}
}

[NetCmdID(10037, name = "Save tutorial state dictionary")]
public class SaveTutorialStateResponse : NetResponse
{
	public override void Decode(BinaryReader reader)
	{
		errorCode = reader.ReadInt16();

		if (errorCode == 0)
		{
		}
	}
}

