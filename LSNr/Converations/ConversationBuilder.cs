using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Types;
using LsnCore.Utilities;
using LSNr.ControlStructures;
using LSNr.Optimization;
using LSNr.ReaderRules;
using LSNr.Statements;

namespace LSNr.Converations
{
	sealed class ConversationBuilder : IConversation, IPreScript
	{
		internal static readonly IReadOnlyList<IStatementRule> _StatementRules = new IStatementRule[] {
			new LetStatementRule(),
			new ReasignmentStatementRule(),
			new BinExprReassignStatementRule("+=", BinaryOperation.Sum),
			new BinExprReassignStatementRule("-=", BinaryOperation.Difference),
			new BinExprReassignStatementRule("*=", BinaryOperation.Product),
			new BinExprReassignStatementRule("/=", BinaryOperation.Quotient),
			new BinExprReassignStatementRule("%=", BinaryOperation.Modulus),
			new BreakStatementRule(),
			new NextStatementRule(),
			new ConversationReturnStatementRule(),
			new SayStatementRule(),
			new GoToStatementRule(),
			new AttachStatementRule(),
			new GiveItemStatementRule(),
			new SetNodeStatementRule(),
			new EndConversationStatementRule()
		}.OrderBy(r => r.Order).ToList();

		internal static readonly IReadOnlyList<ControlStructureRule> _ControlStructureRules = new ControlStructureRule[] {
			new IfStructureRule(),
			new ElsIfStructureRule(),
			new ElseStructureRule(),
			new ChooseStructureRule(),
			new CaseStructureRule(),
			new ConditionedChoiceStructureRule(),
			new ForLoopStructureRule(),
			new IfLetStructureRule()
		}.OrderBy(r => r.Order).ToList();

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

		string Name;
		//readonly ISlice<Token> Args;

		readonly HashSet<string> NodeNames = new HashSet<string>();
		readonly List<INode> Nodes = new List<INode>();
		INode First;
		public ISlice<Token> StartTokens { get; set; }

		LsnFunction Function;

		public IScope CurrentScope { get; set; }

		public Variable JumpTargetVariable { get; private set; }

		public IReadOnlyList<IStatementRule> StatementRules => _StatementRules;

		public IReadOnlyList<ControlStructureRule> ControlStructureRules => _ControlStructureRules;

		public ConversationBuilder(IPreResource res, string name/*,ISlice<Token> args*/)
		{
			Resource = res; Name = name;//Args = args;
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

		List<Component> GetStartBlock()
		{
			if (StartTokens == null || StartTokens.Length == 0)
				return new List<Component>();
			CurrentScope = CurrentScope.CreateChild();
			var parser = new Parser(StartTokens, this);
			parser.Parse();
			var res = Parser.Consolidate(parser.Components);
			CurrentScope = CurrentScope.Pop(res);
			return res;
		}

		public void OnParsingSignatures(IPreResource resource)
		{
			Function = new LsnFunction(new List<Parameter>(), null, Name, resource.Path);
			resource.RegisterFunction(Function);
		}

		public void Parse()
		{
			CurrentScope = new VariableTable(new List<Variable>());
			// ToDo: Get parameters, add to scope.
			JumpTargetVariable = CurrentScope.CreateVariable("Jump Target", LsnType.int_, true);
			JumpTargetVariable.MarkAsUsed();

			var flattener = new ComponentFlattener();

			// Start Block
			flattener.ConvPartialFlatten(GetStartBlock(), "conv start", null);
			var i = 0;
			if(First == null)
			{
				i = 1;
				First = Nodes[0];
			}
			flattener.AddSetTargetStatement(First.Name, JumpTargetVariable);

			// First Node:
			First.Parse(flattener, CurrentScope);

			// Other nodes:
			for (; i < Nodes.Count; i++)
				Nodes[i].Parse(flattener, CurrentScope);

			Function.StackSize = (CurrentScope as VariableTable)?.MaxSize ?? -1;
			Function.Code = flattener.FinishFlatten();
		}
	}
}
