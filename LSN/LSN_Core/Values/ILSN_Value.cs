using LSN_Core.Expressions;

namespace LSN_Core
{
	public interface ILSN_Value : IComponentExpression
	{
		bool BoolValue { get; }

		ILSN_Value Clone();
		//int GetSize();
	}
}