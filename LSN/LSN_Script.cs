using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core
{
	[Serializable]
	public class LSN_Script : LSN_ScriptBase
	{
		public List<Component> Components { get; protected set; }


		public LSN_Script(List<Component> components)
		{
			Components = components;
		}
	}
}
