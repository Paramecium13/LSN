using LsnCore.Expressions;

namespace LsnCore
{
	public interface ILsnValue : IExpression
	{
		bool BoolValue { get; }

		ILsnValue Clone();
		//int GetSize();
	}
}