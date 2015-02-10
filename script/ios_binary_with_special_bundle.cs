using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

using InJoy.AssetBundles;

/// <summary>
/// This class builds a normal player without bundles and is presumably for project "forTC" only, because all the levels listed in 
/// build settings will be packed into a single player and will make it very big.
/// </summary>
public class ios_binary_with_special_bundle : InJoy.UnityBuildSystem.IBuildConfig
{
	public void Build (System.Collections.Generic.List<string> scenesToBuild, UnityEditor.BuildTarget buildTarget, UnityEditor.BuildOptions buildOptions)
	{
		Console.WriteLine("ios_binary_with_special_bundle begin");
		
		BuilderHelper.BootSceneFilename = "Assets/Scenes/Boot/PreBoot.unity";
		const int basicSceneCount = 7;
        if (scenesToBuild.Count > basicSceneCount)
        {
            scenesToBuild.RemoveRange(basicSceneCount, scenesToBuild.Count - basicSceneCount);
        }
		BuilderHelper.CleanStreamingAssets();
		BuilderHelper.Build(ref scenesToBuild, ref buildTarget, ref buildOptions, this, "index", null, false);
		BuilderHelper.CopyToStreamingAssets();
		
		InJoy.UnityBuildSystem.AutoBuild.UnityBuildPlayer(scenesToBuild, buildTarget, buildOptions);
		Console.WriteLine("ios_binary_with_special_bundle end");
	}
}