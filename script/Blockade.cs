using UnityEngine;
using System.Collections;

public class Blockade : AirGate 
{
	public string _fadeoutSfxName;
	public string _fadeinSfxName;
	public FC_GLOBAL_EFFECT _fadeoutEffect = FC_GLOBAL_EFFECT.BLOCKADE_FADEOUT;
	
	private BlockadeController _myController = null;
	
	protected override void Awake()
	{
		//init my controller
		_myController = gameObject.AddComponent<BlockadeController>();		
		_myController.Init();
		
		gameObject.SetActive(false);
	}
	
	protected override void CreateGates()
	{		
		_myController.CreateGates();
	}
	
	// 
	public override void Active()
	{
		_myController.Active();
	}
	
	public override void Deactive()
	{
		_myController.DeActive();
	}
}
