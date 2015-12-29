using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSNr
{
	/// <summary>
	/// Used internally to keep track of scopes.
	/// </summary>
	public class Scope
	{
		private Scope Parent;
		private Dictionary<string, Variable> Variables;

		public Scope() { }


		public Scope(Scope parent)
		{
			Parent = parent;
		}


		public void AddVariable(Variable v)
		{
			Variables.Add(v.Name, v);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool HasVariable(string name) => Variables.ContainsKey(name);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool VariableExists(string name) => HasVariable(name) || Parent.VariableExists(name);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public Variable GetVariable(string name) => HasVariable(name) ? Variables[name] : Parent.GetVariable(name);

		/// <summary>
		/// Returns this scope's parent.
		/// </summary>
		/// <returns> The parent of this scope.</returns>
		public Scope Pop() => Parent;

	}
}
