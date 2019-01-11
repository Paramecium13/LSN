using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore;
using LsnCore.ControlStructures;
using LsnCore.Expressions;
using LsnCore.Statements;
using LsnCore.Types;
using LsnCore.Utilities;
using LsnCore.Values;

namespace LSNr.ControlStructures
{
	public sealed class ForLoopRule : ControlStructureRule
	{
		public override bool PreCheck(Token t) => t.Value == "for";

		public override bool Check(ISlice<Token> tokens, IPreScript script) => true;

		public override ControlStructure Apply(ISlice<Token> head, ISlice<Token> body, IPreScript script)
		{
			var i = 1;
			if (head.Length < 4 || head[i].Type != TokenType.Identifier)
				throw new LsnrParsingException(head[0], "Incorrectly formatted for loop.", script.Path);
			var vName = head[i].Value;
			if (script.CurrentScope.HasVariable(vName))
				throw new LsnrParsingException(head[1], $"A variable named '{vName}' already exists. Cannot reuse it in for loop even if it is marked as 'mut'", script.Path);
			if (head[++i].Value != "in") // i == 2
				throw LsnrParsingException.UnexpectedToken(head[i], "in", script.Path);
			i++; // i == 3; points to expression.
			var expr = Create.Express(head.Skip(3), script);
			if (expr.Type.Type is ICollectionType collType)
			{
				script.CurrentScope = script.CurrentScope.CreateChild();
				var index = script.CurrentScope.CreateVariable(vName + " index", LsnType.int_);
				IExpression collection;
				AssignmentStatement state = null;
				if (ForInCollectionLoop.CheckCollectionVariable(expr))
					collection = expr;
				else
				{
					var collVar = script.CurrentScope.CreateVariable(vName + " collection", expr.Type.Type);
					collection = collVar.AccessExpression;
					state = new AssignmentStatement(collVar.Index, expr);
					collVar.Assignment = state;
					collVar.AddUser(state);
					collVar.MarkAsUsed();
				}
				var iterator = script.CurrentScope.CreateIteratorVariable(vName, collection, index);
				iterator.MarkAsUsed();
				var p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				script.CurrentScope = script.CurrentScope.Pop(components);
				return new ForInCollectionLoop(index, iterator, collection, components)
				{ Statement = state };
			}
			if (expr.Type.Type == RangeType.Instance)
			{
				script.CurrentScope = script.CurrentScope.CreateChild();
				var index = script.CurrentScope.CreateVariable(vName, LsnType.int_);
				var p = new Parser(body, script);
				p.Parse();
				var components = Parser.Consolidate(p.Components);
				script.CurrentScope = script.CurrentScope.Pop(components);
				var loop = new ForInRangeLoop(index, components);
				switch (expr)
				{
					case RangeExpression rExp:
						if (rExp.Start is LsnValue v1)
						{
							loop.Start = new LsnValue(v1.IntValue);
							// statement for end...
							var endVar = script.CurrentScope.CreateVariable("# " + vName, LsnType.int_);
							endVar.MarkAsUsed();
							var st1 = new AssignmentStatement(endVar.Index, rExp.End);
							endVar.Assignment = st1;
							loop.Statement = st1;
							endVar.AddUser(st1);
							loop.End = endVar.AccessExpression;
						}
						else if (rExp.End is LsnValue v2)
						{
							loop.End = v2;
							loop.Start = rExp.Start;
						}
						else goto default;
						break;
					case LsnValue val:
						var range = val.Value as RangeValue;
						loop.Start = new LsnValue(range.Start);
						loop.End = new LsnValue(range.End);
						break;
					case VariableExpression v:
						loop.Start = new FieldAccessExpression(v, 0);
						loop.End = new FieldAccessExpression(v, 1);
						v.Variable.AddUser(loop.Start);
						v.Variable.AddUser(loop.End);
						break;
					default:
						var rVar = script.CurrentScope.CreateVariable("# " + vName, RangeType.Instance);
						rVar.MarkAsUsed();
						var st = new AssignmentStatement(rVar.Index, expr);
						rVar.AddUser(st);
						rVar.Assignment = st;
						loop.Statement = st;

						loop.Start = new FieldAccessExpression(rVar.AccessExpression, 0);
						loop.End = new FieldAccessExpression(rVar.AccessExpression, 1);
						rVar.AddUser(loop.Start);
						rVar.AddUser(loop.End);
						break;
				}
				return loop;
			}
			throw new LsnrParsingException(head[3], "...", script.Path);
		}
	}
}
