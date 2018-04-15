using LsnCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore
{
	[Serializable]
	public class LsnScriptBase
	{
		/// <summary>
		/// The functions that are defined in this resources (if this is a resource)
		/// </summary>
		public Dictionary<string, Function> Functions { get; set; } = new Dictionary<string, Function>();

		public Dictionary<string, RecordType> RecordTypes { get; set; } = new Dictionary<string, RecordType>();

		public Dictionary<string, StructType> StructTypes { get; set; } = new Dictionary<string, StructType>();

		/// <summary>
		/// The resources this script uses.
		/// </summary>
		public List<string> Usings { get; set; } = new List<string>();

		public List<string> Includes { get; set; } = new List<string>();
		public IReadOnlyDictionary<string, ScriptClass> ScriptClassTypes { get; set; }

		public IReadOnlyDictionary<string, HostInterfaceType> HostInterfaces { get; set; }

		public IReadOnlyDictionary<string,GameValue> GameValues { get; set; }

		//protected string Source { get; set; }
		//protected List<string> PreTokens { get; set; }
		//protected List<IToken> Tokens { get; set; }
	}
}
