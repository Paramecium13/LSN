using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSN_Core.Expressions;
using LSN_Core.Compile;
using LSN_Core;

namespace LSN_Core.Statements
{
	[Serializable]
	public class AssignmentStatement : Statement
	{
		public IExpression Value;
		public string VariableName;
		//public LSN_Type Type;
		//public bool Mutable;

		public AssignmentStatement(string name, IExpression value)
		{
			VariableName = name;
			Value = value;
		}

		/*public AssignmentStatement(List<IToken> tokens,bool mutable=false)
		{
			int headTokenCount = 3; // "let" "name" "="
			Mutable = mutable;
			if (tokens[1].Value == "mut")
			{
				Mutable = true;
				headTokenCount++;
			}
			VariableName = tokens[headTokenCount - 2].Value;
			//if(Program.Variables.ContainsKey(VariableName))
			//{
			//	throw new ApplicationException("The variable {VariableName} has already been declared.");
			//}
			Value = Expression.Create(tokens.Skip(headTokenCount).ToList());
			Type = Value.Type;
			//Program.Variables.Add(VariableName, new Variable(VariableName,Type,Mutable));
		}*/

		public override bool Interpret(IInterpreter i)
		{
			i.AddVariable(VariableName,Value.Eval(i));
			return true;
		}
	}
}
