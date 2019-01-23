using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using LsnCore.Utilities;
using LSNr.ControlStructures;
using LSNr.ReaderRules;
using LSNr.Statements;

namespace LSNr.Converations
{
	sealed class ConversationBuilder : IConversation, IPreScript
	{
		readonly IPreResource Resource;

		public bool GenericTypeExists(string name) => Resource.GenericTypeExists(name);
		public void GenericTypeUsed(TypeId typeId) => Resource.GenericTypeUsed(typeId);
		public GenericType GetGenericType(string name) => Resource.GetGenericType(name);
		public LsnType GetType(string name) => Resource.GetType(name);
		public TypeId GetTypeId(string name) => Resource.GetTypeId(name);
		public bool TypeExists(string name) => Resource.TypeExists(name);

		public bool Mutable => false;
		public bool Valid { get => Resource.Valid; set => Resource.Valid = value; }
		public string Path => Resource.Path;
		public Function GetFunction(string name) => Resource.GetFunction(name);

		readonly HashSet<string> NodeNames = new HashSet<string>();
		readonly List<INode> Nodes = new List<INode>();
		INode First;

		public ISlice<Token> StartTokens { get; set; }
		public IScope CurrentScope { get; set; }


		public IReadOnlyList<IStatementRule> StatementRules => throw new NotImplementedException();

		public IReadOnlyList<ControlStructureRule> ControlStructureRules => throw new NotImplementedException();

		public ConversationBuilder(IPreResource res)
		{
			Resource = res;
		}

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

		public SymbolType CheckSymbol(string name)
		{
			if (NodeExists(name))
				return SymbolType.Undefined;
			return Resource.CheckSymbol(name);
		}

	}
}
