using System;
using System.IO;
using System.Threading;
using Aims.Sdk;

namespace Aims.FileCountAgent
{
    public partial class StatisticsMonitor
    {
        private class Watcher : IDisposable
        {
            private readonly FileSystemWatcher _fileSystemWatcher;
            private readonly NodeRef _nodeRef;
            private int _fileCount;

            public Watcher(NodeRef nodeRef)
            {
                _nodeRef = nodeRef;

                _fileSystemWatcher = new FileSystemWatcher(nodeRef.Parts[AgentConstants.NodeRefPart.Id]);
                _fileSystemWatcher.Created += OnFileCreated;
                _fileSystemWatcher.EnableRaisingEvents = true;
            }

            public StatPoint Collect()
            {
                return new StatPoint
                {
                    NodeRef = _nodeRef,
                    StatType = AgentConstants.StatType.FilesCreated,
                    Time = DateTimeOffset.Now,
                    Value = Interlocked.Exchange(ref _fileCount, 0),
                };
            }

            public void Dispose()
            {
                _fileSystemWatcher.Dispose();
            }

            private void OnFileCreated(object sender, FileSystemEventArgs e)
            {
                if (!Directory.Exists(e.FullPath))
                {
                    Interlocked.Increment(ref _fileCount);
                }
            }
        }
    }
}