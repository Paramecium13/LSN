using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Expressions;
using LsnCore.Statements;

namespace LsnCore.ControlStructures
{
	[Serializable]
	public class IfElseControl : ControlStructure
	{
		public IExpression Condition { get; set; }
		public readonly IList<Component> Body;
		public readonly IList<ElsifControl> Elsifs = new List<ElsifControl>();
		public List<Component> ElseBlock; // Can be null

		public IfElseControl(IExpression condition, IList<Component> body)
		{
			Condition = condition; Body = body;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			if (Condition.Eval(i).BoolValue)
			{
				var length = Body.Count;
				for(int j = 0; j < length; j++)
				{
					var val = Body[j].Interpret(i);
					if (val != InterpretValue.Base)
						return val;
					//Which is better?
					/*switch (Body[j].Interpret(i))
					{
						case InterpretValue.Base:
							break;
						case InterpretValue.Next:
							return InterpretValue.Next;
						case InterpretValue.Break:
							return InterpretValue.Break;
						case InterpretValue.Return:
							return InterpretValue.Return;
						default:
							break;
					}*/
				}
				return InterpretValue.Base;
			}
			else
			{
				var elsifcount = Elsifs.Count;
				if(elsifcount != 0)
				{
					for(int j = 0; j < elsifcount; j++)
					{
						var el = Elsifs[j];
						if(el.Condition.Eval(i).BoolValue)
						{
							var b = el.Body;
							var length = b.Count;
							for (int k = 0; k < length; j++)
							{
								var val = b[j].Interpret(i);
								if (val != InterpretValue.Base)
									return val;
								//Which is better?
								/*switch (b[j].Interpret(i))
								{
									case InterpretValue.Base:
										break;
									case InterpretValue.Next:
										return InterpretValue.Next;
									case InterpretValue.Break:
										return InterpretValue.Break;
									case InterpretValue.Return:
										return InterpretValue.Return;
									default:
										break;
								}*/
							}
							return InterpretValue.Base;
						}
					}
				}
				if(ElseBlock != null)
				{
					var length = ElseBlock.Count;
					for (int j = 0; j < length; j++)
					{
						var val = ElseBlock[j].Interpret(i);
						if (val != InterpretValue.Base)
							return val;
						//Which is better?
						/*switch (ElseBlock[j].Interpret(i))
						{
							case InterpretValue.Base:
								break;
							case InterpretValue.Next:
								return InterpretValue.Next;
							case InterpretValue.Break:
								return InterpretValue.Break;
							case InterpretValue.Return:
								return InterpretValue.Return;
							default:
								break;
						}*/
					}
					return InterpretValue.Base;
				}
				return InterpretValue.Base;
			}
			throw new NotImplementedException();
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Condition.Equals(oldExpr)) Condition = newExpr;
		}
	}

	public class IfControl : ControlStructure
	{
		public IExpression Condition { get; private set; }
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

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Condition.Equals(oldExpr)) Condition = newExpr;
		}
	}
	[Serializable]
	public class ElsifControl : ControlStructure
	{
		public IExpression Condition;
		public List<Component> Body;

		public ElsifControl(IExpression c, List<Component> body)
		{
			Condition = c;
			Body = body;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			throw new NotImplementedException();
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (Condition.Equals(oldExpr)) Condition = newExpr;
		}
	}

	public class ElseControl : ControlStructure
	{

		public List<Component> Body;

		public ElseControl(List<Component> body)
		{
			Body = body;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			throw new NotImplementedException();
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{}
	}
}
