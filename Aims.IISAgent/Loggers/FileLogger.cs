using log4net;

namespace Aims.IISAgent.Loggers
{
    public class FileLogger : ILogger
    {
        readonly ILog _logView = LogManager.GetLogger(typeof(FileLogger));

        public void WriteError(string msg)
        {
            _logView.Error(msg);
        }

        public void WriteNotify(string msg)
        {
            _logView.Info(msg);
        }

        public void WriteWarning(string msg)
        {
            _logView.Warn(msg);
        }
    }
}