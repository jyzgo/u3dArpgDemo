#define CHECK_SHADER
using UnityEditor;
using UnityEngine;
using System.Collections;

public class ShaderChecker : AssetPostprocessor
{   
    static string[]_checkDirectories = new string[] {
                "Assets/Bullets",
                "Assets/Characters",
                "Assets/Levels",
                "Assets/Weapon"
        };
    
    static void OnPostprocessAllAssets(string[]importedAssets, string[]deletedAssets, string[]movedAssets, string[] movedFromPath)
    {
#if CHECK_SHADER
        foreach (string asset in importedAssets)
        {
            if (asset.EndsWith(".mat"))
            {
                bool check = false;
                foreach (string dir in _checkDirectories)
                {
                    if (asset.StartsWith(dir))
                    {
                        check = true;
                        break;
                    }
                }
                if (check)
                {
                    Material matObj = AssetDatabase.LoadAssetAtPath(asset, typeof(Material)) as Material;
                    if (matObj.shader == null || !matObj.shader.name.StartsWith("InJoy"))
                    {
                        Debug.LogWarning(string.Format("Wrong shader used in material {0}. Should be InJoy/xxx. Changed to InJoy/Common", asset));
                        matObj.shader = Shader.Find("InJoy/half sphere");
                        EditorUtility.SetDirty(matObj);
                    }
                }
            }
        }
#endif
    }
}
