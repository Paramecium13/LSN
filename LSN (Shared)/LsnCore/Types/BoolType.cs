using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	public class BoolType : LsnBoundedType<bool>
	{
		internal BoolType(string name):base(name, ()=> LsnBoolValue.GetBoolValue(false)) { }

		public override LsnValue CreateDefaultValue() => LsnBoolValue.GetBoolValue(false);
	}

}
