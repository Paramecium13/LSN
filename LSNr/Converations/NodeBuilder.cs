using LsnCore;
using LsnCore.Statements;
using LsnCore.Types;
using LSNr.ReaderRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.Converations
{
	sealed class NodeBuilder : INode
	{
		readonly IConversation Conversation;

		public bool GenericTypeExists(string name) => Conversation.GenericTypeExists(name);
		public void GenericTypeUsed(TypeId typeId) => Conversation.GenericTypeUsed(typeId);
		public GenericType GetGenericType(string name) => Conversation.GetGenericType(name);
		public LsnType GetType(string name) => Conversation.GetType(name);
		public TypeId GetTypeId(string name) => Conversation.GetTypeId(name);
		public bool TypeExists(string name) => Conversation.TypeExists(name);
		public bool NodeExists(string name) => Conversation.NodeExists(name);

		readonly List<IBranch> Branches;

		public string Name { get; }

		public NodeBuilder(IConversation conversation, string name)
		{
			Conversation = conversation; Name = name;
		}

		public bool BranchExists(string name) => Branches.Any(b => b.Name == name);

		IEnumerable<Component> GetChoiceSegment()
		{
			foreach (var branch in Branches)
				yield return new RegisterChoiceStatement(branch.Condition, branch.Prompt, branch.Name);
			yield return new DisplayChoicesStatement();
		}
	}
}
