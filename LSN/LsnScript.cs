using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	[Serializable]
	public class LsnScript : LsnScriptBase
	{
		public List<Component> Components { get; protected set; }


		public LsnScript(List<Component> components)
		{
			Components = components;
		}
	}
}
