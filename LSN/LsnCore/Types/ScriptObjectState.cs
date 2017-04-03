using LsnCore.Expressions;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Types
{
	public sealed class ScriptObjectState
	{
		public readonly string Name;
		public readonly int Id;


		public readonly IReadOnlyDictionary<string, ScriptObjectMethod> ScriptObjectMethods;




		public bool HasMethod(string name) => ScriptObjectMethods.ContainsKey(name);


		public ScriptObjectMethod GetMethod(string name) => ScriptObjectMethods[name];
	}
}
