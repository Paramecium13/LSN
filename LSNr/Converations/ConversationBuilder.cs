using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using LSNr.ReaderRules;

namespace LSNr.Converations
{
	sealed class ConversationBuilder : IConversation
	{
		readonly IPreResource Resource;

		public bool GenericTypeExists(string name) => Resource.GenericTypeExists(name);
		public void GenericTypeUsed(TypeId typeId) => Resource.GenericTypeUsed(typeId);
		public GenericType GetGenericType(string name) => Resource.GetGenericType(name);
		public LsnType GetType(string name) => Resource.GetType(name);
		public TypeId GetTypeId(string name) => Resource.GetTypeId(name);
		public bool TypeExists(string name) => Resource.TypeExists(name);

		readonly HashSet<string> NodeNames = new HashSet<string>();
		readonly List<INode> Nodes = new List<INode>();
		INode First;

		public void RegisterNode(INode node, bool first)
		{
			if (first)
			{
				if (First != null)
					throw new ApplicationException("...");
				First = node;
			}
			else
				Nodes.Add(node);
			NodeNames.Add(node.Name);
		}

		public bool NodeExists(string name) => NodeNames.Contains(name);
	}
}
