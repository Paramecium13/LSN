using System;
using System.Collections.Generic;
using System.Text;
using LsnCore.Types;
using Syroot.BinaryData;

namespace LsnCore
{
	/// <summary>
	/// LSN value that directly corresponds to a .NET type.
	/// </summary>
	/// <typeparam name="T">The .NET type this value has.</typeparam>
	public interface IBoundValue<out T> : ILsnValue
	{
		T Value { get; }
	}

	/// <summary>
	/// LSN value that contains a string, is effectively passed by reference.
	/// </summary>
	public sealed class StringValue : IBoundValue<string>
	{
		private static readonly TypeId id = LsnType.string_.Id;

		/// <summary>
		/// Gets the TypeId of th.
		/// </summary>
		public TypeId Type => id;

		public string Value { get; }
		public bool BoolValue => true;

		public static StringValue Empty { get; } = new("");

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="val">The value</param>
		public StringValue(string val)
		{
			Value = val ?? throw new ArgumentNullException(nameof(val));
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
		public static explicit operator StringValue(string s) => new(s);

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is StringValue strVal && strVal.Value == Value;
		}

		/// <inheritdoc/>
		public override int GetHashCode() => Value.GetHashCode();
		
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
		private static readonly LsnValue True = new(true);
		private static readonly LsnValue False = new(false);
		public static LsnValue GetBoolValue(bool val)
			=> val? True : False;
	}
}
