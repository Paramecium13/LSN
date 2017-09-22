using LsnCore.Expressions;
using Syroot.BinaryData;

namespace LsnCore
{
	public interface ILsnValue : IExpression
	{
		bool BoolValue { get; }

		ILsnValue Clone();
		//int GetSize();
		void Serialize(BinaryDataWriter writer);
	}
}