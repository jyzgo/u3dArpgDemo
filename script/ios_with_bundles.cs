using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

using InJoy.AssetBundles;

public class ios_with_bundles : InJoy.UnityBuildSystem.IBuildConfig
{
	public void Build (System.Collections.Generic.List<string> scenesToBuild, UnityEditor.BuildTarget buildTarget, UnityEditor.BuildOptions buildOptions)
	{
		Console.WriteLine("ios PreBuild");
		BuilderHelper.BootSceneFilename = "Assets/Scenes/Boot/Boot.unity";
		BuilderHelper.BuildInsidePlayer(ref scenesToBuild, ref buildTarget, ref buildOptions, this);
		InJoy.UnityBuildSystem.AutoBuild.UnityBuildPlayer(scenesToBuild, buildTarget, buildOptions);
		AssetBundlesFtp.GenerateIndexSizeMapToFile("assetBundlesSizeFile.txt");
		Console.WriteLine("ios PostBuild");
	}
}
