using System;
using System.Collections.Generic;
using System.Text;
using LSN_Core.Expressions;

namespace LSN_Core
{
	/// <summary>
	/// LSN value that directly corresponds to a .NET type.
	/// </summary>
	/// <typeparam name="T">The .NET type this value has.</typeparam>
	public interface IBoundValue<T> : ILSN_Value
	{
		T Value { get; }
	}

	/// <summary>
	/// LSN value that contains a 32 bit integer.
	/// </summary>
	public struct IntValue : IBoundValue<int>
	{
		public int Value { get; private set; }
		public LSN_Type Type { get { return LSN_Type.int_; } }

		public IntValue(int val)
		{
			Value = val;
		}

		public ILSN_Value Clone() => new IntValue(Value);

		public ILSN_Value Eval(IInterpreter i) => this;
		public IExpression Fold() => this;
		public bool IsReifyTimeConst() => true;
		public string TranslateUniversal() => Value.ToString();
		public int GetSize() => sizeof(int);
	}

	/// <summary>
	/// LSN value that contains a string, is still passed by value.
	/// </summary>
	public struct StringValue : IBoundValue<string>
	{
		public LSN_Type Type {get { return LSN_Type.string_; } }
		public string Value { get; private set; }

		public StringValue(string val)
		{
			Value = val;
		}

		public ILSN_Value Clone()
		{
			char[] c = new char[Value.Length];
			Value.CopyTo(0, c, 0, 1);
			return new StringValue(new string(c));
		}

		public ILSN_Value Eval(IInterpreter i) => this;
		public IExpression Fold() => this;
		public bool IsReifyTimeConst() => true;
		public string TranslateUniversal() => Value.ToString();

		public int GetSize() => sizeof(char)* Value.Length;
	}

	/// <summary>
	/// LSN value that contains a Double.
	/// </summary>
	public struct DoubleValue : IBoundValue<double>
	{
		public double Value { get; private set; }
		public LSN_Type Type { get { return LSN_Type.double_; } }

		public DoubleValue(double val)
		{
			Value = val;
		}
		public ILSN_Value Clone() => new DoubleValue(Value);

		public ILSN_Value Eval(IInterpreter i) => this;
		public IExpression Fold() => this;
		public bool IsReifyTimeConst() => true;
		public string TranslateUniversal() => Value.ToString();
		public int GetSize() => sizeof(double);
    }

	public class BoolValue : IBoundValue<bool>
	{
		public LSN_Type Type { get { return LSN_Type.Bool_; } }
		public bool Value { get; private set; }
		public BoolValue(bool val)
		{
			Value = val;
		}
		public ILSN_Value Clone() => this;
		public ILSN_Value Eval(IInterpreter i) => this;
		public IExpression Fold() => this;
		public bool IsReifyTimeConst() => true;
		public string TranslateUniversal() => Value.ToString();
		public int GetSize() => sizeof(bool);
    }
}
