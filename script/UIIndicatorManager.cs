using UnityEngine;
using System.Collections;

public class UIIndicatorManager : MonoBehaviour
{

    public UIRoot _root;
    public Transform _enemyIndicatorTemplate;
    float _enemyIndicatorBold;
    System.Collections.Generic.List<Transform> _enemyIndicators;
    System.Collections.Generic.Dictionary<Transform, Transform> _activedEnemyIndicators;
    public Transform _pathIndicator;
    public Transform _pathIndicatorArrow;
    public TweenColor _pathIndicatorEffect;
    public float _pathIndicatorBold;

    public Vector2 _pathIndicatorCenter;
    public Vector2 _enemyIndicatorCenter;

    private Vector3 _pathTarget;
    private bool _pathIndicatorEnable = false;
    private UIPanel _parentPanel;
    private float _pixelAdjustment = 1.0f;

    void Awake()
    {
        _enemyIndicators = new System.Collections.Generic.List<UnityEngine.Transform>();
        _enemyIndicators.Add(_enemyIndicatorTemplate);
        _enemyIndicatorTemplate.gameObject.SetActive(false);
        _activedEnemyIndicators = new System.Collections.Generic.Dictionary<UnityEngine.Transform, UnityEngine.Transform>();
        _enemyIndicatorBold = _enemyIndicatorTemplate.localScale.x * 0.5f;
    }

    void Start()
    {
        _parentPanel = gameObject.GetComponent<UIPanel>();
        _pixelAdjustment = _root.pixelSizeAdjustment;
    }

    public void Cleanup()
    {
        _activedEnemyIndicators.Clear();
        foreach (Transform t in _enemyIndicators)
        {
            t.gameObject.SetActive(false);
        }
        _pathIndicatorEnable = false;
        _pathIndicator.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Rect screen = new Rect(0.0f, 0.0f, Screen.width, Screen.height);
        Camera cam = CameraController.Instance.MainCamera;
        Vector3 pos;
        Quaternion rot;
        Rect screenClamp = Rect.MinMaxRect(Screen.width * (1.0f - _enemyIndicatorCenter.x) * 0.5f,
                                            Screen.height * (1.0f - _enemyIndicatorCenter.y) * 0.5f,
                                            Screen.width * (1.0f + _enemyIndicatorCenter.x) * 0.5f,
                                            Screen.height * (1.0f + _enemyIndicatorCenter.y) * 0.5f);
        foreach (System.Collections.Generic.KeyValuePair<Transform, Transform> p in _activedEnemyIndicators)
        {
            pos = p.Value.localPosition;
            rot = p.Value.localRotation;
            p.Value.gameObject.SetActive(UpdateIndicator(screen, screenClamp, cam, p.Key.position, _enemyIndicatorBold, ref pos, ref rot));
            p.Value.localPosition = pos;
            p.Value.localRotation = rot;
        }
        if (_pathIndicatorEnable)
        {
            screenClamp = Rect.MinMaxRect(Screen.width * (1.0f - _pathIndicatorCenter.x) * 0.5f,
                                            Screen.height * (1.0f - _pathIndicatorCenter.y) * 0.5f,
                                            Screen.width * (1.0f + _pathIndicatorCenter.x) * 0.5f,
                                            Screen.height * (1.0f + _pathIndicatorCenter.y) * 0.5f);
            pos = _pathIndicator.localPosition;
            rot = _pathIndicator.localRotation;
            _pathIndicator.gameObject.SetActive(UpdateIndicator(screen, screenClamp, cam, _pathTarget, _pathIndicatorBold, ref pos, ref rot));
            _pathIndicator.localPosition = pos;
            _pathIndicatorArrow.localRotation = rot;
        }
    }

    bool UpdateIndicator(Rect screen, Rect screenClamp, Camera cam, Vector3 target, float bolder, ref Vector3 pos, ref Quaternion rot)
    {
        Vector3 screenPos = cam.WorldToScreenPoint(target);
        screenPos.x *= (screenPos.z < 0.0f ? -1.0f : 1.0f);
        screenPos.y *= (screenPos.z < 0.0f ? -1.0f : 1.0f);
        if (screen.Contains(screenPos))
        {
            return false;
        }

        pos.x = Mathf.Clamp(screenPos.x, screenClamp.xMin + bolder / _pixelAdjustment, screenClamp.xMax - bolder / _pixelAdjustment);
        pos.y = Mathf.Clamp(screenPos.y, screenClamp.yMin + bolder / _pixelAdjustment, screenClamp.yMax - bolder / _pixelAdjustment);
        Vector3 dir = screenPos - pos;
        dir.z = 0.0f;
        rot = Quaternion.FromToRotation(Vector3.down, dir);
        // UI coordination convertion.
        pos.x = (pos.x - screen.width / 2.0f) * _pixelAdjustment;
        pos.y = (pos.y - screen.height / 2.0f) * _pixelAdjustment;
        return true;
    }

    Transform CreateIndicator()
    {
        GameObject go = GameObject.Instantiate(_enemyIndicatorTemplate.gameObject) as GameObject;
        Transform newTransform = go.transform;
        newTransform.parent = _enemyIndicatorTemplate.parent;
        newTransform.localScale = _enemyIndicatorTemplate.localScale;
        go.SetActive(false);
        UIWidget widegets = go.GetComponent<UIWidget>();
        _parentPanel.AddWidget(widegets);
        return go.transform;
    }

    public void InitIndicatorPool(int length)
    {
        for (int i = 0; i < length - 1; ++i)
        {
            Transform newIndicator = CreateIndicator();
            _enemyIndicators.Add(newIndicator);
        }
    }

    public void ActiveEnemyIndicator(Transform enemy)
    {
        Transform indicator = null;
        // out of instance in pool
        if (_enemyIndicators.Count <= 0)
        {
            indicator = CreateIndicator();
        }
        // get from pool
        else
        {
            indicator = _enemyIndicators[0];
            _enemyIndicators.RemoveAt(0);
        }
        Assertion.Check(indicator != null);

        // register enemy.
        Assertion.Check(!_activedEnemyIndicators.ContainsKey(enemy));
        _activedEnemyIndicators.Add(enemy, indicator);
    }

    public void DeactiveEnemyIndicator(ActionController ac)
    {
        Transform enemy = ac.ThisTransform;
        Transform indicator = null;
        if (_activedEnemyIndicators.TryGetValue(enemy, out indicator))
        {
            indicator.gameObject.SetActive(false);
            _activedEnemyIndicators.Remove(enemy);
            _enemyIndicators.Add(indicator);
        }

        ObjectManager.Instance.RemoveObject(ac.ObjectID);
    }

    public void ActiveTriggerIndicator(Vector3 pos)
    {
        _pathTarget = pos;
        _pathIndicatorEnable = true;
        _pathIndicatorEffect.Play(true);
    }

    public void DeactiveTriggerIndicator()
    {
        _pathIndicatorEnable = false;
        _pathIndicator.gameObject.SetActive(false);
    }
}
