using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

using InJoy.AssetBundles;

public class ios_bundles_for_download : InJoy.UnityBuildSystem.IBuildConfig
{
	public void Build (System.Collections.Generic.List<string> scenesToBuild, UnityEditor.BuildTarget buildTarget, UnityEditor.BuildOptions buildOptions)
	{
        Console.WriteLine("ios_bundles_for_download PreBuild");
		BuilderHelper.BuildOutsideOfPlayer(ref scenesToBuild, ref buildTarget, ref buildOptions, this);
        Console.WriteLine("ios_bundles_for_download PostBuild");
	}
}
