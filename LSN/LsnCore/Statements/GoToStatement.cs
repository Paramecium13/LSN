﻿using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsnCore.Statements
{
	/// <summary>
	/// Change the location of an actor/the party.
	/// </summary>
	[Serializable]
	public class GoToStatement : Statement
	{

		private enum Form { Map, MapLabel, MapXY, MapPos, Label, XY, Pos}

		private IExpression Map;

		private IExpression X;

		private IExpression Y;

		private IExpression LocLabel;

		private IExpression Position;

		private IExpression Actor;

		private Form MyForm;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ex0"></param>
		/// <param name="ex1"></param>
		/// <param name="ex2"></param>
		/// <param name="actor"></param>
		/// <param name="hasKeywordLabel"></param>
		public GoToStatement(IExpression ex0, IExpression ex1, IExpression ex2, IExpression actor = null, bool hasKeywordLabel = false)
		{
			if (ex0 == null) throw new ArgumentNullException();
			Actor = actor; // Make sure its the right type.
			if (ex0.Type == LsnType.string_)
			{
				if (hasKeywordLabel)
				{
					LocLabel = ex0;
					MyForm = Form.Label;
					return;
				}
				// It's a map name
				Map = ex0;
				if (ex1 == null) // It's of the form '(actor) goto <map>;'
				{
					MyForm = Form.Map;
					return;
				}

				if (ex1.Type == LsnType.string_) // It's of the form '(actor) goto <map> ` <locLable>;'
				{
					LocLabel = ex1;
					MyForm = Form.MapLabel;
					return;
				}

				if (ex1.Type == LsnType.int_) // It's of the form '(actor) goto <map> <x> <y>;'
				{
					if (ex2 == null) throw new ArgumentNullException();
					if (ex2.Type != LsnType.int_) throw new ArgumentException();
					X = ex1;
					Y = ex2;
					MyForm = Form.MapXY;
					return;
				}

				if (ex1.Type.Name == "Point") // It's of the form '(actor) goto <map> ` <point>;'
					throw new NotImplementedException(); // MyForm = Form.MapPos;

				throw new ArgumentException();
			}

			if (ex0.Type == LsnType.int_)
			{
				if (ex1 == null) throw new ArgumentNullException();
				if (ex1.Type != LsnType.int_) throw new ArgumentException();
				X = ex0;
				Y = ex1;
				MyForm = Form.XY;
			}

			if (ex0.Type.Name == "Point") throw new NotImplementedException();

			throw new ArgumentException();
		}


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
				case Form.Label:
					break;
				case Form.XY:
					break;
				case Form.Pos:
					break;
			}
			throw new NotImplementedException();
		}
	}
}