using System;
using System.Linq;
using LsnCore;
using LsnCore.Expressions;
using LsnCore.Runtime.Types;
using LsnCore.Types;
using LsnCore.Statements;
using LsnCore.Utilities;

namespace LSNr.Statements
{
	public sealed class LetStatementRule : IStatementRule
	{
		/// <inheritdoc/>
		public int Order => StatementRuleOrders.Base;

		/// <inheritdoc/>
		public bool PreCheck(Token token) => token.Value == "let";

		/// <inheritdoc/>
		public bool Check(ISlice<Token> tokens, IPreScript script) => true;

		/// <inheritdoc/>
		public Statement Apply(ISlice<Token> tokens, IPreScript script)
		{
			// let x = 1
			if (tokens.Count < 4)
				throw new LsnrParsingException(tokens[0], "Improperly formatted let statement.", script.Path);
			var i = 1;
			var mut = tokens[1].Value == "mut"; if (mut) i++;
			if(tokens[i].Type == TokenType.SyntaxSymbol && tokens[i].Value == ":")
			{
				i++;
				throw new NotImplementedException(); // Specify type...
			}
			if (tokens[i].Type != TokenType.Identifier)
				throw LsnrParsingException.UnexpectedToken(tokens[i], "an identifier", script.Path);
			var name = tokens[i++].Value;
			var symType = script.CheckSymbol(name);
			switch (symType)
			{//1F4CE
				case SymbolType.UniqueScriptObject:
				case SymbolType.Variable:
					var msg = $"Cannot name a new variable '{name}'. That name is already used for another variable.";
					if (script.CurrentScope.GetVariable(name).Mutable)
						msg += $"\r\n\uCEDC\u3DD8:\"It looks like you're trying to change the value of '{name}'. To do so here, simply leave off the word 'let'.\"";
					else
						msg += $"\r\n\uCEDC\u3DD8:\"It looks like you're trying to change the value of '{name}'. To do so, mark it as mutable where it was declared by putting 'mut' after 'let' and leave off the word 'let' here.\"";
					throw new LsnrParsingException(tokens[i - 1], msg, script.Path);
				case SymbolType.GlobalVariable:
				case SymbolType.Field:
				case SymbolType.Property:
				case SymbolType.ScriptClassMethod:
				case SymbolType.HostInterfaceMethod:
				case SymbolType.Function:
					throw new LsnrParsingException(tokens[i-1], $"Cannot name a new variable '{name}'. That name is already used for a {symType}.", script.Path);
			}
			if (tokens[i].Value != "=")
				throw LsnrParsingException.UnexpectedToken(tokens[i], "=", script.Path);
			i++;
			var val = Create.Express(tokens.CreateSliceAt(i), script);
			
			
			// ToDo: Move this logic into AssignmentStatement, IScope, or Variable.
			var variable = script.CurrentScope.CreateVariable(name, mut, val);
			var st = new AssignmentStatement(variable, val);
			variable.Assignment = st;

			return st;
		}
	}

	public class ReassignmentStatementRule : IStatementRule
	{
		/// <inheritdoc/>
		public virtual int Order => StatementRuleOrders.Reassign;

		/// <inheritdoc/>
		public virtual bool PreCheck(Token t) => true;

		/// <inheritdoc/>
		public virtual bool Check(ISlice<Token> tokens, IPreScript script) => tokens.Any(t => t.Value == "=");

		/// <inheritdoc/>
		public virtual Statement Apply(ISlice<Token> tokens, IPreScript script)
		{
			var i = tokens.IndexOf("=");
			var lTokens = tokens.CreateSliceTaking(i);
			return Make(Create.Express(lTokens, script), Create.Express(tokens.CreateSliceAt(i + 1), script), script, lTokens);
		}
		
		protected static Statement Make(IExpression lValue, IExpression rValue, IPreScript script, ISlice<Token> lTokens)
		{
			switch (lValue)
			{
				case VariableExpression v:
					{
						if (!v.Variable.Mutable)
							throw new LsnrParsingException(lTokens[0], $"Cannot reassign variable '{v.Variable.Name}' as it has not been marked as mutable.", script.Path);
						if (!v.Type.Subsumes(rValue.Type))
							throw LsnrParsingException.TypeMismatch(lTokens[0], v.Type.Name, rValue.Type.Name, script.Path);
						var r = new AssignmentStatement(v.Variable, rValue);
						v.Variable.AddReasignment(r);
						return r;
					}
				case CollectionValueAccessExpression col:
					{
						var colType = (ICollectionType) col.Collection.Type.Type;
						if (colType is ArrayType)
							throw new LsnrParsingException(lTokens[0], "Cannot reassign contents of an array.", script.Path);
						if (!colType.ContentsType.Subsumes(rValue.Type.Type))
							throw LsnrParsingException.TypeMismatch(lTokens[0], colType.ContentsType.Name, rValue.Type.Name, script.Path);
						return new CollectionValueAssignmentStatement(col.Collection, col.Index, rValue);
					}
				case FieldAccessExpression f:
					{
						if (f.Type.Type is RecordType)
						{
							throw new LsnrParsingException(lTokens[0], "Cannot reassign the contents of a record.", script.Path);
						}

						if (f.Type.Type is ScriptClass type && !(type.Fields[f.Index].Mutable) && !((script as PreScriptClassFunction)?.IsConstructor ?? false))
						{
							throw new LsnrParsingException(lTokens[0], $"The field '{type.Fields[f.Index].Name}' of script class '{type.Name}' is immutable.", script.Path);
						}

						if (!f.Type.Subsumes(rValue.Type))
						{
							throw LsnrParsingException.TypeMismatch(lTokens[0], f.Type.Name, rValue.Type.Name, script.Path);
						}

						return new FieldAssignmentStatement(f.Value, f.Index, rValue);
					}
				case GlobalVariableAccessExpression gv:
					throw new NotImplementedException();
				default:
					throw new LsnrParsingException(lTokens[0],
						$"Cannot assign a different value to '{lTokens.Select(t=> t.Value).Aggregate((x,y) =>x + " " + y)}'.", script.Path);
			}
		}
	}

	/// <summary>
	/// A binary expression reassignment statement rule (e.g. +=).
	/// </summary>
	/// <seealso cref="ReassignmentStatementRule" />
	public sealed class BinExprReassignStatementRule : ReassignmentStatementRule
	{
		private readonly string Value;
		private readonly BinaryOperation Operation;

		/// <summary>
		/// Initializes a new instance of the <see cref="BinExprReassignStatementRule"/> class.
		/// </summary>
		/// <param name="val">The text value of the operator, e.g. "+=".</param>
		/// <param name="op">The operator.</param>
		public BinExprReassignStatementRule(string val, BinaryOperation op) { Value = val; Operation = op; }

		/// <inheritdoc />
		public override bool Check(ISlice<Token> tokens, IPreScript script)
			=> tokens.Any(t => t.Value == Value);

		/// <inheritdoc />
		public override Statement Apply(ISlice<Token> tokens, IPreScript script)
		{
			var i = tokens.IndexOf(Value);
			var lTokens = tokens.CreateSliceTaking(i);
			var lValue = Create.Express(lTokens, script);
			var tmp = Create.Express(tokens.CreateSliceAt(i + 1),script);
			var argTypes = BinaryExpressionBase.GetArgTypes(lValue.Type, tmp.Type);
			var rValue = argTypes == BinaryOperationArgsType.String_Int || argTypes == BinaryOperationArgsType.String_String ?
				new StringBinaryExpression(lValue, tmp, Operation)
				: (IExpression)new BinaryArithmeticExpression(lValue, tmp, Operation);
			return Make(lValue, rValue, script, lTokens);
		}
	}
}