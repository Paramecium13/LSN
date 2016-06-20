using System;
using System.Collections.Generic;
using System.Text;

namespace LsnCore
{
	[Serializable]
	public class Scope : IScope<Scope>
	{
		protected Scope Parent;

		protected Dictionary<string, ILsnValue> Variables = new Dictionary<string, ILsnValue>();

		public Scope() { }

		public  Scope(Scope parent) { Parent = parent; }

		protected bool Contains(string name) => Variables.ContainsKey(name);

		/// <summary>
		/// Creates a new scope with this as its parent.
		/// </summary>
		/// <returns>A new scope.</returns>
		public Scope Push() => new Scope(this);
		public Scope Pop() => Parent;

		public void AddVariable(string name, ILsnValue val)
		{
			if (Contains(name))
				Variables.Add(name, val);
			else Parent.AddVariable(name, val);
		}

		public virtual void ReAssignVariable(string name, ILsnValue val)
		{
			if (Contains(name))
				Variables[name] = val;
			else Parent.ReAssignVariable(name, val);
		}

		public ILsnValue GetValue(string name) => Variables[name];
	}
}
