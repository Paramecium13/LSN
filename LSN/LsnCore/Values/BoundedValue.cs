﻿using System;
using System.Collections.Generic;
using System.Text;
using LsnCore.Expressions;
using LsnCore.Types;

namespace LsnCore
{
	/// <summary>
	/// LSN value that directly corresponds to a .NET type.
	/// </summary>
	/// <typeparam name="T">The .NET type this value has.</typeparam>
	public interface IBoundValue<T> : ILsnValue
	{
		T Value { get; }
	}

	/// <summary>
	/// LSN value that contains a 32 bit integer.
	/// </summary>
	[Serializable]
	public struct IntValue : IBoundValue<int>
	{
		public static TypeId id = LsnType.int_.Id;
		
		public TypeId Type { get { return id; } }
		public int Value { get; private set; }
		public bool BoolValue { get { return true; } }

		public bool IsPure => true;

		public IntValue(int val)
		{
			Value = val;
		}

		public ILsnValue Clone() => new IntValue(Value);

		public ILsnValue Eval(IInterpreter i) => this;
		public IExpression Fold() => this;
		public bool IsReifyTimeConst() => true;

		public void Replace(IExpression oldExpr, IExpression newExpr){}

		public bool Equals(IExpression other)
		{
			var e = other as IBoundValue<int>;
			if (e == null) return false;
			return e.Value == Value;
		}

		public static explicit operator int(IntValue v) => v.Value;
		public static explicit operator double(IntValue v) => v.Value;

		public static explicit operator DoubleValue(IntValue v) => new DoubleValue(v.Value);
	}

	/// <summary>
	/// LSN value that contains a string, is effectively passed by reference.
	/// </summary>
	[Serializable]
	public struct StringValue : IBoundValue<string>
	{
		public static TypeId id = LsnType.string_.Id;

		public TypeId Type { get { return id; } }

		public string Value { get; private set; }
		public bool BoolValue { get { return true; } }

		public bool IsPure => true;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="val"></param>
		public StringValue(string val)
		{
			Value = val;
		}

		/// <summary>
		/// Strings are immutable; returns this.
		/// </summary>
		/// <returns></returns>
		public ILsnValue Clone() => this;

		/// <summary>
		/// Strings are immutable; returns this.
		/// </summary>
		/// <returns></returns>
		public ILsnValue DeepClone() => this;
		/*{
			char[] c = new char[Value.Length];
			Value.CopyTo(0, c, 0, 1);
			return new StringValue(new string(c));
		}*/

		public ILsnValue Eval(IInterpreter i) => this;
		public IExpression Fold() => this;
		public bool IsReifyTimeConst() => true;
		public string TranslateUniversal() => Value.ToString();

		public static explicit operator string(StringValue v) => v.Value;
		public static explicit operator StringValue(string s) => new StringValue(s);

		public void Replace(IExpression oldExpr, IExpression newExpr) { }

		public bool Equals(IExpression other)
		{
			var e = other as IBoundValue<string>;
			if (e == null) return false;
			return e.Value == Value;
		}
	}

	/// <summary>
	/// LSN value that contains a Double.
	/// </summary>
	[Serializable]
	public struct DoubleValue : IBoundValue<double>
	{
		public static TypeId id = LsnType.double_.Id;

		public TypeId Type { get { return id; } }

		public double Value { get; private set; }
		public bool BoolValue { get { return true; } }

		public bool IsPure => true;

		public DoubleValue(double val)
		{
			Value = val;
		}
		public ILsnValue Clone() => new DoubleValue(Value);

		public ILsnValue Eval(IInterpreter i) => this;
		public IExpression Fold() => this;
		public bool IsReifyTimeConst() => true;


		public static explicit operator double(DoubleValue v) => v.Value;

		public static explicit operator DoubleValue(double v) => new DoubleValue(v);

		public void Replace(IExpression oldExpr, IExpression newExpr) { }

		public bool Equals(IExpression other)
		{
			var e = other as IBoundValue<double>;
			if (e == null) return false;
			return e.Value == Value;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class LsnBoolValue : IBoundValue<bool>
	{
		private static LsnBoolValue True = new LsnBoolValue(true);
		private static LsnBoolValue False = new LsnBoolValue(false);
		public static LsnBoolValue GetBoolValue(bool val)
			=> val? True : False;

		public bool IsPure => true;

		public static TypeId id = LsnType.Bool_.Id;


		public TypeId Type { get { return id; } }

		public bool Value { get; private set; }
		public bool BoolValue { get { return Value; } }

		private LsnBoolValue(bool val)
		{
			Value = val;
		}

		public ILsnValue Clone() => this;
		public ILsnValue Eval(IInterpreter i) => this;
		public IExpression Fold() => this;
		public bool IsReifyTimeConst() => true;


		public void Replace(IExpression oldExpr, IExpression newExpr) { }

		public bool Equals(IExpression other)
		{
			var e = other as IBoundValue<bool>;
			if (e == null) return false;
			return e.Value == Value;
		}
	}
}
