using System;

namespace LSN_Core.Expressions
{
	public interface IExpression
	{
		LSN_Type Type { get; }

		ILSN_Value Eval(IInterpreter i);
		IExpression Fold();
		bool IsReifyTimeConst();
	}
}