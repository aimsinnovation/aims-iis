using System;
using System.Collections.Generic;
using System.Linq;
using Aims.IISAgent.Module.Pipes;
using Aims.IISAgent.NodeRefCreators;
using Aims.Sdk;
using Microsoft.Web.Administration;

namespace Aims.IISAgent.PerformanceCounterCollectors.BufferedCollector
{
	public partial class MessageConverterToStatPoint : IConverterToStatPoint<Message>
	{
		private readonly INodeRefCreator _nodeRefCreator;

		public MessageConverterToStatPoint(INodeRefCreator nodeRefCreator)
		{
			_nodeRefCreator = nodeRefCreator;
		}

		//possible return null if can't find site with that binds
		public StatPoint ConvertPoint(Message item)
		{
			item.Domain = item.Domain.ReplaceIfLocalhostOrIp();
			item.Path = item.Path;
			if (SiteNameSearch(item, out string siteName) && TryFindStatType(item, out string statType))
			{
				return new StatPoint
				{
					NodeRef = _nodeRefCreator.CreateFromInstanceName(siteName),
					StatType = statType,
					Time = item.DateTime,
					Value = 1,
				};
			}
			else
				return null;
		}

		private static bool TryFindStatType(Message msg, out string statType)
		{
			statType = string.Empty;
			if (!string.Equals(msg.StatType, string.Empty))
				statType = msg.StatType;
			else if (msg.Code >= 500 && msg.Code < 600)
				statType = AgentConstants.StatType.Error5xx;
			else if (msg.Code >= 400 && msg.Code < 500)
				statType = AgentConstants.StatType.Error4xx;
			//else if (msg.Code == 200)//TODO debug line
			//	statType = AgentConstants.StatType.Undefined;
			else
				return false;
			return true;
		}

		private Dictionary<SiteBindings, string> GetMapBindToSiteName()
		{
			Dictionary<SiteBindings, string> answer = new Dictionary<SiteBindings, string>();
			using (var iisManager = new ServerManager())
			{
				foreach (var site in iisManager.Sites)
				{
					if (site.State != ObjectState.Started)//TODO write why
						continue;
					foreach (var bind in site.Bindings)
					{
						if (bind.EndPoint != null)
							foreach (var application in site.Applications)
							{
								answer.Add(new SiteBindings
								{
									Domain = bind.Host,
									Port = bind.EndPoint.Port,
									Protocol = bind.Protocol,
									Application = application.Path
								}, site.Name);
							}
					}
				}
			}

			return answer;
		}

		//search N times, trying find max path equals.
		//should help if resource of site in deep deep folder
		private bool SiteNameSearch(Message m, out string siteName)
		{
			var bindMapper = GetMapBindToSiteName();

			string[] segments = m.Path.Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray();
			int index = segments.Length;
			while (index >= 0)
			{
				string path = "/" + string.Join("/", segments.Take(index));
				var bind = new SiteBindings
				{
					Domain = m.Domain,
					Port = m.Port,
					Protocol = m.Scheme,
					Application = path,
				};
				if (bindMapper.TryGetValue(bind, out siteName)) return true;
				--index;
			}
			siteName = string.Empty;
			return false;
		}
	}
}