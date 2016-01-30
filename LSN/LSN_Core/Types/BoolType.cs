using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core
{
	[Serializable]
	public class BoolType : LSN_BoundedType<bool>
	{
		internal BoolType(string name, params string[] args):base(name,null,args) { }

		public override ILSN_Value CreateDefaultValue() => LSN_BoolValue.GetBoolValue(false);
	}

}
