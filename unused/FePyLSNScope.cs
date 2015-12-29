using IronPython;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace LSN_Core
{
	public class FePyLSNScope : IScope<FePyLSNScope>
	{
		protected FePyLSNScope Parent;
		public ScriptScope PyScope { get; protected set; }
		protected ScriptEngine Engine;

		public FePyLSNScope(ScriptEngine engine)
		{
			Engine = engine;
			PyScope = Engine.CreateScope();
		}

		public FePyLSNScope(FePyLSNScope parent, ScriptEngine engine):this(engine)
		{
			Parent = parent;
		}

		public void AddVariable(string name, ILSN_Value val)
		{
			PyScope.SetVariable(name, val);
		}

		public ILSN_Value GetValue(string name)
		{
			if (PyScope.ContainsVariable(name)) return PyScope.GetVariable<ILSN_Value>(name);
			return Parent.GetValue(name);
		}

		public FePyLSNScope Pop() => Parent;

		public FePyLSNScope Push() => new FePyLSNScope(this, Engine);

		public void ReAssignVariable(string name, ILSN_Value val)
		{
			PyScope.SetVariable(name, val);
		}
	}
}
