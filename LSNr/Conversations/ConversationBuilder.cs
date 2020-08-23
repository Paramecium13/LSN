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
	internal sealed class ConversationBuilder : IConversation, IPreFunction
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

		public IFunctionContainer Parent { get; }

		public bool GenericTypeExists(string name) => Parent.GenericTypeExists(name);
		public void GenericTypeUsed(TypeId typeId) => Parent.GenericTypeUsed(typeId);
		public GenericType GetGenericType(string name) => Parent.GetGenericType(name);
		public LsnType GetType(string name) => Parent.GetType(name);
		public TypeId GetTypeId(string name) => Parent.GetTypeId(name);
		public bool TypeExists(string name) => Parent.TypeExists(name);

		public bool Valid { get => Parent.Valid; set => Parent.Valid = value; }
		public string Path => Parent.Path;
		public Function GetFunction(string name) => Parent.GetFunction(name);

		private readonly string Name;
		private readonly ISlice<Token> Args;

		private readonly HashSet<string> NodeNames = new HashSet<string>();
		private readonly List<INode> Nodes = new List<INode>();
		private INode First;
		private readonly List<IConversationVariable> PreStartConvVars = new List<IConversationVariable>();
		public ISlice<Token> StartTokens { get; set; }
		private readonly List<IConversationVariable> PostStartConvVars = new List<IConversationVariable>();

		private IProcedure Function;

		public IScope CurrentScope { get; set; }

		public Variable JumpTargetVariable { get; private set; }

		public IReadOnlyList<IStatementRule> StatementRules => _StatementRules;

		public IReadOnlyList<ControlStructureRule> ControlStructureRules => _ControlStructureRules;

		public bool IsVirtual { get; }

		public ConversationBuilder(IFunctionContainer res, string name, ISlice<Token> args, bool isVirtual = false)
		{
			Parent = res; Name = name; Args = args ?? Slice<Token>.Create(new Token[0], 0, 0); IsVirtual = isVirtual;
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

		public void RegisterConvVar(IConversationVariable convVar)
		{
			if (StartTokens == null)
				PreStartConvVars.Add(convVar);
			else PostStartConvVars.Add(convVar);
		}

		public bool NodeExists(string name) => NodeNames.Contains(name);

		public SymbolType CheckSymbol(string name)
		{
			if (NodeExists(name))
				return SymbolType.Undefined; // Add symbol type for this?
			if (CurrentScope.VariableExists(name))
				return SymbolType.Variable;
			return Parent.CheckSymbol(name);
		}

		private List<Component> GetStartBlock()
		{
			if (StartTokens == null || StartTokens.Length == 0)
				return new List<Component>();
			this.PushScope();
			var res = Parser.Parse(StartTokens, this);
			this.PopScope(res);
			return res;
		}

		public void OnParsingSignatures(IFunctionContainer resource)
		{
			CurrentScope = new VariableTable();
			// Get parameters, add to scope.
			var args = Parent.ParseParameters(Args);
			foreach (var arg in args)
				CurrentScope.CreateVariable(arg);
			Function = resource.CreateFunction(args, null, Name, IsVirtual);
		}

		public void Parse()
		{
			JumpTargetVariable = CurrentScope.CreateVariable("Jump Target", LsnType.int_, true);
			JumpTargetVariable.MarkAsUsed();

			var flattener = new ComponentFlattener();

			// Conversation variables before start block:
			foreach (var cv in PreStartConvVars)
				cv.Parse(flattener, this);

			// Start Block
			flattener.ConvPartialFlatten(GetStartBlock(), "conv start", null);
			var i = 0;
			if(First == null)
			{
				i = 1;
				First = Nodes[0];
			}
			flattener.AddSetTargetStatement(First.Name + " Start", JumpTargetVariable);

			// Conversation variables after start block:
			foreach (var cv in PostStartConvVars)
				cv.Parse(flattener, this);

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
