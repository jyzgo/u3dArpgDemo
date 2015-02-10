using UnityEngine;
using System.Collections;

namespace InJoy.DynamicContentPipeline
{
	public class DynamicContentParam
	{	
		public bool IsAddonContent
		{
			set;get;
		}
		
		public string SpecialIndexName
		{
			set;get;
		}
		
		public DynamicContentInfo DCInfoCache
		{
			set;get;
		}
		
		public FCDownloadManager.FCIndexDownloadInfo TargetIndexDownloadInfo
		{
			set;get;
		}
		
		public DynamicContentParam()
		{
			SpecialIndexName = "none";
			IsAddonContent = true;
			DCInfoCache = null;
			TargetIndexDownloadInfo = null;
		}
		
		public string CheckAllSpecialIndexNameValid(string[] allIndexNames)
		{
			foreach(string fullTargetName in allIndexNames)
			{
				string targetVersionFileName = fullTargetName.Substring(fullTargetName.LastIndexOf('/') + 1);
				if(SpecialIndexName.CompareTo(targetVersionFileName) == 0)
				{
					return fullTargetName;
				}
			}
			
			return "";
		}
	}
}
