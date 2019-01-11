using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.Expressions;
using LsnCore.Types;
using LsnCore.Statements;
using LsnCore.Utilities;

namespace LSNr.Statements
{
	public sealed class LetStatementRule : IStatementRule
	{
		public int Order => StatementRuleOrders.Base;

		public bool PreCheck(Token t) => t.Value == "let";

		public bool Check(ISlice<Token> tokens, IPreScript script) => true;

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
				throw new NotImplementedException();
			}
			if (tokens[i].Type != TokenType.Identifier)
				throw LsnrParsingException.UnexpectedToken(tokens[i], "an identifier", script.Path);
			var name = tokens[i++].Value;
			var symType = script.CheckSymbol(name);
			switch (symType)
			{
				case SymbolType.UniqueScriptObject:
				case SymbolType.Variable:
				case SymbolType.GlobalVariable:
				case SymbolType.Field:
				case SymbolType.Property:
				case SymbolType.ScriptClassMethod:
				case SymbolType.HostInterfaceMethod:
				case SymbolType.Function:
					throw new LsnrParsingException(tokens[i-1], $"Cannot name a new variable '{name}'. That name is already used for a {symType.ToString()}.", script.Path);
				default:
					break;
			}
			if (tokens[i].Value != "=")
				throw LsnrParsingException.UnexpectedToken(tokens[i], "=", script.Path);
			i++;
			var val = Create.Express(tokens.CreateSliceAt(i), script);
			var variable = script.CurrentScope.CreateVariable(name, mut, val);
			var st = new AssignmentStatement(variable.Index, val);
			variable.Assignment = st;
			return st;
		}
	}

	public class ReasignmentStatementRule : IStatementRule
	{
		public int Order => StatementRuleOrders.Last;

		public virtual bool PreCheck(Token t) => true;

		public virtual bool Check(ISlice<Token> tokens, IPreScript script) => tokens.Any(t => t.Value == "=");

		public virtual Statement Apply(ISlice<Token> tokens, IPreScript script)
		{
			throw new NotImplementedException();
		}

		protected static Statement Make(ISlice<Token> lTokens, IExpression rValue, IPreScript script)
		{
			switch (Create.Express(lTokens, script))
			{
				case VariableExpression v:
					{
						if (!v.Variable.Mutable)
							throw new LsnrParsingException(lTokens[0], $"Cannot reassign variable '{v.Variable.Name}' as it has not been marked as mutable.", script.Path);
						if (!v.Type.Subsumes(rValue.Type))
							throw LsnrParsingException.TypeMismatch(lTokens[0], v.Type.Name, rValue.Type.Name, script.Path);
							var r = new AssignmentStatement(v.Index, rValue);
						v.Variable.AddReasignment(r);
						return r;
					}
				case CollectionValueAccessExpression col:
					{
						var colType = col.Collection.Type.Type as ICollectionType;
						if (colType is VectorType)
							throw new LsnrParsingException(lTokens[0], "Cannot reassign contents of a vector.", script.Path);
						if (!colType.ContentsType.Subsumes(rValue.Type.Type))
							throw LsnrParsingException.TypeMismatch(lTokens[0], colType.ContentsType.Name, rValue.Type.Name, script.Path);
						return new CollectionValueAssignmentStatement(col.Collection, col.Index, rValue);
					}
				case FieldAccessExpression f:
					{
						if (f.Type.Type is RecordType)
							throw new LsnrParsingException(lTokens[0], "Cannot reassign the contents of a record.", script.Path);
						/*if(f.Type.Type is ScriptClass type && !(type.Fields[f.Index].Mutable))
							throw ...;*/
						if (!f.Type.Subsumes(rValue.Type))
							throw LsnrParsingException.TypeMismatch(lTokens[0], f.Type.Name, rValue.Type.Name, script.Path);
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
}