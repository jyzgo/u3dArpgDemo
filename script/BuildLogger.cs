namespace InJoy.UnityBuildSystem
{
	using BuildLoggerImpl;
	
	public static class BuildLogger
	{
		#region Interface
		
		public static void OpenBlock(string blockName)
		{
			impl.OpenBlock(blockName);
		}
		
		public static void CloseBlock(string blockName)
		{
			impl.CloseBlock(blockName);
		}
		
		public static void LogMessage(string format, params string[] args)
		{
			var message = string.Format(format, args);
			impl.LogMessage(message);
		}
		
		public static void LogWarning(string format, params string[] args)
		{
			var message = string.Format(format, args);
			impl.LogWarning(message);
		}
		
		public static void LogError(string format, params string[] args)
		{
			var message = string.Format(format, args);
			impl.LogError(message);
		}
		
		#endregion
		#region Implementation
		
		static BuildLogger()
		{
			bool isUnityInBatchMode = UnityEditorInternal.InternalEditorUtility.inBatchMode;
			bool isBuildLocal = IsBuildLocal();
			
			if(isUnityInBatchMode && !isBuildLocal)
				impl = new TeamCityBuildLogger();
			else
				impl = new ConsoleBuildLogger();
			
			// TODO: declare (but not define) private partial static void Awake()
			//Awake();
		}
		
		private static bool IsBuildLocal()
		{
			// HACK: tag for locally built artifacts would contain word "Local"
			const string LOCAL_BUILD_INDICATOR_SUBSTRING = "Local";
			string buildTag = null;
			var args = System.Environment.GetCommandLineArgs();
			for(int idx = 0; idx < args.Length; idx++)
			{
				if((args[idx] == "+buildTag") && (args.Length > idx + 1))
				{
					buildTag = args[idx + 1];
					break;
				}
			}
			return (buildTag == null) || buildTag.ToLower().Contains(LOCAL_BUILD_INDICATOR_SUBSTRING.ToLower());
		}
		
		private static IBuildLogger impl { set; get; }
		
		#endregion
	}
}