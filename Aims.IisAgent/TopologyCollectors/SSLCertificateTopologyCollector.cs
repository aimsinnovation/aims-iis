using Aims.Sdk;
using System;
using System.Collections.Generic;
using Aims.IISAgent.NodeRefCreators;
using Microsoft.Web.Administration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Aims.IISAgent.TopologyCollectors
{
	class SslCertificateTopologyCollector : ITopologyCollector
	{
		private readonly SiteNodeRefCreator _siteNodeRefCreator;
		private readonly SslCertificateNodeRefCreator _certificateNodeRefCreator;
		private readonly TimeSpan _warningSpan;
		private readonly TimeSpan _criticalSpan;

		public SslCertificateTopologyCollector(SiteNodeRefCreator siteNodeRefCreator, 
			SslCertificateNodeRefCreator certificateNodeRefCreator,
			TimeSpan warningSpan, TimeSpan criticalSpan)
		{
			_siteNodeRefCreator = siteNodeRefCreator;
			_certificateNodeRefCreator = certificateNodeRefCreator;
			_warningSpan = warningSpan;
			_criticalSpan = criticalSpan;
		}

		IEnumerable<Topology> ITopologyCollector.Collect()
		{
			using (var iisManager = new ServerManager())
			{
				X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
				store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
				var certificates = store.Certificates
					.Cast<X509Certificate2>()
					.ToDictionary(c => c.GetCertHashString().ToUpperInvariant());

				foreach (var site in iisManager.Sites)
				{
					var hashes = site.Bindings
						.Where(b => b.Protocol.Equals("https"))
						.Select(b => b.CertificateHash.ToHex());
					foreach (var hash in hashes)
					{
						X509Certificate2 cert;
						if (!certificates.TryGetValue(hash, out cert)) continue;
						yield return new Topology
						{
							Node = CreateNodeFromCert(cert),
							Links = new Link[]
							{
								new Link
								{
									From = _siteNodeRefCreator.CreateNodeRefFromObj(site),
									To = _certificateNodeRefCreator.CreateNodeRefFromObj(cert),
									LinkType = LinkType.Binding
								}
							}
						};
					}
				}

				store.Close();
			}
		}

		private Node CreateNodeFromCert(X509Certificate2 cert)
		{
			var nowTime = DateTime.Now;
			string status = nowTime > cert.NotAfter ? AgentConstants.Status.Expired:
				nowTime.Add(_criticalSpan) > cert.NotAfter ? AgentConstants.Status.CriticalSoonExpires:
				nowTime.Add(_warningSpan) > cert.NotAfter ? AgentConstants.Status.SoonExpires:
				AgentConstants.Status.Normal;
			return new Node
			{
				NodeRef = _certificateNodeRefCreator.CreateNodeRefFromObj(cert),
				Name = cert.FriendlyName,
				Status = status,
				CreationTime = cert.NotBefore.ToUniversalTime(),
				ModificationTime = DateTimeOffset.UtcNow,
				Properties = new Dictionary<string, string>{
						{AgentConstants.Properties.ExpirationDate, cert.NotAfter.ToUniversalTime().ToLongDateString() },
						{AgentConstants.Properties.IssueDate, cert.NotBefore.ToUniversalTime().ToLongDateString() },
					}
			};
		}
	}
}

