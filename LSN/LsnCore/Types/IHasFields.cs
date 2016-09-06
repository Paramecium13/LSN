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
		IReadOnlyDictionary<string, LsnType> Fields { get; }
		int GetIndex(string name);
	}

	public interface IHasFieldsValue: IExpression
	{
		ILsnValue GetValue(int index);
	}
	
}
