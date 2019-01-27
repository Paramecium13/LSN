using LsnCore;
using LsnCore.Statements;
using LsnCore.Types;
using LSNr.ReaderRules;
using LSNr.Optimization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Utilities;
using LSNr.ControlStructures;
using LSNr.Statements;

namespace LSNr.Converations
{
	sealed class NodeBuilder : INode, IPreFunction
	{
		public ConversationBuilder _Conversation;
		public IConversation Conversation => _Conversation;
		public IFunctionContainer Parent => _Conversation.Parent;

		public bool GenericTypeExists(string name) => _Conversation.GenericTypeExists(name);
		public void GenericTypeUsed(TypeId typeId) => _Conversation.GenericTypeUsed(typeId);
		public GenericType GetGenericType(string name) => _Conversation.GetGenericType(name);
		public LsnType GetType(string name) => _Conversation.GetType(name);
		public TypeId GetTypeId(string name) => _Conversation.GetTypeId(name);
		public bool TypeExists(string name) => _Conversation.TypeExists(name);
		public bool NodeExists(string name) => _Conversation.NodeExists(name);
		public bool Mutable => _Conversation.Mutable;
		public bool Valid { get => _Conversation.Valid; set => _Conversation.Valid = value; }
		public string Path => _Conversation.Path;

		readonly List<IBranch> Branches = new List<IBranch>();

		public string Name { get; }

		public ISlice<Token> StartBlockTokens { get; set; }
		public IScope CurrentScope { get; set; }

		public IReadOnlyList<IStatementRule> StatementRules => _Conversation.StatementRules;

		public IReadOnlyList<ControlStructureRule> ControlStructureRules => _Conversation.ControlStructureRules;

		public NodeBuilder(ConversationBuilder conversation, string name)
		{
			if (name == "node")
				throw new ApplicationException("You're doing this wrong...");
			_Conversation = conversation; Name = name;
		}

		public bool BranchExists(string name) => Branches.Any(b => b.Name == name);

		public void RegisterBranch(IBranch branch)
		{
			Branches.Add(branch);
		}

		List<Component> GetStartBlock()
		{
			if (StartBlockTokens == null || StartBlockTokens.Length == 0)
				return new List<Component>();
			CurrentScope = CurrentScope.CreateChild();
			var parser = new Parser(StartBlockTokens, this);
			parser.Parse();
			var res = Parser.Consolidate(parser.Components);
			CurrentScope = CurrentScope.Pop(res);
			return res;
		}

		List<Component> GetChoiceSegment()
		{
			var ls = new List<Component>();
			foreach (var branch in Branches)
			{
				branch.Parse(this);
				if (branch.Prompt == null)
					throw new ApplicationException($"Branch {branch.Name} has no prompt.");
				if (branch.ActionTokens == null)
					throw new ApplicationException($"Branch {branch.Name} has no action.");

				ls.Add(new RegisterChoiceStatement(branch.Condition ?? LsnBoolValue.GetBoolValue(true), branch.Prompt, Name + " " + branch.Name));
			}
			ls.Add(new DisplayChoicesStatement());
			return ls;
		}

		public void Parse(ComponentFlattener flattener, IScope scope)
		{
			CurrentScope = scope;
			if (StartBlockTokens != null)
			{
				flattener.ConvPartialFlatten(GetStartBlock(), Name + " ", Name + " Start");
				flattener.AddSetTargetStatement(Name, _Conversation.JumpTargetVariable);
			}
			else
				flattener.AddLabelAlias(Name + " Start", Name);
			flattener.ConvPartialFlatten(GetChoiceSegment(), Name + " ", Name);
			foreach (var branch in Branches)
			{
				CurrentScope = CurrentScope.CreateChild();
				var parser = new Parser(branch.ActionTokens, this);
				parser.Parse();
				var res = Parser.Consolidate(parser.Components);
				CurrentScope = CurrentScope.Pop(res);
				var label = Name + " " + branch.Name;
				flattener.ConvPartialFlatten(res, label + " ", label);
				flattener.AddOptionalJumpToTargetStatement(_Conversation.JumpTargetVariable);
			}
		}

		public SymbolType CheckSymbol(string name)
		{
			return _Conversation.CheckSymbol(name);
		}

		public Function GetFunction(string name)
		{
			return _Conversation.GetFunction(name);
		}
	}
}
