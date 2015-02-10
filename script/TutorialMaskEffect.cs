using UnityEngine;
using System.Collections;

public class TutorialMaskEffect : MonoBehaviour
{
	private static TutorialMaskEffect _instance;

	public static TutorialMaskEffect Instance
	{
		get { return _instance; }
	}

	public UISprite topBanner;
	public UISprite bottomBanner;
	public UISprite leftBanner;
	public UISprite rightBanner;

	public UISprite substitute;

	public UILabel labelTip;

	public Camera mbCamera;

	void Awake()
	{
		_instance = this;
	}

	void OnDestroy()
	{
		_instance = null;
	}
	

	public void GotoTarget(GameObject target)
	{
		Transform t = target.transform;

		BoxCollider collider = t.collider as BoxCollider;

		//assume the target alignment is Hcenter and Vcenter.
		float leftX = t.localPosition.x + (t.localScale.x * collider.center.x - t.localScale.x * collider.size.x) / 2;
		float bottomY = t.localPosition.y + (t.localScale.y * collider.center.y - t.localScale.y * collider.size.y) / 2;

		//local
		Vector3 bottomLeft = new Vector3(leftX, bottomY, 0);
		
		Vector3 size = new Vector3(t.localScale.x * collider.size.x, t.localScale.y * collider.size.y, 0);
		
		Vector3 topRight = bottomLeft + size;

		//to screen point
		Camera camera = UIManager.Instance.MainCamera;

		bottomLeft = camera.WorldToScreenPoint(t.parent.TransformPoint(bottomLeft));
		topRight = camera.WorldToScreenPoint(t.parent.TransformPoint(topRight));

		//DisplayTargetArea(bottomLeft, topRight);

		DisplayMasks(bottomLeft, topRight);
	}

	private void DisplayMasks(Vector3 screenBottomLeft, Vector3 screenTopRight)
	{
		//move the masks
		Vector3 pos, pos2;
		TweenScale tweener;
		UISprite mask;

		//top, ->
		mask = topBanner;
		pos = this.transform.InverseTransformPoint(mbCamera.ScreenToWorldPoint(new Vector3(0, screenTopRight.y, 0)));
		mask.transform.localPosition = pos;

		pos2 = this.transform.InverseTransformPoint(mbCamera.ScreenToWorldPoint(new Vector3(Screen.width, screenTopRight.y, 0)));

		tweener = mask.GetComponent<TweenScale>();
		tweener.from = new Vector3(0, 640, 1);
		tweener.to = new Vector3(pos2.x - pos.x, 640, 1);
		tweener.Reset();
		tweener.enabled = true;

		mask.gameObject.SetActive(true);
		
		//right, V
		mask = rightBanner;
		pos = this.transform.InverseTransformPoint(mbCamera.ScreenToWorldPoint(new Vector3(screenTopRight.x, screenTopRight.y, 0)));
		mask.transform.localPosition = pos;

		pos2 = this.transform.InverseTransformPoint(mbCamera.ScreenToWorldPoint(new Vector3(screenTopRight.x, screenBottomLeft.y, 0)));

		tweener = mask.GetComponent<TweenScale>();
		tweener.from = new Vector3(1200, 0, 1);
		tweener.to = new Vector3(1200, pos.y - pos2.y, 1);
		tweener.enabled = true;
		tweener.Reset();

		mask.gameObject.SetActive(true);
		
		//bottom, <
		mask = bottomBanner;
		pos = this.transform.InverseTransformPoint(mbCamera.ScreenToWorldPoint(new Vector3(Screen.width, screenBottomLeft.y, 0)));
		mask.transform.localPosition = pos;

		pos2 = this.transform.InverseTransformPoint(mbCamera.ScreenToWorldPoint(new Vector3(0, screenBottomLeft.y, 0)));

		tweener = mask.GetComponent<TweenScale>();
		tweener.from = new Vector3(0, 640, 1);
		tweener.to = new Vector3(pos.x - pos2.x, 640, 1);
		tweener.enabled = true;
		tweener.Reset();

		mask.gameObject.SetActive(true);

		//left, ^
		mask = leftBanner;
		pos = this.transform.InverseTransformPoint(mbCamera.ScreenToWorldPoint(new Vector3(screenBottomLeft.x, screenBottomLeft.y, 0)));
		mask.transform.localPosition = pos;

		pos2 = this.transform.InverseTransformPoint(mbCamera.ScreenToWorldPoint(new Vector3(screenBottomLeft.x, screenTopRight.y, 0)));

		tweener = mask.GetComponent<TweenScale>();
		tweener.from = new Vector3(1200, 0, 1);
		tweener.to = new Vector3(1200, pos2.y - pos.y, 1);
		tweener.enabled = true;
		tweener.Reset();

		mask.gameObject.SetActive(true);
	}

	//show a substitute of the target with sprite. They should have the same size. For debug only. The coordinates are local.
	//aligned to Hcenter, Vcenter
	private void DisplayTargetArea(Vector3 screenBottomLeft, Vector3 screenTopRight)
	{
		//to world of current camera
		Vector3 bottomLeft = mbCamera.ScreenToWorldPoint(screenBottomLeft);
		Vector3 topRight = mbCamera.ScreenToWorldPoint(screenTopRight);

		//to local
		bottomLeft = this.transform.InverseTransformPoint(bottomLeft);
		topRight = this.transform.InverseTransformPoint(topRight);

	
		Vector3 size = topRight - bottomLeft;
		Vector3 center = bottomLeft + size / 2;
		substitute.transform.localPosition = center;
		substitute.transform.localScale = size;

		substitute.gameObject.SetActive(true);
	}

	public void Close()
	{
		topBanner.gameObject.SetActive(false);
		bottomBanner.gameObject.SetActive(false);
		leftBanner.gameObject.SetActive(false);
		rightBanner.gameObject.SetActive(false);
		
		labelTip.gameObject.SetActive(false);
	}

#if DEVELOPMENT_BUILD || UNITY_EDITOR
	private float _posLeft = 300;
	private float _posRight = 400;
	private float _posTop = 340;
	private float _posBottom = 200;
	void OnGUI()
	{
		if (CheatManager.tutorialMaskDebug)
		{
			GUILayout.BeginArea(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 50, 300, 100));
			GUILayout.BeginHorizontal();

			GUILayout.BeginVertical();
			GUILayout.Label("Bottom left(x,y):");
			float.TryParse(GUILayout.TextField(_posLeft.ToString()), out _posLeft);
			float.TryParse(GUILayout.TextField(_posBottom.ToString()), out _posBottom);
			GUILayout.EndVertical();

			GUILayout.BeginVertical();
			GUILayout.Label("Top right(x,y):");
			float.TryParse(GUILayout.TextField(_posRight.ToString()), out _posRight);
			float.TryParse(GUILayout.TextField(_posTop.ToString()), out _posTop);
			GUILayout.EndVertical();

			GUILayout.BeginVertical();
			if (GUILayout.Button("Start"))
			{
				DisplayMasks(new Vector3(_posLeft, _posBottom, 0), new Vector3(_posRight, _posTop, 0));
			}
			if (GUILayout.Button("Stop"))
			{
				this.Close();
			}
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}
	}
#endif
}
