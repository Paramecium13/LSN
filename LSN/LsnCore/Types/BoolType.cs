using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	[Serializable]
	public class BoolType : LsnBoundedType<bool>
	{
		internal BoolType(string name, params string[] args):base(name,null,args) { }

		public override ILsnValue CreateDefaultValue() => LSN_BoolValue.GetBoolValue(false);
	}

}
