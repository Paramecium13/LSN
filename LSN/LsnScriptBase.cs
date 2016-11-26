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
		private Dictionary<string, Function> _Functions = new Dictionary<string, Function>();
		/// <summary>
		/// The functions that are defined in this resources (if this is a resource) 
		/// </summary>
		public Dictionary<string, Function> Functions { get { return _Functions; } set { _Functions = value; } }

		private Dictionary<string, LsnStructType> _StructTypes = new Dictionary<string, LsnStructType>();
		public Dictionary<string, LsnStructType> StructTypes { get { return _StructTypes; } set { _StructTypes = value; } }

		private Dictionary<string, RecordType> _RecordTypes = new Dictionary<string, RecordType>();
		public Dictionary<string, RecordType> RecordTypes { get { return _RecordTypes; } set { _RecordTypes = value; } }

		public List<LsnType> Types;

		private List<string> _Usings = new List<string>();
		/// <summary>
		/// The resources this script uses.
		/// </summary>
		public List<string> Usings { get { return _Usings; } set { _Usings = value; } }


		private List<string> _Includes = new List<string>();


		public List<string> Includes { get { return _Includes; } set { _Includes = value; } }

		//protected string Source { get; set; }
		//protected List<string> PreTokens { get; set; }
		//protected List<IToken> Tokens { get; set; }
	}
}
