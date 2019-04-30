using System;
using System.Collections.Generic;
using System.Linq;
using Aims.IISAgent.NodeRefCreators;
using Aims.IISAgent.Pipes;
using Aims.Sdk;
using Microsoft.Web.Administration;

namespace Aims.IISAgent.Collectors.BufferedCollector
{
	public partial class MessageConverterToStatPoint : IConverterToStatPoint<Message>
	{
		private const double CacheElapsedTime = 50;//seconds
		private readonly Cache<Dictionary<string, string>> _siteCache;
        private readonly INodeRefCreator _nodeRefCreator;

		public MessageConverterToStatPoint(INodeRefCreator nodeRefCreator)
		{
			if (nodeRefCreator == null) throw new ArgumentNullException(nameof(nodeRefCreator));
			_nodeRefCreator = nodeRefCreator;
            _siteCache = new Cache<Dictionary<string, string>>(GetMapIdToSiteName, TimeSpan.FromSeconds(CacheElapsedTime), 1, 1);
		}

		//possible return null if can't find site with that binds
		public StatPoint ConvertPoint(Message item)
		{
			if (_siteCache.Value.TryGetValue(item.SiteId, out string siteName) && TryFindStatType(item, out string statType))
			{
				return new StatPoint
				{
					NodeRef = _nodeRefCreator.CreateFromInstanceName(siteName),
					StatType = statType,
					Time = item.DateTime,
					Value = 1,
				};
			}
			return null;
		}

        private static Dictionary<string, string> GetMapIdToSiteName()
        {
            using (var iisManager = new ServerManager())
            {
                return iisManager.Sites.ToDictionary(s => s.Id.ToString(), s => s.Name);
            }
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
	}
}