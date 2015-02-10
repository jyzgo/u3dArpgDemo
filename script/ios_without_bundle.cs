using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

using InJoy.AssetBundles;

/// <summary>
/// This class builds a normal player without bundles and is presumably for project "forTC" only, because all the levels listed in 
/// build settings will be packed into a single player and will make it very big.
/// </summary>
public class ios_without_bundles : InJoy.UnityBuildSystem.IBuildConfig
{
	public void Build (System.Collections.Generic.List<string> scenesToBuild, UnityEditor.BuildTarget buildTarget, UnityEditor.BuildOptions buildOptions)
	{
		Console.WriteLine("ios without bundle PreBuild");
		BuilderHelper.BootSceneFilename = "Assets/Scenes/Boot/PreBoot.unity";
        //Remove all scenes except the first 3 basic ones
		
		const int basicSceneCount = 7;
        if (scenesToBuild.Count > basicSceneCount)
        {
            scenesToBuild.RemoveRange(basicSceneCount, scenesToBuild.Count - basicSceneCount);
        }
		InJoy.UnityBuildSystem.AutoBuild.UnityBuildPlayer(scenesToBuild, buildTarget, buildOptions);
		Console.WriteLine("ios without bundle PostBuild");
	}
}
