using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Aims.IISAgent.NodeRefCreators;
using Aims.Sdk;
using Microsoft.Web.Administration;

namespace Aims.IISAgent.TopologyCollectors
{
	internal class SslCertificateTopologyCollector : ITopologyCollector
	{
		private readonly INodeRefCreator<X509Certificate2> _certificateNodeRefCreator;
		private readonly TimeSpan _criticalSpan;
		private readonly INodeRefCreator<Site> _siteNodeRefCreator;
		private readonly TimeSpan _warningSpan;

		public SslCertificateTopologyCollector(INodeRefCreator<Site> siteNodeRefCreator,
			INodeRefCreator<X509Certificate2> certificateNodeRefCreator,
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
			var status = GetStatus(cert);

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

		private string GetStatus(X509Certificate2 cert)
		{
			var nowTime = DateTime.Now;
			return nowTime > cert.NotAfter
				? AgentConstants.Status.Expired
				: nowTime.Add(_criticalSpan) > cert.NotAfter
					? AgentConstants.Status.CriticalSoonExpires
					: nowTime.Add(_warningSpan) > cert.NotAfter
						? AgentConstants.Status.SoonExpires
						: AgentConstants.Status.Normal;
		}
	}
}