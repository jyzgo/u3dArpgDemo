using UnityEngine;
using System.Collections;


public enum IdentityStatus{
	NOT_STARTED = 0,
	RETRIEVING_REQUEST = 1,
	RETRIEVING_AUTHORIZATION = 2,
	RETRIEVING_IDENTIFICATION = 3,
	COMPLETE = 4,
	
	ERROR = -1
}


public class IdentityProvider 
{
	
	protected IdentityStatus loginStatus = IdentityStatus.NOT_STARTED;
	protected Identity identity;
	
	
		/// <summary>
	/// Retrieves the player's name 
	/// </summary>
	/// <returns>
	/// A <see cref="System.String"/>
	/// </returns>
	public string name{
		get{
			return identity==null?null:identity.name;	
		}
	}
	
	public string auth{
		get{
			return identity==null?null:identity.auth;
		}
	}
	
	public string id{
		get{
			return identity==null?null:identity.id;
		}
	}
	
	public string secretAuth{
		get{
			return identity==null?null:identity.secretAuth;
		}
	}
	
	
	public delegate void OnLoginComplete(bool succeeded);
	
	public OnLoginComplete onLoginComplete;


	
	public virtual IdentityStatus GetStatus(){
		return loginStatus;	
	}
	
	public IdentityStatus status{
		get{
			return loginStatus;
		}
	}
	
	public void Login(OnLoginComplete oLC){
		if(loginStatus != IdentityStatus.NOT_STARTED)
			return;

		onLoginComplete = oLC;
		
		Login ();
	}
	
	public virtual void Login(){}
	
	public virtual void Login(string username, string password){}
	
	public virtual void Logout(){}
	
	public virtual Identity GetIdentity(){
		if(loginStatus == IdentityStatus.COMPLETE)
			return identity;
		else
			return null;
	}
}
