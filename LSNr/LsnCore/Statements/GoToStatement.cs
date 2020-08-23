using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace LsnCore.Statements
{
	/// <summary>
	/// Change the location of an actor/the party.
	/// </summary>
	public sealed class GoToStatement : Statement
	{
		internal enum Form:byte { Map, MapLabel, MapXY, /*Label,*/ XY, Pos, MapPos}

		private IExpression Map;

		private IExpression X;

		private IExpression Y;

		//private readonly IExpression LocLabel;

		private IExpression Position;

		private readonly IExpression Actor;

		private readonly Form MyForm;

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="ex0"></param>
		/// <param name="ex1"></param>
		/// <param name="ex2"></param>
		/// <param name="actor"></param>
		// <param name="hasKeywordLabel"></param>
		public GoToStatement(IExpression ex0, IExpression ex1, IExpression ex2, IExpression actor = null/*, bool hasKeywordLabel = false*/)
		{
			if (ex0 == null) throw new ArgumentNullException();
			Actor = actor; // Make sure its the right type.
			if (ex0.Type.Type == LsnType.string_)
			{
				/*if (hasKeywordLabel)
				{
					LocLabel = ex0;
					MyForm = Form.Label;
					return;
				}*/
				// It's a map name
				Map = ex0;
				if (ex1 == null) // It's of the form '(actor) goto <map>;'
				{
					MyForm = Form.Map;
					return;
				}

				/*if (ex1.Type.Type == LsnType.string_) // It's of the form '(actor) goto <map> ` <locLable>;'
				{
					LocLabel = ex1;
					MyForm = Form.MapLabel;
					return;
				}*/

				if (ex1.Type.Type == LsnType.int_) // It's of the form '(actor) goto <map> <x> <y>;'
				{
					if (ex2 == null || (ex2.Type.Type != LsnType.int_))
						throw new ArgumentNullException();
					X = ex1;
					Y = ex2;
					MyForm = Form.MapXY;
					return;
				}

				if (ex1.Type.Name == "Point") // It's of the form '(actor) goto <map> ` <point>;'
					throw new NotImplementedException(); // MyForm = Form.MapPos;

				throw new ArgumentException();
			}

			if (ex0.Type.Type == LsnType.int_)
			{
				if (ex1 == null) throw new ArgumentNullException();
				if (ex1.Type.Type != LsnType.int_) throw new ArgumentException();
				X = ex0;
				Y = ex1;
				MyForm = Form.XY;
				return;
			}

			if (ex0.Type.Name == "Point") throw new NotImplementedException();

			throw new ArgumentException();
		}

#if CORE
		public override InterpretValue Interpret(IInterpreter i)
		{
			switch (MyForm)
			{
				case Form.Map:
					break;
				case Form.MapLabel:
					break;
				case Form.MapXY:
					break;
				case Form.MapPos:
					break;
				/*case Form.Label:
					break;*/
				case Form.XY:
					break;
				case Form.Pos:
					break;
			}
			throw new NotImplementedException();
		}
#endif

		public override void Replace(IExpression oldExpr, IExpression newExpr)
		{
			//if (LocLabel.Equals( oldExpr)) LocLabel = newExpr;
			if (Map.Equals (oldExpr)) Map = newExpr;
			if (Position.Equals(oldExpr)) Position = newExpr;
			if (X.Equals(oldExpr)) X = newExpr;
			if (Y.Equals(oldExpr)) Y = newExpr;
		}

		internal override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			throw new NotImplementedException();
		}

		public override IEnumerator<IExpression> GetEnumerator()
		{
			if (Map != null && !Map.Equals(LsnValue.Nil))
			{
				yield return Map;
				foreach (var expr in Map.SelectMany(e => e))
					yield return expr;
			}

			if (X != null && !X.Equals(LsnValue.Nil))
			{
				yield return X;
				foreach (var expr in X.SelectMany(e => e))
					yield return expr;
			}

			if (Y != null && !Y.Equals(LsnValue.Nil))
			{
				yield return Y;
				foreach (var expr in Y.SelectMany(e => e))
					yield return expr;
			}

			if (Position != null && !Position.Equals(LsnValue.Nil))
			{
				yield return Position;
				foreach (var expr in Position.SelectMany(e => e))
					yield return expr;
			}

			if (Actor == null || Actor.Equals(LsnValue.Nil)) yield break;
			yield return Actor;
			foreach (var expr in Actor.SelectMany(e => e))
				yield return Actor;
		}
	}
}
