namespace Aims.FileCountAgent
{
    public static class AgentConstants
    {
        public static class NodeRefPart
        {
            public const string Id = "id";
	        public const string InstanceName = "instatnce-name";
	        public const string MachineName = "machine-name";
		}

        public static class NodeType
        {
            public const string Site = "aims.iis.site";
	        public const string AppPool = "aims.iis.app-pool";
	        public const string Machine = "aims.iis.machine";
        }

		public static class NodeProperties
		{
			public const string ApplicationPool = "aims.iis.site.property-app-pool";
		}
		
		//TODO remake
        public static class Service
        {
            public const string ApplicationName = "AIMS File Count Agent";
            public const string EventSource = "AIMS File Count Agent";
            public const string Log = "Application";
            public const string ServiceName = "aims-filecnt-agent";
        }

        public static class StatType
        {
	        public const string RequestsPerSec = "aims.iis.requests-per-sec";
	        public static string TotalThreads = "aims.iis.total-threads";
	        public static string RequestQueued = "aims.iis.requests-Queued";
	        public static string ActiveRequest = "aims.iis.active-sequest";
        }

        public static class Status
        {
            public const string Undefined = "aims.core.undefined";
	        public const string Running = "aims.core.running";
	        public const string Stopped = "aims.core.stopped";
	        public const string Paused = "aims.core.paused";
		}
    }
}