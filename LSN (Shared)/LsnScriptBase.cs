using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Runtime.Types;

namespace LsnCore
{
	public class LsnScriptBase
	{
		/// <summary>
		/// The functions that are defined in this resources (if this is a resource)
		/// </summary>
		public IReadOnlyDictionary<string, Function> Functions { get; set; } = new Dictionary<string, Function>();

		public IReadOnlyDictionary<string, RecordType> RecordTypes { get; set; } = new Dictionary<string, RecordType>();

		public IReadOnlyList<HandleType> HandleTypes { get; set; } = new List<HandleType>();

		public IReadOnlyDictionary<string, StructType> StructTypes { get; set; } = new Dictionary<string, StructType>();

		/// <summary>
		/// The resources this script uses.
		/// </summary>
		public IReadOnlyList<string> Usings { get; set; } = new List<string>();

		public IReadOnlyDictionary<string, ScriptClass> ScriptClassTypes { get; set; }

		public IReadOnlyDictionary<string, HostInterfaceType> HostInterfaces { get; set; }

		public IReadOnlyDictionary<string, GameValue> GameValues { get; set; }
	}
}
