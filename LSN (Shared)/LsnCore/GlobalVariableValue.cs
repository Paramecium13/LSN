using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	[Serializable]
	public class GlobalVariableValue
	{
		public readonly string Name;

		protected LsnValue _Value;

		public virtual LsnValue Value
		{
			get { return _Value; }
			set { _Value = value; }
		}

		public GlobalVariableValue(string name,LsnValue value)
		{
			Name = name;_Value = value;
		}

	}
}
