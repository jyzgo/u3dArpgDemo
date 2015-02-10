using UnityEngine;
using System.Collections;

[System.Serializable]
public class SkeletonAnim
{
	public string resource;
	
	GameObject FBXResource;
	Animation AnimComp;
}

public class SkeletonLib : ScriptableObject {
		
	public enum SkeletonAnimType
	{
		SkeletonAnimType_Biped = 0,
		SkeletonAnimType_Minotaur,
		SkeletonAnimType_Ogre,
		SkeletonAnimType_Beast,
		SkeletonAnimType_Mollusk,
		SkeletonAnimType_Ghost,
		SkeletonAnimType_Bird,
		SkeletonAnimType_Hero0,
		SkeletonAnimType_Hero1,
		SkeletonAnimType_Hero2,
		SkeletonAnimType_Count
	}
	public static int SkeletonAnimTypeCount
	{
		get {return (int)SkeletonAnimType.SkeletonAnimType_Count;}
	}
	
	public SkeletonAnim []_skeletonAnimationConfig;
}
