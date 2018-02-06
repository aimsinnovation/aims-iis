namespace Aims.IISAgent
{
	public static class AgentConstants
	{
		public static class NodeRefPart
		{
			public const string Id = "id";
			public const string InstanceName = "instance-name";
			public const string MachineName = "machine-name";
			public const string SslCertificateFriendlyName = "ssl-certificate-name";
		}

		public static class NodeType
		{
			public const string Site = "aims.iis.site";
			public const string AppPool = "aims.iis.app-pool";
			public const string Server = "aims.iis.server";

			public const string SslCert = "aims.iis.ssl-cert";
		}

		public static class Properties
		{
			public const string IssueDate = "aims.iis.issue-date";
			public const string ExpirationDate = "aims.iis.expiration-date";
		}

		public static class Service
		{
			public const string ApplicationName = "AIMS IIS Agent";
			public const string EventSource = "AIMS IIS Agent";
			public const string Log = "Application";
			public const string ServiceName = "aims-iis-agent";
		}

		public static class StatType
		{
			public const string Requests = "aims.iis.requests";
			public const string TotalThreads = "aims.iis.total-threads";
			public const string RequestQueued = "aims.iis.requests-queued";
			public const string ActiveRequests = "aims.iis.active-requests";
			public const string GetRequests = "aims.iis.get-requests";
			public const string PostRequests = "aims.iis.post-requests";
			public const string BytesSent = "aims.iis.bytes-sent";
			public const string BytesReceived = "aims.iis.bytes-received";
			public const string ActiveConnections = "aims.iis.active-connections";

			// ReSharper disable once InconsistentNaming
			public const string Error4xx = "aims.iis.errors-4xx";

			// ReSharper disable once InconsistentNaming
			public const string Error5xx = "aims.iis.errors-5xx";

			public const string LogonSuccessful = "aims.iis.logons-successful";
			public const string LogonFailed = "aims.iis.logons-failed";
			public const string Undefined = "null";
		}

		public static class Status
		{
			public const string Undefined = "aims.core.undefined";
			public const string Running = "aims.core.running";
			public const string Stopped = "aims.core.stopped";
			public const string Paused = "aims.core.paused";

			public const string Normal = "aims.iis.ssl-cert-valid";
			public const string SoonExpires = "aims.iis.ssl-cert-expires-soon";
			public const string CriticalSoonExpires = "aims.iis.ssl-cert-critical-expires-soon";
			public const string Expired = "aims.iis.ssl-cert-expired";
		}

		public static class Pipes
		{
			//If you chancge this line, change another line in 'Aims.IisAgent.Module' with that name
			public const string NameOfMainPipe = "aims-iis-agent.main-named-pipe";
		}

		public static class InstallConstatnts
		{
			public const string IisModuleName = "AimsIisAgentModule";

			//string, that give me the GAC.
			public const string IisModuleType =
					"Aims.IISAgent.Module.AimsIisModule, Aims.IISAgent.Module, Version=1.0.0.0, Culture=neutral, PublicKeyToken=671f7217e3d88943";
		}
	}
}