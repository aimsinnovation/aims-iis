namespace Aims.IISAgent.Module.Loggers
{
	public interface ILogger
	{
		void WriteError(string msg);

		void WriteNotify(string msg);
	}
}