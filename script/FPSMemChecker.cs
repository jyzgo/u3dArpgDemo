using UnityEngine;
using System.Collections;

public class FPSMemChecker : MonoBehaviour
{
    public float UpdateInterval = 0.5f;
    //
    private float _lastFPSTime;
    private int _frameCount;
    private int _fps;
    private int _curMemory;
    //
    private const float TEXT_SCALE_COEFF = 2.0f;
    private Vector3 _scaleVector;
    //
    private const float RECT_WIDTH = 64.0f;
    private const float RECT_HEIGHT = 72.0f;
    private Texture2D _texture;
    private Rect _rect;
 
    public enum AlignHorzEnum
    {
        LEFT,
        CENTER,
        RIGHT
    }

    public enum AlignVertEnum
    {
        UP,
        CENTER,
        DOWN
    }
 
    public AlignHorzEnum AlignHorz = AlignHorzEnum.LEFT;
    public AlignVertEnum AlignVert = AlignVertEnum.UP;
    
    public int FPS    { get { return _fps; } }
    public int Memory { get { return _curMemory; } }
    //
    private float _xPos;
    private float _yPos;
    //
    private float _screenWidth;
    private float _screenHeight;

#if FPS_COUNTER_ENABLED
    void CalculateRect()
    {
        _xPos = 0;
        switch(AlignHorz)
        {
        case AlignHorzEnum.CENTER:
            _xPos = 0.5f * (Screen.width - RECT_WIDTH);
            break;
        case AlignHorzEnum.RIGHT:
            _xPos = Screen.width - RECT_WIDTH;
            break;
        }        
     
        _yPos = 0;
        switch(AlignVert)
        {
        case AlignVertEnum.CENTER:
            _yPos = 0.5f * (Screen.height - RECT_HEIGHT);
            break;
        case AlignVertEnum.DOWN:
            _yPos = Screen.height - RECT_HEIGHT;
            break;
        }        
        _rect = new Rect(_xPos, _yPos, RECT_WIDTH, RECT_HEIGHT);
     
        Debug.Log("Screen width  ====> " + Screen.width);
        Debug.Log("Screen height ====> " + Screen.height);
     
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;
        
        Debug.Log(_rect);
    }
 
    void Awake()
    {
        CalculateRect();
     
        _scaleVector = new Vector3(TEXT_SCALE_COEFF, TEXT_SCALE_COEFF, 1.0f);                
        
        _texture = new Texture2D(1, 1);
        _texture.SetPixel(0, 0, Color.black);
        _texture.Apply();
    }
    
    void Start()
    {
        _lastFPSTime = Time.realtimeSinceStartup;
        _frameCount = 0;
        _fps = 0;
        
        _curMemory = 0;               
    }

    void Update()
    {
        _frameCount += 1;
        
        float currentTime = Time.realtimeSinceStartup;
        
        if(currentTime > _lastFPSTime + UpdateInterval)
        {
            _fps = (int)(_frameCount / (currentTime - _lastFPSTime));
            _lastFPSTime = currentTime;
            _frameCount = 0;
            
            _curMemory = (NativeUtils.GetCurrentMemoryBytes() >> 20);
        }
     
        if(_screenHeight != Screen.height || _screenWidth != Screen.width)
            CalculateRect();
    }

    void OnGUI()
    {
        GUI.DrawTexture(_rect, _texture);
        
        Color lastcolor = GUI.contentColor;
        GUI.contentColor = Color.magenta;

        Matrix4x4 lastMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3(_xPos, _yPos, 0), Quaternion.identity, _scaleVector);
        
        string str = _fps.ToString("00") + "\n" + _curMemory.ToString("000");
        GUILayout.Label(str);
        
        GUI.matrix = lastMatrix;
        GUI.contentColor = lastcolor;
    }
#else    
    private void Start()
    {
        GameObject.Destroy(this.gameObject);
    }
#endif
}
