using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

using InJoy.AssetBundles;

/// <summary>
/// Make bundles only for a released binary. Some bundles are uncompressed and copied to streaming folder of binary by the binary maker project; 
/// other bundles will not be shipped with binary and will be uploaded to web server.
/// </summary>
public class ios_bundles_for_release : InJoy.UnityBuildSystem.IBuildConfig
{
	public void Build (System.Collections.Generic.List<string> scenesToBuild, UnityEditor.BuildTarget buildTarget, UnityEditor.BuildOptions buildOptions)
	{
        Console.WriteLine("ios_bundles_for_release PreBuild");
		BuilderHelper.BootSceneFilename = "Assets/Scenes/Boot/Boot.unity";
		BuilderHelper.BuildOutsidePlayer2(ref scenesToBuild, ref buildTarget, ref buildOptions, this);
		Console.WriteLine("ios_bundles_for_release PostBuild");
	}
}
