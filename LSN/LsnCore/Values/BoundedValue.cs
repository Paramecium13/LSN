using System;
using System.Collections.Generic;
using System.Text;
using LsnCore.Expressions;

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
		public int Value { get; private set; }
		public LsnType Type { get; set; }
		public bool BoolValue { get { return true; } }

		public IntValue(int val, LsnType type)
		{
			Type = type;
			Value = val;
		}

		public ILsnValue Clone() => new IntValue(Value, Type);

		public ILsnValue Eval(IInterpreter i) => this;
		public IExpression Fold() => this;
		public bool IsReifyTimeConst() => true;
		public string TranslateUniversal() => Value.ToString();

		public void Replace(IExpression oldExpr, IExpression newExpr){}

		public bool Equals(IExpression other)
		{
			var e = other as IBoundValue<int>;
			if (e == null) return false;
			return e.Value == Value;
		}

		public static explicit operator int(IntValue v) => v.Value;
		public static explicit operator double(IntValue v) => v.Value;
	}

	/// <summary>
	/// LSN value that contains a string, is effectively passed by reference.
	/// </summary>
	[Serializable]
	public class StringValue : IBoundValue<string>
	{
		public LsnType Type { get; set; }
		public string Value { get; private set; }
		public bool BoolValue { get { return true; } }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="val"></param>
		public StringValue(string val, LsnType type)
		{
			Value = val;
			Type = type;
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
		public double Value { get; private set; }
		public LsnType Type { get; set; }
		public bool BoolValue { get { return true; } }

		public DoubleValue(double val, LsnType type)
		{
			Value = val;
			Type = type;
		}
		public ILsnValue Clone() => new DoubleValue(Value,Type);

		public ILsnValue Eval(IInterpreter i) => this;
		public IExpression Fold() => this;
		public bool IsReifyTimeConst() => true;
		public string TranslateUniversal() => Value.ToString();


		public static explicit operator double(DoubleValue v) => v.Value;

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
	public class LSN_BoolValue : IBoundValue<bool>
	{
		private static LSN_BoolValue True = new LSN_BoolValue(true);
		private static LSN_BoolValue False = new LSN_BoolValue(false);
		public static LSN_BoolValue GetBoolValue(bool val)
			=> val? True : False;

		public LsnType Type { get; set; }
		public bool Value { get; private set; }
		public bool BoolValue { get { return Value; } }

		private LSN_BoolValue(bool val)
		{
			Value = val;
		}

		public ILsnValue Clone() => this;
		public ILsnValue Eval(IInterpreter i) => this;
		public IExpression Fold() => this;
		public bool IsReifyTimeConst() => true;
		public string TranslateUniversal() => Value.ToString();


		public void Replace(IExpression oldExpr, IExpression newExpr) { }

		public bool Equals(IExpression other)
		{
			var e = other as IBoundValue<bool>;
			if (e == null) return false;
			return e.Value == Value;
		}
	}
}
