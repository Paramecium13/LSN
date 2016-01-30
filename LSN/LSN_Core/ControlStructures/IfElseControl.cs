using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSN_Core.Expressions;
using LSN_Core.Statements;
using LSN_Core.Compile;

namespace LSN_Core.ControlStructures
{
	[Serializable]
	public class IfElseControl : ControlStructure
	{
		public IExpression Condition;
		public List<Component> Body;
		public List<ElsifControl> Elsifs = new List<ElsifControl>();
		public List<Component> ElseBlock;

		public override InterpretValue Interpret(IInterpreter i)
		{
			throw new NotImplementedException();
		}
	}
	public class IfControl : ControlStructure
	{
		public IExpression Condition;
		public List<Component> Body;

		public IfControl(IExpression c, List<Component> body)
		{
			Condition = c;
			Body = body;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			throw new NotImplementedException();
		}
	}
	public class ElsifControl : ControlStructure
	{
		internal IExpression Condition;
		internal List<Component> Body;
		/*public string Translate()
		{
			string translation = "elsif ";
			translation += Condition.Translate() + "\n";
			translation += TranslateBody(Body);
			return translation;
		}*/
		public ElsifControl(IExpression c, List<Component> body)
		{
			Condition = c;
			Body = body;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			throw new NotImplementedException();
		}
	}
	public class ElseControl : ControlStructure
	{
		public IExpression Condition;

		public List<Component> Body;

		public ElseControl(IExpression c, List<Component> body)
		{
			Condition = c;
			Body = body;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			throw new NotImplementedException();
		}
	}
}
