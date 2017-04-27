namespace Aims.FileCountAgent
{
    public static class AgentConstants
    {
        public static class NodeRefPart
        {
            public const string Id = "id";
        }

        public static class NodeType
        {
            public const string Site = "aims.iis.site";
        }

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
        }
    }
}