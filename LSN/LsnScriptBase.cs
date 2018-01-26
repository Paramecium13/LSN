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
		private Dictionary<string, Function> _Functions = new Dictionary<string, Function>(); // TODO: Encapsulate
		/// <summary>
		/// The functions that are defined in this resources (if this is a resource) 
		/// </summary>
		public Dictionary<string, Function> Functions { get { return _Functions; } set { _Functions = value; } }

		private Dictionary<string, RecordType> _RecordTypes = new Dictionary<string, RecordType>(); // TODO: Encapsulate
		public Dictionary<string, RecordType> RecordTypes { get { return _RecordTypes; } set { _RecordTypes = value; } }

		private Dictionary<string, StructType> _StructTypes = new Dictionary<string, StructType>(); // TODO: Encapsulate
		public Dictionary<string, StructType> StructTypes { get { return _StructTypes; } set { _StructTypes = value; } }

		//public List<LsnType> Types;

		private List<string> _Usings = new List<string>(); // TODO: Encapsulate
		/// <summary>
		/// The resources this script uses.
		/// </summary>
		public List<string> Usings { get { return _Usings; } set { _Usings = value; } }


		private List<string> _Includes = new List<string>(); // TODO: Encapsulate

		public List<string> Includes { get { return _Includes; } set { _Includes = value; } }

		private IReadOnlyDictionary<string, ScriptClass> _ScriptObjectTypes;
		public IReadOnlyDictionary<string, ScriptClass> ScriptObjectTypes
		{
			get { return _ScriptObjectTypes; }
			set { _ScriptObjectTypes = value; }
		}

		private IReadOnlyDictionary<string,HostInterfaceType> _HostInterfaces;

		public IReadOnlyDictionary<string, HostInterfaceType> HostInterfaces
		{
			get { return _HostInterfaces; }
			set { _HostInterfaces = value; }
		}


		//protected string Source { get; set; }
		//protected List<string> PreTokens { get; set; }
		//protected List<IToken> Tokens { get; set; }
	}
}
