#define DEAD_EFFECT_ANIMATION

using UnityEditor;
using UnityEngine;
using System.Collections;


public class TextureGenerator : ScriptableWizard {
	
	public int _width;
	public int _height;
	
	public string _savePath = "Assets/Textures/test/test.png";
	
	public AnimationCurve _curve;
	public AnimationCurve _curve1;
	public Color _color;
	
	public void OnWizardUpdate()
    {
		isValid = (_width > 0 && _height > 0 && _savePath.Length > 0);
    }
	
	public void OnWizardCreate() {
		
		Texture2D newTex = new Texture2D(_width, _height);
		
		for(int i = 0;i < _width;++i)
		{
			for(int j = 0;j < _height;++j)
			{
				newTex.SetPixel(i, j, GetColorFromCoordination(i, j));
			}
		}
		
		byte[] texByte = newTex.EncodeToPNG();
		
		System.IO.DirectoryInfo path = System.IO.Directory.GetParent(_savePath);
		if(!System.IO.Directory.Exists(path.FullName))
		{
			System.IO.Directory.CreateDirectory(path.FullName);
		}
		System.IO.File.WriteAllBytes(_savePath, texByte);
		DestroyImmediate(newTex);
		
		AssetDatabase.Refresh();
	}
	
	[MenuItem("Tools/Texture/Generate Procedual Texture", false, 4)]
    static void GenerateTexture()
    {
		TextureGenerator tg = ScriptableWizard.DisplayWizard<TextureGenerator>("Generate Procedual Texture","Create");
		Texture2D tex = Selection.activeObject as Texture2D;
		if(tex != null) {
			tg._savePath = AssetDatabase.GetAssetPath(Selection.activeObject);
		}
	}
	
#if DEPTH_CONVERSION
	Color32 GetColorFromCoordination(int i, int j)
	{
		uint bit32 = 0xffffffff;
		float coordx = (float)i / (float)(_width - 1);
		uint maxcurrate = (uint)(coordx * bit32);
		Color32 newColor = new Color32(0,0,0,0);
		newColor.r = (byte)(maxcurrate >> 24);
		newColor.g = (byte)((maxcurrate & 0x00ffffff) >> 16);
		newColor.b = (byte)((maxcurrate & 0x0000ffff) >> 8);
		newColor.a = (byte)(maxcurrate & 0xff);
		
		return newColor;
	}
#endif
	
#if NORMAL_CONVERSION
	Color32 GetColorFromCoordination(int i, int j)
	{
		float nx = (float)i / (float)(_width - 1);
		float ny = (float)j / (float)(_height - 1);
		
		Color32 newColor;
		newColor.r = (byte)(nx * 255);
		newColor.g = (byte)(ny * 255);
		newColor.b = (byte)((Mathf.Sqrt(1.0f - (nx * 2 - 1) * (nx * 2 - 1) - (ny * 2 - 1) * (ny * 2 - 1)) * 0.5 + 0.5) * 255);
		newColor.a = 255;
		
		return newColor;
	}
#endif
	
#if GLOSS_CONVERSION
	Color32 GetColorFromCoordination(int i, int j)
	{
		float gloss = 15.0f;
		
		float nx = (float)i / (float)(_width - 1);
		float ny = (float)j / (float)(_height - 1);
		
		Color32 newColor;
		uint diffuse = (uint)(nx * 0xffff);
		newColor.r = (byte)(diffuse >> 8);
		newColor.g = (byte)(diffuse & 0xff);
		
		uint specular = (uint)(Mathf.Pow(ny, gloss) * 0xffff);
		newColor.b = (byte)(specular >> 8);
		newColor.a = (byte)(specular & 0xff);
		
		return newColor;
	}
#endif
	
#if ATTENUATION_DIFFUSE
	Color32 GetColorFromCoordination(int i, int j)
	{
		// x is dot, y is attenuation
		float nx = (float)i / (float)(_width - 1);
		float ny = (float)j / (float)(_height - 1);
		
		Color32 newColor;
		float attenuation = Mathf.Max(0.0f, _curve.Evaluate(ny));
		float diffuse = nx;
		float final = diffuse * attenuation;
		uint finalUint = (uint)(final * 0xffffffff);
		newColor.r = (byte)(finalUint >> 24);
		newColor.g = (byte)((finalUint >> 16) & 0xff);
		newColor.b = (byte)((finalUint >> 8) & 0xff);
		newColor.a = (byte)(finalUint & 0xff);
		
		return newColor;
	}
#endif
	
#if CONE_ATTENUATION
	Color32 GetColorFromCoordination(int i, int j) {
		float nx = (float)i / (float)(_width - 1);
		
		Color32 newColor;
		uint brightness = (uint)(_curve.Evaluate(nx) * 0xffffffff);
		newColor.r = (byte)(brightness >> 24);
		newColor.g = (byte)((brightness >> 16) & 0xff);
		newColor.b = (byte)((brightness >> 8) & 0xff);
		newColor.a = (byte)(brightness & 0xff);
		
		return newColor;
	}
#endif
	
#if COLOR_HUE
	Color32 GetColorFromCoordination(int i, int j)
	{
		byte []rangeR = new byte[]{255,255,0,0,0,255,255};
		byte []rangeG = new byte[]{0,255,255,255,0,0,0};
		byte []rangeB = new byte[]{0,0,0,255,255,255,0};
		float nx = (float)i / (float)(_width - 1) * 6.0f;
		
		Color32 newColor;
		
		float floor = Mathf.Floor(nx);
		float ceil = Mathf.Ceil(nx);
		
		newColor.r = (byte)(rangeR[(int)floor] * (1 - nx + floor) + rangeR[(int)ceil] * (nx - floor));
		newColor.g = (byte)(rangeG[(int)floor] * (1 - nx + floor) + rangeG[(int)ceil] * (nx - floor));
		newColor.b = (byte)(rangeB[(int)floor] * (1 - nx + floor) + rangeB[(int)ceil] * (nx - floor));
		newColor.a = 255;
		
		return newColor;
	}
#endif
	
#if GAMMA_2DTEXTURE
	Color32 GetColorFromCoordination(int i, int j) 
	{
		float r = (float) i / (float)(_width - 1);
		float gb = (float)j / (float)(_height - 1);
		float scaleGB = gb * 4.0f;
		float b = Mathf.Floor(scaleGB);
		float g = scaleGB - b;		
		b = b / 4.0f;
		
		Color32 newColor;
		float average = (r + g + b) / 3.0f;
		float percent = Mathf.Clamp(g / (average * 3), 0.0f, 1.0f);
		newColor.r = (byte)(255 * Mathf.Lerp(average, r, percent));
		newColor.g = (byte)(255 * Mathf.Lerp(average, g, percent));
		newColor.b = (byte)(255 * Mathf.Lerp(average, b, percent));
		newColor.a = 255;
		
		return newColor;
	}
#endif
	
#if TIMELINE_INFO
	Color32 GetColorFromCoordination(int i, int j)
	{
		float x = (float)i / (float)(_width - 1);
		
		Color32 newColor;
		float v = Mathf.Clamp01(_curve.Evaluate(x));
		float v1 = Mathf.Clamp01(_curve1.Evaluate(x));
		byte b = (byte)(v * 255);
		byte a = (byte)(v1 * 255);
		
		newColor.r = newColor.g = newColor.b = b;
		newColor.a = a;
		
		return newColor;
	}
#endif
	
#if COLOR_AND_DOT_REMAPPING
	Color32 GetColorFromCoordination(int i, int j)
	{
		float x = (float)i / (float)(_width - 1);
		float y = (float)j / (float)(_height - 1);
		
		Color32 newColor;
		float v = Mathf.Clamp01(_curve.Evaluate(x));
		uint dot = (uint)(v * 65535.0f);
		uint gloss = (uint)(Mathf.Pow(y, 15) * 65535.0f);
		
		newColor.r = (byte)((dot >> 8) & 0xff);
		newColor.g = (byte)(dot & 0xff);
		
		newColor.b = (byte)((gloss >> 8) & 0xff);
		newColor.a = (byte)(gloss & 0xff);
		
		return newColor;
	}
#endif
	
#if DEAD_EFFECT_ANIMATION
	Color32 GetColorFromCoordination(int i, int j) {
		float x = (float)i / (float)(_width - 1);
		
		Color32 newColor;
		float v = Mathf.Clamp01(_curve.Evaluate(x));
		newColor = _color;
		newColor.r = (byte)(newColor.r * v);
		newColor.g = (byte)(newColor.g * v);
		newColor.b = (byte)(newColor.b * v);
		v = Mathf.Clamp01(_curve1.Evaluate(x));
		newColor.a = (byte)(255 * v);
		
		return newColor;
	}
#endif

}
