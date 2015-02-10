using UnityEngine;
using System.Collections;

public class CharacterAssembler : MonoBehaviour
{

	public CharacterTable _characterTable;

	private static CharacterAssembler _inst;
	public static CharacterAssembler Singleton
	{
		get { return _inst; }
	}

	void Awake()
	{
		if (_inst != null)
		{
			Debug.LogError("CharacterDictionary: detected singleton instance has existed. Destroy this one " + gameObject.name);
			Destroy(this);
			return;
		}

		_inst = this;

		_activedCharacters = new System.Collections.Generic.Dictionary<string, CharacterInformation>();
	}

	void OnDestroy()
	{
		if (_inst == this)
		{
			_inst = null;
		}
	}

	System.Collections.Generic.Dictionary<string, CharacterInformation> _activedCharacters;

	public bool AddCharacterToLoad(string label)
	{
		if (_activedCharacters.ContainsKey(label))
		{
			return true;
		}

		CharacterInformation ci = _characterTable.FindCharacterByLabel(label);
		if (ci != null)
		{
			_activedCharacters.Add(label, ci);
			return true;
		}

		return false;
	}

	public void ClearAll()
	{
		_activedCharacters.Clear();
	}

	public GameObject AssembleCharacter(string label)
	{
		AcData data = DataManager.Instance.CurAcDataList.Find(label);

		if (data == null)
		{
			Debug.LogError("can't find ac data: [" + label + "]");
			return null;
		}

		AddCharacterToLoad(data.characterId);


		CharacterInformation ci = null;
		if (_activedCharacters.TryGetValue(data.characterId, out ci))
		{
			// prepare ai module prefab.
			GameObject aiModule = InJoy.AssetBundles.AssetBundles.Load(ci.aiAgentPath) as GameObject;
			if (aiModule != null)
			{
				// instance a graphics model.
				GameObject model = CharacterFactory.Singleton.AssembleCharacter(ci.modelPath);
				if (model != null)
				{
					GameObject ai = GameObject.Instantiate(aiModule) as GameObject;
					// assemble ActionController
					ActionController ac = model.GetComponent<ActionController>();
					if (ac == null)
					{
						ac = model.AddComponent<ActionController>();
					}
					//ac._data = ci.extraData.Duplicate();
					ac.Data = data;
					ac._agent = ai;
					ai.transform.parent = model.transform;
					ai.transform.localPosition = Vector3.zero;
					ai.transform.localRotation = Quaternion.identity;
					ai.transform.localScale = Vector3.one;

					//trace all colliders in children
					//if I find the Damage collider, regist it
					bool hasFindDamageCollider = false;

					Collider[] colliders = model.GetComponentsInChildren<Collider>();
					foreach (Collider collider in colliders)
					{
						if (collider.gameObject.name == "DamageCollider")
						{
							hasFindDamageCollider = true;
							ActionControllerManager.Instance.RegisterACByCollider(ac, collider);
						}
					}

					//no damage collider found
					//register the collider in the root.
					if (!hasFindDamageCollider)
					{
						Collider colliderInRoot = model.GetComponent<Collider>();
						if ((colliderInRoot != null) && (ActionControllerManager.Instance != null))
						{
							ActionControllerManager.Instance.RegisterACByCollider(ac, colliderInRoot);
						}
					}

					// navMesh component settings.
					NavMeshAgent nma = model.GetComponentInChildren<NavMeshAgent>();
					MoveAgent[] agents = model.GetComponentsInChildren<MoveAgent>();
					foreach (MoveAgent ma in agents)
					{
						ma._navAgent = nma;
					}

					return model;
				}
				else
				{
					Debug.LogError(string.Format("[CharacterAssembler] Model not found: \"{0}\"", ci.modelPath));
				}
			}
			else
			{
				Debug.LogError(string.Format("[CharacterAssembler] Agent path not found: \"{0}\"", ci.aiAgentPath));
			}
		}
		else
		{
			Debug.LogError(string.Format("[CharacterAssembler] Character id not found: \"{0}\"", data.characterId));
		}

		return null;
	}

	public GameObject AssembleCharacterWithoutAI(string label)
	{
		AddCharacterToLoad(label);
		CharacterInformation ci = null;

		if (_activedCharacters.TryGetValue(label, out ci))
		{
			GameObject model = CharacterFactory.Singleton.AssembleCharacter(ci.modelPath);

			return model;
		}
		return null;
	}
}