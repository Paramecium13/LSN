using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.ControlStructures
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class MatchStructure : ControlStructure
	{
		private string VariableName;

		/// <summary>
		/// For when variables are assigned to indexes.
		/// </summary>
		private int VariableIndex;

		private List<CaseStructure> Cases = new List<CaseStructure>();


		public MatchStructure(string variable, List<Component> components)
		{
			foreach(var component in components)
			{
				var c = component as CaseStructure;
				if (c != null) Cases.Add(c);
				else throw new ArgumentException("All members a match structure must be case structures.");
			}
		}


		public override InterpretValue Interpret(IInterpreter i)
		{
			ILsnValue value = i.GetValue(VariableName);
			for(int j = 0; j < Cases.Count; j++)
			{
				if (Cases[j].Value.Eval(i) == value) return Cases[j].Interpret(i);
			}
			return InterpretValue.Base;
		}
	}
}
