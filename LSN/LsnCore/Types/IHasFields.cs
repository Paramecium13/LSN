using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	/// <summary>
	/// A(n) LSN type that has fields.
	/// </summary>
	public interface IHasFieldsType
	{
		IReadOnlyCollection<Field> FieldsB { get; }
		int GetIndex(string name);
	}

	public interface IHasFieldsValue: IExpression
	{
		LsnValue GetFieldValue(int index);
	}

	public interface IHasMutableFieldsValue : IHasFieldsValue
	{
		void SetFieldValue(int index, LsnValue value);
	}

}
