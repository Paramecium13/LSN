using LsnCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LsnCore.Statements;

namespace LSNr
{
	/// <summary>
	/// Used internally to keep track of scopes.
	/// </summary>
	public class Scope : IScope
	{
		private readonly Scope Parent;
		private Dictionary<string, Variable> Variables = new Dictionary<string, Variable>();

		public Scope() { }


		public Scope(Scope parent)
		{
			Parent = parent;
		}

		public Variable CreateVariable(string name, bool mutable, IExpression init, AssignmentStatement assign)
		{
			var v = new Variable(name, mutable, init, assign);
			Variables.Add(name,v);
			return v;
		}

		public Variable CreateVariable(Parameter param)
		{
			var v = new Variable(param);
			Variables.Add(v.Name, v);
			return v;
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
		public bool VariableExists(string name) => HasVariable(name) || (Parent?.VariableExists(name) ?? false);

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
		public IScope Pop(List<Component> components)
		{
			// Optimize contained variables...
			foreach(var variable in Variables)
			{
				if (!variable.Value.Used && variable.Value.Assignment != null) components.Remove(variable.Value.Assignment);
			}
			return Parent;
		}

		public IScope CreateChild() => new Scope(this);
	}
}
