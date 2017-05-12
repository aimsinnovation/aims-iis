namespace Aims.IisAgent
{
    public static class AgentConstants
    {
	    public static class NodeRefPart
	    {
		    public const string Id = "id";
		    public const string InstanceName = "instance-name";
		    public const string MachineName = "machine-name";
	    }

	    public static class NodeType
	    {
		    public const string Site = "aims.iis.site";
		    public const string AppPool = "aims.iis.app-pool";
		    public const string Server = "aims.iis.server";
	    }

	    public static class NodeProperties
	    {
	    }

	    public static class Service
	    {
		    public const string ApplicationName = "AIMS Internet Information Services Agent";
		    public const string EventSource = "AIMS Internet Information Services Agent";
		    public const string Log = "Application";
		    public const string ServiceName = "aims-iis-agent";
	    }

	    public static class StatType
	    {
		    public const string RequestsPerSec = "aims.iis.requests-per-sec";
		    public const string TotalThreads = "aims.iis.total-threads";
		    public const string RequestQueued = "aims.iis.requests-queued";
		    public const string ActiveRequests = "aims.iis.active-requests";
		    public const string GetRequests = "aims.iis.get-requests";
		    public const string PostRequests = "aims.iis.post-requests";
		    public const string BytesSentPerSec = "aims.iis.bytes-sent-per-sec";
		    public const string BytesReceivedPerSec = "aims.iis.bytes-received-per-sec";
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