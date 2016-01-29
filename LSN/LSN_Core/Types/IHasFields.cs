using LSN_Core.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core.Types
{
	/// <summary>
	/// A(n) LSN type that has fields.
	/// </summary>
	public interface IHasFieldsType
	{
		IReadOnlyDictionary<string, LSN_Type> Fields { get; }
	}

	public interface IHasFieldsValue: IExpression
	{
		ILSN_Value GetValue(string name);
    }
	
}
