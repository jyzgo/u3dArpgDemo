namespace InJoy.UnityBuildSystem.BuildLoggerImpl
{
	public class ConsoleBuildLogger : IBuildLogger
	{
		#region IBuildLogger implementation
		
		void IBuildLogger.OpenBlock(string blockName)
		{
			if(!string.IsNullOrEmpty(blockName))
			{
				var message = string.Format("==STARTING {0}==", blockName.Trim()).ToUpper();
				Log(message);
			}
		}
		
		void IBuildLogger.CloseBlock(string blockName)
		{
			if(!string.IsNullOrEmpty(blockName))
			{
				var message = string.Format("=={0} FINISHED==", blockName.Trim()).ToUpper();
				Log(message);
			}
		}
		
		void IBuildLogger.LogMessage(string message)
		{
			Log(message);
		}
		
		void IBuildLogger.LogWarning(string message)
		{
			message = string.Format("warning: {0}", message);
			Log(message);
		}
		
		void IBuildLogger.LogError(string message)
		{
			message = string.Format("error: {0}", message);
			Log(message);
		}
		
		#endregion
		#region Implementation
		
		private void Log(string message)
		{
			System.Console.WriteLine(message);
		}
		
		#endregion
	}
}