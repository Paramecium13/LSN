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
	sealed class NodeBuilder : INode, IPreScript
	{
		readonly ConversationBuilder Conversation;

		public bool GenericTypeExists(string name) => Conversation.GenericTypeExists(name);
		public void GenericTypeUsed(TypeId typeId) => Conversation.GenericTypeUsed(typeId);
		public GenericType GetGenericType(string name) => Conversation.GetGenericType(name);
		public LsnType GetType(string name) => Conversation.GetType(name);
		public TypeId GetTypeId(string name) => Conversation.GetTypeId(name);
		public bool TypeExists(string name) => Conversation.TypeExists(name);
		public bool NodeExists(string name) => Conversation.NodeExists(name);
		public bool Mutable => Conversation.Mutable;
		public bool Valid { get => Conversation.Valid; set => Conversation.Valid = value; }
		public string Path => Conversation.Path;

		readonly List<IBranch> Branches = new List<IBranch>();

		public string Name { get; }

		public ISlice<Token> StartBlockTokens { get; set; }
		public IScope CurrentScope { get; set; }

		public IReadOnlyList<IStatementRule> StatementRules => Conversation.StatementRules;

		public IReadOnlyList<ControlStructureRule> ControlStructureRules => Conversation.ControlStructureRules;

		public NodeBuilder(ConversationBuilder conversation, string name)
		{
			Conversation = conversation; Name = name;
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
				ls.Add(new RegisterChoiceStatement(branch.Condition, branch.Prompt, Name + " " + branch.Name));
			ls.Add(new DisplayChoicesStatement());
			return ls;
		}

		public void Parse(ComponentFlattener flattener)
		{
			if (StartBlockTokens != null)
			{
				flattener.ConvPartialFlatten(GetStartBlock(), Name + " Start", Name + " ");
				flattener.AddSetTargetStatement(Name, Conversation.JumpTargetVariable);
			}
			else flattener.AddLabelAlias(Name + " Start", Name);
			flattener.ConvPartialFlatten(GetChoiceSegment(), Name, Name + " ");
			foreach (var branch in Branches)
			{
				CurrentScope = CurrentScope.CreateChild();
				var parser = new Parser(branch.ActionTokens, this);
				parser.Parse();
				var res = Parser.Consolidate(parser.Components);
				CurrentScope = CurrentScope.Pop(res);
				var label = Name + " " + branch.Name;
				flattener.ConvPartialFlatten(res, label + " ", label);
				flattener.AddOptionalJumpToTargetStatement(Conversation.JumpTargetVariable);
			}
		}

		public SymbolType CheckSymbol(string name)
		{
			return Conversation.CheckSymbol(name);
		}

		public Function GetFunction(string name)
		{
			return Conversation.GetFunction(name);
		}
	}
}
