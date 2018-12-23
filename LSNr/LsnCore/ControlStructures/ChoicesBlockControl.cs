using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;

namespace LsnCore.ControlStructures
{
	public class ChoicesBlockControl : ControlStructure
	{
		public readonly List<Choice> Choices = new List<Choice>();

		public ChoicesBlockControl(List<Component> components)
		{
			foreach(var c in components)
			{
				if (c.GetType() != typeof(Choice)) throw new ArgumentException("Choice blocks can only contain choices.");
				Choices.Add((Choice)c);
			}
		}

#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			throw new InvalidOperationException();
			/*var choices = Choices.Where(c => c.Check(i)).ToList();
			int index = i.Choice(choices.Select(c => c.Title.Eval(i).ToString()).ToList());
			return choices[index].Interpret(i);*/
		}
#endif

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{}
	}
}
