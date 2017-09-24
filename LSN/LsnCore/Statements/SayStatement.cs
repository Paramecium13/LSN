using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	[Serializable]
	public class SayStatement : Statement
	{

		private IExpression _Message;
		public IExpression Message { get { return _Message; } set { _Message = value; } }

		private IExpression _Graphic;
		public IExpression Graphic { get { return _Graphic; } set { _Graphic = value; } }

		private IExpression _Title;
		public IExpression Title { get { return _Title; } set { _Title = value; } }

		public SayStatement(IExpression mssg, IExpression graphic, IExpression title)
		{
			Message = mssg; Graphic = graphic; Title = title;
		}

		public override InterpretValue Interpret(IInterpreter i)
		{
			i.Say(((StringValue)Message.Eval(i).Value).Value, Graphic?.Eval(i) ?? LsnValue.Nil, Title?.Eval(i).Value?.ToString());
			return InterpretValue.Base;
		}

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (_Message.Equals( oldExpr)) _Message = newExpr;
			if (_Graphic.Equals( oldExpr)) _Graphic = newExpr;
			if (_Title.Equals (oldExpr)) _Title = newExpr;

		}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)StatementCode.Say);
			Message.Serialize(writer, resourceSerializer);
			Graphic.Serialize(writer, resourceSerializer);
			Title.Serialize(writer, resourceSerializer);
		}
	}
}
