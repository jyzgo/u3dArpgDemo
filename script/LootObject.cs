using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LootObject : MonoBehaviour
{

    private BoxCollider _collider;

    private string _lootId;
    public string LootId
    {
        get { return _lootId; }
        set { _lootId = value; }
    }
    private int _lootCount;
    public int LootCount
    {
        get { return _lootCount; }
        set { _lootCount = value; }
    }


    private GameObject _partcileEffect = null;
    public GameObject PartcileEffect
    {
        set { _partcileEffect = value; }
    }

//    private float _alpha = 1;   //transparency

//    private float _time = 0;    //in seconds

 //   private float _duration = 0.2f;

  //  private Renderer _renderer;

    void OnDestroy()
    {
        RealDestroy();
    }

    void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        _collider.enabled = false;
    }

    public void StartPickup(float liftTime)
    {
        _collider.enabled = true;
        StartCoroutine(DelayDestroy(liftTime));
    }

    void Start()
    {
        //_renderer = transform.FindChild("Cube").GetComponent<Renderer>();
    }

    void OnTriggerEnter(Collider other)
    {
        ActionController ac = other.gameObject.GetComponent<ActionController>();
        if (ac != null && ac.IsPlayerSelf)
        {
            OnCollect();
        }
    }


    void OnCollect()
    {
        AddToCombatInventory();
        QuestManager.instance.UpdateQuests(QuestTargetType.find_loot, _lootId, _lootCount);
        StopAllCoroutines();

        RealDestroy();
    }

    void AddToCombatInventory()
    {
        if (_lootId == "money")
        {
            float sc_add = ObjectManager.Instance.GetMyActionController().Data.TotalScAddition; ;
			int moneyCount = (int)(_lootCount  *(1 + sc_add));
            //PlayerInfoManager.Singleton.AccountProfile.AddSoftCurrency(moneyCount);

            if (ObjectManager.Instance != null)
            {
                string textFormat = Localization.instance.Get("IDS_MESSAGE_COMBAT_GET_SC");// "[ffffff]+{0} sc[-]";
                string message = string.Format(textFormat, moneyCount);
                ObjectManager.Instance.GetMyActionController().ACShowPickUpMoney(message, true);
				
				BattleSummary.Instance.CoinsEarned+=moneyCount;
            }

            SoundManager.Instance.PlaySoundEffect("money_loot");
			
			//TutorialManager.Instance.ShowSc();
        }
        else
        {
            SoundManager.Instance.PlaySoundEffect("item_loot");
			
			
			
			

            ItemData itemData = DataManager.Instance.GetItemData(_lootId);
            Dictionary<FC_EQUIP_EXTEND_ATTRIBUTE, int> extendAttributes = new Dictionary<FC_EQUIP_EXTEND_ATTRIBUTE, int>();


            PlayerInfo.Instance.CombatInventory.AddItemInventory(itemData, 1);
			
			string message = Localization.instance.Get(itemData.nameIds);
			message = AddColor(message,itemData.rareLevel);
            MessageController.Instance.AddMessageIds(2.0f, "IDS_MESSAGE_COMBAT_LOOT_ITEM", message);
        }
    }
	
	
	public static string[] _colors = new string[]{
		"[ffffff]",
		"[28ac38]",
		"[38a4ff]",
		"[d24ed0]",
		"[ffbb38]"
	};
	
	public string AddColor(string message , int index)
	{
		string col = _colors[index];
		message = col + message + "[-]";
		return message;
	}
	
	

    IEnumerator DelayDestroy(float time)
    {
        yield return new WaitForSeconds(time);
        RealDestroy();
    }

    void RealDestroy()
    {
        Destroy(gameObject);

        if (_partcileEffect != null)
        {
            Destroy(_partcileEffect);

            _partcileEffect = null;
        }
    }
}
