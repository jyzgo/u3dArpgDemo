using UnityEngine;
using System.Collections;

public class OBJECT_ID
{
	private int _id;
	private int _networkId = -1;
	public int NetworkId
	{
		set{
			Assertion.Check( _networkId == -1 , "network id could not be set if its value is not -1 !" ) ;
			_networkId = value;
			ObjectManager.Instance.SaveNetObject(this);
		}
		get{
			return _networkId;
		}
	}
	private FCObject _fcObj;
	private FC_OBJECT_TYPE _objectType;
	
	public FCObject fcObj
	{
		get
		{
			return _fcObj;
		}		
		
	}
	
	public FC_OBJECT_TYPE getOnlyObjectType
	{
		get
		{
			return (FC_OBJECT_TYPE)((int)_objectType & 0xff00);
		}
	}	
	
	public FC_AI_TYPE getOnlyAIType
	{
		get
		{
			return (FC_AI_TYPE)((int)_objectType & 0xff);
		}
	}	
	
	public FC_OBJECT_TYPE ObjectType
	{
		get
		{
			return _objectType;
		}
		set
		{
			_objectType = value;
		}
	}
	public bool IsValid
	{
		get
		{
			return _id>=0 && _fcObj != null;
		}
	}
	
	private static int tempObjID = 0;
	
	public OBJECT_ID(FCObject iobjw,FC_OBJECT_TYPE eot)
	{
		_id = tempObjID++; //iobjw.GetInstanceID();
		_networkId = -1;
		_fcObj = iobjw;
		_objectType = eot;
	}
	
	public bool HandleCommand(ref FCCommand ewd)
	{
		if(!_fcObj.IsDestroyed)
		{
			return _fcObj.HandleCommand(ref ewd);
		}
		else
		{
			return true;
		}
	}
	
	public static explicit operator int(OBJECT_ID objid)
    {
        return objid._id;
    }
}
