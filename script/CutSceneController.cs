using UnityEngine;

public class CutSceneController : MonoBehaviour
{
	public TweenScale topEdge;

	public TweenScale bottomEdge;

	void OnInitialize()
	{
		topEdge.enabled = true;
		topEdge.Reset();

		bottomEdge.enabled = true;
		bottomEdge.Reset();
	}
}