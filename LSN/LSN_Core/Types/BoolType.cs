using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core
{
	public class BoolType : LSN_BoundedType<bool>
	{
        public override bool BoolVal { get { return true; } }
		internal BoolType(string name, params string[] args):base(name,sizeof(bool),args) { }

		public override ILSN_Value CreateDefaultValue() => new BoolValue(false);
	}

}
