using LSN_Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSN_Core
{
	[Serializable]
	public class LSN_ScriptBase
	{
		private Dictionary<string, Function> _Functions = new Dictionary<string, Function>();
		/// <summary>
		/// The functions that are defined in this resources (if this is a resource) 
		/// </summary>
		public Dictionary<string, Function> Functions { get { return _Functions; } set { _Functions = value; } }

		private Dictionary<string, LSN_StructType> _StructTypes = new Dictionary<string, LSN_StructType>();
		public Dictionary<string, LSN_StructType> StructTypes { get { return _StructTypes; } set { _StructTypes = value; } }

		private List<string> _Usings = new List<string>();
		/// <summary>
		/// The resources this script uses.
		/// </summary>
		public List<string> Usings { get { return _Usings; } set { _Usings = value; } }

		//protected string Source { get; set; }
		//protected List<string> PreTokens { get; set; }
		//protected List<IToken> Tokens { get; set; }
	}
}
