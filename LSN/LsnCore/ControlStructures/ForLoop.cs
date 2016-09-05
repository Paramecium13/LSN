using LsnCore.Expressions;
using LsnCore.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.ControlStructures
{
	/// <summary>
	/// A(n) LSN for loop...
	/// </summary>
	[Serializable]
	public class ForLoop : ControlStructure
	{

		public readonly string VarName;

		public IExpression VarValue;

		public IExpression Condition;
		
		public readonly List<Component> Body;

		public Statement Post;


		public ForLoop(string varName,IExpression val, IExpression con, List<Component> body, Statement post)
		{
			VarName = varName; VarValue = val; Condition = con; Body = body; Post = post;
		}


		public override InterpretValue Interpret(IInterpreter i)
		{
			i.EnterScope();
			i.AddVariable(VarName, VarValue.Eval(i));
			while(Condition.Eval(i).BoolValue)
			{
				var val = Interpret(Body, i);
				switch (val)
				{
					case InterpretValue.Base:
						break;
					case InterpretValue.Next:
						break;
					case InterpretValue.Break:
						i.ExitScope();
						return InterpretValue.Base;
					case InterpretValue.Return:
						i.ExitScope();
						return InterpretValue.Return;
					default:
						break;
				}
				Post.Interpret(i);
			}
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (VarValue.Equals(oldExpr)) VarValue = newExpr;
			if (Condition.Equals(oldExpr)) Condition = newExpr;
		}
	}
}
