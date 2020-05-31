using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Expressions;
using LsnCore.Types;
using LSNr.ControlStructures;
using LSNr.Statements;

namespace LSNr
{
	public class PreFunction : IPreScript
	{
		private readonly IPreScript Resource;

		internal PreFunction(IPreScript resource)
		{
			Resource = resource;
		}

		private static readonly IReadOnlyList<IStatementRule> _StatementRules = new IStatementRule[] {
			new LetStatementRule(),
			new ReasignmentStatementRule(),
			new BinExprReassignStatementRule("+=", BinaryOperation.Sum),
			new BinExprReassignStatementRule("-=", BinaryOperation.Difference),
			new BinExprReassignStatementRule("*=", BinaryOperation.Product),
			new BinExprReassignStatementRule("/=", BinaryOperation.Quotient),
			new BinExprReassignStatementRule("%=", BinaryOperation.Modulus),
			new BreakStatementRule(),
			new NextStatementRule(),
			new ReturnStatementRule(),
			new SayStatementRule(),
			new GoToStatementRule(),
			new AttachStatementRule(),
			new GiveItemStatementRule(),
		}.OrderBy(r => r.Order).ToList();

		public IReadOnlyList<IStatementRule> StatementRules => _StatementRules;

		private static readonly IReadOnlyList<ControlStructureRule> _ControlStructureRules = new ControlStructureRule[] {
			new IfStructureRule(),
			new ElsIfStructureRule(),
			new ElseStructureRule(),
			new ChooseStructureRule(),
			new CaseStructureRule(),
			new ConditionedChoiceStructureRule(),
			new ForLoopStructureRule(),
			new IfLetStructureRule()
		}.OrderBy(r => r.Order).ToList();

		public IReadOnlyList<ControlStructureRule> ControlStructureRules => _ControlStructureRules;

		public IScope CurrentScope { get; set; } = new VariableTable();

		public bool Valid {get => Resource.Valid; set => Resource.Valid = value;}

		public bool GenericTypeExists(string name)		=> Resource.GenericTypeExists(name);
		public Function GetFunction(string name)		=> Resource.GetFunction(name);
		public GenericType GetGenericType(string name)	=> Resource.GetGenericType(name);
		public LsnType GetType(string name)				=> Resource.GetType(name);
		public bool TypeExists(string name)				=> Resource.TypeExists(name);
		public string Path								=> Resource.Path;
		public TypeId GetTypeId(string name)			=> Resource.GetTypeId(name);

		public void GenericTypeUsed(TypeId typeId) 		=> Resource.GenericTypeUsed(typeId);

		public SymbolType CheckSymbol(string name)
		{
			if (CurrentScope.VariableExists(name))
				return SymbolType.Variable;
			return Resource.CheckSymbol(name);
		}
	}
}
