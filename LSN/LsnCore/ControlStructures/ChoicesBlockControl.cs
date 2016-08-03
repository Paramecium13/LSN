using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;

namespace LsnCore.ControlStructures
{
	[Serializable]
	public class ChoicesBlockControl : ControlStructure
	{
		private List<Choice> Choices = new List<Choice>();

		public ChoicesBlockControl(List<Component> components)
		{
			foreach(var c in components)
			{
				if (c.GetType() != typeof(Choice)) throw new ArgumentException("Choice blocks can only contain choices.");
				Choices.Add((Choice)c);
			}
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			var choices = Choices.Where(c => c.Check(i)).ToList();
			int index = i.Choice(choices.Select(c => c.Title.Eval(i).ToString()).ToList());
			return choices[index].Interpret(i);
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{}
	}
}
