namespace InJoy.UnityBuildSystem.BuildLoggerImpl
{
	public interface IBuildLogger
	{
		void OpenBlock(string blockName);
		void CloseBlock(string blockName);
		void LogMessage(string message);
		void LogWarning(string message);
		void LogError(string message);
	}
}