using UnityEngine;
using System.Collections;

abstract public class BattleCharEffect : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	abstract public void PrepareEffect();
	
	//show basic effect
	//0 --  stop and play destroy effects
	//1~n -- play basic effect
	abstract public void ShowEffect(int effectIndex);
	
	//hide or not
	abstract public void Show(bool show);
	
	//on/off some start mes
	abstract public void ShowStartEffect(bool show);
	
	//begin some special end effect
	abstract public void ShowSpecialEndEffect(int effectIndex);

	//reset location
	abstract public void ResetLocation(Vector3 location);	
	
	//lock me?
	abstract public void LockLocation(bool lockMe);		
}
