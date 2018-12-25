using System;
using System.Collections.Generic;
using System.Text;
using LsnCore.Expressions;
using LsnCore.Types;
using Syroot.BinaryData;

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
	/// LSN value that contains a string, is effectively passed by reference.
	/// </summary>
	public sealed class StringValue : IBoundValue<string>
	{
		private static readonly TypeId id = LsnType.string_.Id;

		public TypeId Type { get { return id; } }

		public string Value { get; private set; }
		public bool BoolValue { get { return true; } }

		public static bool IsPure => true;

		/// <summary>
		/// ...
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

		public override string ToString() => Value;

		public static explicit operator string(StringValue v) => v.Value;
		public static explicit operator StringValue(string s) => new StringValue(s);

		public bool Equals(IExpression other)
		{
			var e = other as IBoundValue<string>;
			if (e == null) return false;
			return e.Value == Value;
		}

		public void Serialize(BinaryDataWriter writer)
		{
			writer.Write((byte)ConstantCode.String);
			writer.Write(Value);
		}

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.TabledConstant);
			writer.Write(resourceSerializer.TableConstant(this));
		}
	}

	/// <summary>
	/// ...
	/// </summary>
	public static class LsnBoolValue
	{
		static readonly LsnValue True = new LsnValue(1);
		static readonly LsnValue False = LsnValue.Nil;
		public static LsnValue GetBoolValue(bool val)
			=> val? True : False;
	}
}
