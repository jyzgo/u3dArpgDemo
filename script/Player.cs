using UnityEngine;
using System.Collections;

public class Identity{
	
	public int provider;
	public string id;
	public string name;
	public string auth;
	public string secretAuth =null;
	
	public Identity(int provider, string id,string name,string auth){
		this.provider = provider;
		this.id = id;
		this.name = name;
		this.auth = auth;
	}
	
	public Identity(int provider, string id, string name){
		this.provider = provider;
		this.id = id;
		this.name = name;
		this.auth =null;
	}
	
	public Identity(int provider){
		this.provider = provider;
		this.id = null;
		this.name = null;
		this.auth =null;
	}
	
	public Identity(){
		this.provider = 0;
		this.id = null;
		this.name = null;
		this.auth =null;
	}
}


public class FriendData
{
    public string friendName;
    public ArrayList GSList = new ArrayList();
    public Hashtable towerFarestPosition = new Hashtable();
    public int towerFarsetPositionForSelectedTower = 0;
}

