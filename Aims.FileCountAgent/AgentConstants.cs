namespace Aims.FileCountAgent
{
    public static class AgentConstants
    {
        public static class NodeRefPart
        {
            public const string Id = "id";
	        public const string InstanceName = "instatnce-name";
		}

        public static class NodeType
        {
            public const string Site = "aims.iis.site";
	        public const string AppPool = "aims.iis.app-pool";
        }

		public static class NodeProperties
		{
			public const string ApplicationPool = "aims.iis.site.property-app-pool";
		}
		
		//TODO repair
        public static class Service
        {
            public const string ApplicationName = "AIMS File Count Agent";
            public const string EventSource = "AIMS File Count Agent";
            public const string Log = "Application";
            public const string ServiceName = "aims-filecnt-agent";
        }

        public static class StatType
        {
            public const string FilesCreated = "aims.filecnt.files-created";
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