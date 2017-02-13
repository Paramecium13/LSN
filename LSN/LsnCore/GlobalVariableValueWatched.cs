using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	[Serializable]
	public sealed class GlobalVariableValueWatched : GlobalVariableValue
	{
		public override LsnValue Value
		{
			get { return _Value; }
			set
			{
				throw new NotImplementedException();
			}
		}



		public GlobalVariableValueWatched(string name, LsnValue value) : base(name, value){}


		public void Watch(object arg)
		{
			throw new NotImplementedException();
		}

		public void UnWatch(object arg)
		{
			throw new NotImplementedException();
		}

	}
}
