using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;
using System.Collections;

namespace LsnCore.Statements
{
	public class SayStatement : Statement, IEnumerable<IExpression>
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

#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			var msg = Message.Eval(i);
			var strVal = msg.Value as StringValue;
			var text = strVal.Value;
			var g = Graphic?.Eval(i) ?? LsnValue.Nil;
			string t = null;
			if (Title != null && !(Title is LsnValue v && v.IsNull))
				t = (Title.Eval(i).Value as StringValue).Value;
			i.Say(text, g, t);
			return InterpretValue.Base;
		}
#endif

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			if (_Message.Equals( oldExpr)) _Message = newExpr;
			if (_Graphic.Equals( oldExpr)) _Graphic = newExpr;
			if (_Title.Equals (oldExpr)) _Title = newExpr;
		}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write(StatementCode.Say);
			Message.Serialize(writer, resourceSerializer);
			Graphic.Serialize(writer, resourceSerializer);
			Title.Serialize(writer, resourceSerializer);
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			yield return _Message;
			foreach (var expr in _Message.SelectMany(e => e))
				yield return expr;
			if (_Graphic != null && !Graphic.Equals(LsnValue.Nil))
			{
				yield return _Graphic;
				foreach (var expr in _Graphic.SelectMany(e => e))
					yield return expr;
			}
			if(_Title != null && !_Title.Equals(LsnValue.Nil))
			{
				yield return _Title;
				foreach (var expr in _Title.SelectMany(e => e))
					yield return expr;
			}
		}
	}
}
