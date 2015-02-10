using UnityEngine;
using System.Collections;

public interface CommandComponent
{
	bool HandleCommand(ref FCCommand ewd);
}

[AddComponentMenu("FC/Logic/FCObject")]
public class FCObject : MonoBehaviour,CommandComponent {
	#region id
	private OBJECT_ID _objectID;
	#endregion
	
	#region cache point
	private Transform _thisTransform;
	private GameObject _thisObject;
	#endregion
	
	protected bool _isDestroyed = false;
	
	public bool IsDestroyed
	{
		get
		{
			return _isDestroyed;
		}
	}
	#region read and write ability declare
	public OBJECT_ID ObjectID
	{
		get
		{
			return _objectID;
		}
	}
	
	
	public Transform ThisTransform
	{
		get
		{
			return _thisTransform;
		}
	}
	
	public GameObject ThisObject
	{
		get
		{
			return _thisObject;
		}
	}
	#endregion
	
	protected virtual void Awake()
	{
		FirstInit();
		_isDestroyed = false;
	}
	
	protected virtual void FirstInit()
	{
		_thisTransform = transform;
		_thisObject = gameObject;
		_objectID = new OBJECT_ID(this,FC_OBJECT_TYPE.OBJ_NORMAL);
		ObjectManager.Instance.SaveObject(_objectID);
	}
	public virtual bool HandleCommand(ref FCCommand ewd)
	{
		return true;
	}
	
	protected virtual void OnDestroy()
	{
		_isDestroyed = true;
	}
	
}
