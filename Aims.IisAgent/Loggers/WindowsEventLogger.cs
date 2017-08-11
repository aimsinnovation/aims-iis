using System.Diagnostics;

namespace Aims.IISAgent.Loggers
{
	public class WindowsEventLogger : ILogger
	{
		private readonly EventLog _eventLog;

		private readonly bool _verboseLog;

		public WindowsEventLogger(EventLog eventLog, bool verboseLog = true)
		{
			_eventLog = eventLog;
			_verboseLog = verboseLog;
		}

		public void WriteError(string msg)
		{
			if (_verboseLog)
				_eventLog.WriteEntry(msg, EventLogEntryType.Error);
		}

		public void WriteNotify(string msg)
		{
			if (_verboseLog)
				_eventLog.WriteEntry(msg, EventLogEntryType.Information);
		}

		public void WriteWarning(string msg)
		{
			if (_verboseLog)
				_eventLog.WriteEntry(msg, EventLogEntryType.Warning);
		}
	}
}