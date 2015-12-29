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
	public class IfElseControl : ControlStructure
	{
		public Expression Condition;
		public List<Component> Body;
		public List<ElsifControl> Elsifs = new List<ElsifControl>();
		public List<Component> ElseBlock;

		public override bool Interpret(IInterpreter i)
		{
			throw new NotImplementedException();
		}
	}
	public class IfControl : ControlStructure
	{
		public Expression Condition;
		public List<Component> Body;

		public IfControl(Expression c, List<Component> body)
		{
			Condition = c;
			Body = body;
		}

		public override bool Interpret(IInterpreter i)
		{
			throw new NotImplementedException();
		}
	}
	public class ElsifControl : ControlStructure
	{
		internal Expression Condition;
		internal List<Component> Body;
		/*public string Translate()
		{
			string translation = "elsif ";
			translation += Condition.Translate() + "\n";
			translation += TranslateBody(Body);
			return translation;
		}*/
		public ElsifControl(Expression c, List<Component> body)
		{
			Condition = c;
			Body = body;
		}

		public override bool Interpret(IInterpreter i)
		{
			throw new NotImplementedException();
		}
	}
	public class ElseControl : ControlStructure
	{
		public Expression Condition;

		public List<Component> Body;

		public ElseControl(Expression c, List<Component> body)
		{
			Condition = c;
			Body = body;
		}

		public override bool Interpret(IInterpreter i)
		{
			throw new NotImplementedException();
		}
	}
}
