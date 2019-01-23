using LsnCore;
using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Types;
using LsnCore.Utilities;
using LSNr.ReaderRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr.Converations
{
	sealed class BranchBuilder : IBranch
	{
		readonly IConversation Conversation;

		public bool GenericTypeExists(string name) => Conversation.GenericTypeExists(name);
		public void GenericTypeUsed(TypeId typeId) => Conversation.GenericTypeUsed(typeId);
		public GenericType GetGenericType(string name) => Conversation.GetGenericType(name);
		public LsnType GetType(string name) => Conversation.GetType(name);
		public TypeId GetTypeId(string name) => Conversation.GetTypeId(name);
		public bool TypeExists(string name) => Conversation.TypeExists(name);

		public string Name { get; }

		public ISlice<Token> ConditionTokens { get; set; }
		public IExpression Condition { get; set; }

		public ISlice<Token> PromptTokens { get; set; }
		public IExpression Prompt { get; set; }

		public ISlice<Token> ActionTokens { get; set; }

		public BranchBuilder(string name, IConversation conversation)
		{
			Name = name; Conversation = conversation;
		}
	}
}
