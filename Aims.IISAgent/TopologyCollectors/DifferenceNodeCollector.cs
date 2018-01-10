using System;
using System.Collections.Generic;
using System.Linq;
using Aims.Sdk;

namespace Aims.IISAgent.TopologyCollectors
{
	public class ChangedNodesEventArgs : EventArgs
	{
		public Node[] RemovedNodes { get; set; }
	}

	public class DifferenceNodeCollector : ITopologyCollector
	{
		private readonly ITopologyCollector _baseNodeCollector;
		private Node[] _prewNodes;

		public DifferenceNodeCollector(ITopologyCollector baseNodeCollector, IEnumerable<Node> prewNodes)
		{
			_baseNodeCollector = baseNodeCollector ?? throw new ArgumentNullException(nameof(baseNodeCollector));
			if (prewNodes == null)
				prewNodes = new List<Node>();
			_prewNodes = prewNodes.ToArray();
		}

		public event EventHandler OnTopologyChanged;

		public IEnumerable<Topology> Collect()
		{
			var newValues = _baseNodeCollector.Collect().ToArray();
			var removed = FindRemoved(
				newValues
					.Select(t => t.Node)
					.Distinct(new SpecialNodeComparer())
					.ToDictionary(n => n.NodeRef),
				_prewNodes).ToArray();
			if (removed.Length > 0 || newValues.Length != _prewNodes.Length)
				OnTopologyChanged?.Invoke(this, new ChangedNodesEventArgs
				{
					RemovedNodes = removed
				});

			_prewNodes = newValues.Select(t => t.Node).ToArray();
			return newValues;
		}

		private static IEnumerable<Node> FindRemoved(IDictionary<NodeRef, Node> newNodes, IEnumerable<Node> oldNodes)
		{
			var removedNodes = new List<Node>();
			foreach (var oldNode in oldNodes)
			{
				Node newNode;
				if (newNodes.TryGetValue(oldNode.NodeRef, out newNode))
					newNode.CreationTime = oldNode.CreationTime;
				else
					removedNodes.Add(oldNode);
			}
			return removedNodes;
		}
	}

	public class SpecialNodeComparer : IEqualityComparer<Node>
	{
		public bool Equals(Node x, Node y)
		{
			return y != null && x != null && Equals(x.NodeRef, y.NodeRef);
		}

		public int GetHashCode(Node obj)
		{
			return obj.NodeRef.GetHashCode();
		}
	}
}