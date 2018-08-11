using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;

namespace LsnCore.ControlStructures
{
	/// <summary>
	/// ...
	/// </summary>
	public class MatchStructure : ControlStructure
	{
		/// <summary>
		/// For when variables are assigned to indexes.
		/// </summary>
		private int VariableIndex;

		public readonly List<CaseStructure> Cases = new List<CaseStructure>();


		public MatchStructure(int variable, IList<Component> components)
		{
			VariableIndex = variable;
			foreach (var component in components)
			{
				var c = component as CaseStructure;
				if (c != null) Cases.Add(c);
				else throw new ArgumentException("All members a match structure must be case structures.");
			}
		}


		public override InterpretValue Interpret(IInterpreter i)
		{
			LsnValue value = i.GetVariable(VariableIndex);
			for(int j = 0; j < Cases.Count; j++)
			{
				if (Cases[j].Value.Eval(i).Equals(value)) return Cases[j].Interpret(i);
			}
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr){}

		public override PreStatement[] Flatten(LabelInfo labelInfo)
		{
			throw new NotImplementedException();
		}
	}
}
