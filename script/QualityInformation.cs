using UnityEngine;
using System.Collections;

public class QualityInformation : MonoBehaviour{

	void Awake() {
		InitQualitySettings();
	}
	
	public static void InitQualitySettings() {
		int level = GameSettings.Instance.LODSettings.GetDeviceLevel();
		// qualities: 
		// 0. Low Level
		// 1. Base Level
		// 2. Standard Level
		// 3. Advance Level
		// 4. Next-Gen Level
		QualitySettings.SetQualityLevel(level, true);
		
		// shader LOD
		int []shaderLOD = new int[]{150, 250, 350, 450, 550};
		int shaderDebugLevel = 650;
		Shader.globalMaximumLOD = GameSettings.Instance.DebugShader ? shaderDebugLevel : shaderLOD[Mathf.Min(level, shaderLOD.Length - 1)];
		
		// FPS settings.
		int targetFPS = GameSettings.Instance.LODSettings.GetTargetFPS();
		Application.targetFrameRate = targetFPS;
	}
	
	static public int GetCharacterTextureSize(CharacterGraphicsQuality quality) {
		int []_dynamicTextureResolutionArray = new int[]{256, 256, 512};
		int []_dynamicTextureResolutionArray_low = new int[]{128, 128, 256};
		// lower resolution for low-end devices.
		int lowResLevel = 0;
		int []_activedResolution = 
			(GameSettings.Instance.LODSettings.GetDeviceLevel() <= lowResLevel ? _dynamicTextureResolutionArray_low : _dynamicTextureResolutionArray); 
		return _activedResolution[(int)quality];
	}
}

public enum CharacterGraphicsQuality
{
	CharacterGraphicsQuality_Battle = 0,
	CharacterGraphicsQuality_Town,
	CharacterGraphicsQuality_Preview,
	CharacterGraphicsQuality_Count
}
