using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Types;
using LSNr.Statements;
using LsnCore.Expressions;
using LSNr.ControlStructures;
using LSNr.ScriptObjects;
using LSNr.ReaderRules;

namespace LSNr
{
	public interface IPreFunction : IPreScript
	{
		IFunctionContainer Parent { get; }
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "PreScript")]
	public sealed class PreScriptClassFunction : IPreFunction
	{
		public IBasePreScriptClass Parent { get; }
		IFunctionContainer IPreFunction.Parent => Parent;

		internal PreScriptClassFunction(IBasePreScriptClass parent, bool isConstructor = false)
		{
			Parent = parent; IsConstructor = isConstructor;
		}

		public IScope CurrentScope { get; set; } = new VariableTable(new List<Variable>());

		public bool Valid { get { return Parent.Valid; } set { Parent.Valid = value; } }

		public bool Mutable								=> false;
		public string Path								=> Parent.Path;

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

			new SetStateStatementRule()
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

		public bool IsConstructor { get; private set; }


		public bool GenericTypeExists(string name)		=> Parent.GenericTypeExists(name);
		public Function GetFunction(string name)		=> Parent.GetFunction(name);
		public GenericType GetGenericType(string name)	=> Parent.GetGenericType(name);
		public LsnType GetType(string name)				=> Parent.GetType(name);
		public TypeId GetTypeId(string name)			=> Parent.GetTypeId(name);
		public bool TypeExists(string name)				=> Parent.TypeExists(name);

		public int GetStateIndex(string name)			=> Parent.GetStateIndex(name);

		public SymbolType CheckSymbol(string name)
		{
			if (CurrentScope.VariableExists(name))
				return SymbolType.Variable;
			return Parent.CheckSymbol(name);
		}

		public void GenericTypeUsed(TypeId typeId)
		{
			Parent.GenericTypeUsed(typeId);
		}
	}
}
