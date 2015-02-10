using UnityEngine;
using System.Collections;

// Wall of air
public class AirGate : MonoBehaviour 
{	
	// Prefab of gate for game.
	public string _gateForGame;
	
	protected virtual void Awake()
	{
	}
	
	// 
	public virtual void Active()
	{		
		CreateGates();
	}
	
	protected virtual void CreateGates()
	{
	}
	
	public virtual void Deactive()
	{
	}
}