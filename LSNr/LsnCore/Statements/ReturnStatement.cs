using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSNr;
using Syroot.BinaryData;
using MoreLinq;

namespace LsnCore.Statements
{
	/// <summary>
	/// Returns from a function, possibly with a value
	/// </summary>
	public class ReturnStatement : Statement
	{
		public IExpression Value { get; set; }

		public ReturnStatement(IExpression e)
		{
			Value = e;
		}

#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			if(Value != null) i.ReturnValue = Value.Eval(i);
			return InterpretValue.Return;
		}
#endif

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Value == null) return;
			if (oldExpr.Equals(Value))
				Value = newExpr;
			else
				Value.Replace(oldExpr, newExpr);
		}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			if(Value == null)
				writer.Write(StatementCode.Return);
			else
			{
				writer.Write(StatementCode.ReturnValue);
				Value.Serialize(writer, resourceSerializer);
			}
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			if (!(!Value?.Equals(LsnValue.Nil) ?? false)) yield break;
			yield return Value;
			foreach (var expr in Value.SelectMany(e => e))
				yield return expr;
		}

		/// <inheritdoc />
		protected override IEnumerable<PreInstruction> GetInstructions(string target)
		{
			if (Value != null)
			{
				return Value.GetInstructions().Append(new SimplePreInstruction(OpCode.Ret, 0));
			}

			yield return new SimplePreInstruction(OpCode.Ret, 0);
		}
	}
}
