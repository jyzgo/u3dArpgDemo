namespace InJoy.UnityBuildSystem.BuildLoggerImpl
{
	public class TeamCityBuildLogger : IBuildLogger
	{
		#region IBuildLogger implementation
		
		void IBuildLogger.OpenBlock(string blockName)
		{
			var message = string.Format("##teamcity[blockOpened name='{0}']", blockName);
			Log(message);
		}
		
		void IBuildLogger.CloseBlock(string blockName)
		{
			var message = string.Format("##teamcity[blockClosed name='{0}']", blockName);
			Log(message);
		}
		
		void IBuildLogger.LogMessage(string message)
		{
			message = string.Format("##teamcity[message text='{0}' status='NORMAL']", message);
			Log(message);
		}
		
		void IBuildLogger.LogWarning(string message)
		{
			message = string.Format("##teamcity[message text='{0}' status='WARNING']", message);
			Log(message);
		}
		
		void IBuildLogger.LogError(string message)
		{
			message = string.Format("##teamcity[message text='{0}' status='ERROR']", message);
			Log(message);
		}
		
		#endregion
		#region
		
		private void Log(string message)
		{
			System.Console.WriteLine(message);
		}
		
		#endregion
	}
}