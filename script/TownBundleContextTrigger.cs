using UnityEngine;
using System.Collections;

public class TownBundleContextTrigger : MonoBehaviour 
{
	public string _targetButtonName;
	public string _functionName;
	public string _bundleConfig;
	
	private TownHUD _townUI;
	
	// Use this for initialization
	void Start () 
	{	
#if OUTLINE_SIMPLE_ENABLED
		_parent.gameObject.AddComponent("OutLineRendererSimple");
#endif
	}
	
	void OnTriggerEnter(Collider other)
	{
		ActionController ac = ActionControllerManager.Instance.GetACByCollider(other);
		if(ac != null)
		{
			if(_townUI == null && UIManager.Instance != null) {
				GameObject ui = UIManager.Instance.GetUI("TownHome");
				if( ui != null) {
					_townUI = ui.GetComponent<TownHUD>();
				}
			}
			if (_townUI != null && ac.IsPlayerSelf)
			{
				//_townUI.SetContextIcon(_targetButtonName, _functionName);
				
#if OUTLINE_ENABLED
				_parent.gameObject.layer = FC_CONST.LAYER_TRIGGER;
#elif OUTLINE_SIMPLE_ENABLED
				_parent.gameObject.SendMessage("OnReplaceByOutlineShader" , SendMessageOptions.DontRequireReceiver);
#endif
			}
			_fastCommand._param1 = _parent;
			_fastCommand._cmd = FCCommand.CMD.ACTION_IS_NEAR_NPC;
			CommandManager.Instance.SendFast(ref _fastCommand,ac);
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		ActionController ac = ActionControllerManager.Instance.GetACByCollider(other);
		if(ac != null)
		{
			if(_townUI == null && UIManager.Instance != null) {
				GameObject ui = UIManager.Instance.GetUI("TownHome");
				if( ui != null) {
					_townUI = ui.GetComponent<TownHUD>();
				}
			}
			if (_townUI != null && ac.IsPlayerSelf)
			{
				//_townUI.SetContextIcon(null, "");
#if OUTLINE_ENABLED
				_parent.gameObject.layer = FC_CONST.LAYER_DEFAULT;
#elif OUTLINE_SIMPLE_ENABLED
				_parent.gameObject.SendMessage("OnRevertOriginalShader" , SendMessageOptions.DontRequireReceiver);
#endif
			}
			_fastCommand._param1 = _parent;
			_fastCommand._cmd = FCCommand.CMD.ACTION_IS_AWAY_NPC;
			CommandManager.Instance.SendFast(ref _fastCommand,ac);
		}

		
	}
	
	public Transform _parent = null;
	protected FCCommand _fastCommand;
		// Use this for initialization
	void Awake()
	{
		_fastCommand = new FCCommand();
		_fastCommand.Set(FCCommand.CMD.STOP,null,FCCommand.STATE.RIGHTNOW,true);
	}

}
