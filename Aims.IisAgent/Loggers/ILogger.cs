namespace Aims.IISAgent.Loggers
{
	public interface ILogger
	{
		void WriteError(string msg);

		void WriteNotify(string msg);

		void WriteWarning(string msg);
	}
}