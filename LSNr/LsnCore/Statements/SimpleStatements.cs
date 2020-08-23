using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	/// <summary>
	/// Exit the innermost loop.
	/// </summary>
	public class BreakStatement : Statement
	{
		public override IEnumerator<IExpression> GetEnumerator()
		{
			throw new NotImplementedException();
		}

#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			throw new InvalidOperationException();
		}
#endif

		public override void Replace(IExpression oldExpr, IExpression newExpr){}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			throw new InvalidOperationException();
		}
	}

	/// <summary>
	/// Move on to the next iteration of the innermost loop.
	/// </summary>
	public class NextStatement : Statement
	{
		public override IEnumerator<IExpression> GetEnumerator()
		{
			throw new NotImplementedException();
		}

#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			throw new InvalidOperationException();
		}
#endif

		public override void Replace(IExpression oldExpr, IExpression newExpr){}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			throw new InvalidOperationException();
		}
	}
}
