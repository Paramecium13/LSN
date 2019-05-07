using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Syroot.BinaryData;
using System.Collections;

namespace LsnCore
{
#pragma warning disable CS1718 // Comparison made to same variable
#pragma warning disable RECS0088 // Comparing equal expression for equality is usually useless
	[StructLayout( LayoutKind.Explicit)]
	public
#if LSNR
		unsafe
#endif
		struct LsnValue : IExpression, IEquatable<LsnValue>
	{
		/// <summary>
		/// Nil
		/// </summary>
		public static readonly LsnValue Nil = new LsnValue(double.NaN, null);

		public bool IsPure => true;

		public bool IsNull => Data != Data && Value == null;

		public bool BoolValue =>
			Data == Data ? Math.Abs(Data) > double.Epsilon : Value?.BoolValue ?? false;

		public bool BoolValueSimple => IntValue != 0;

		/// <summary>
		/// The numeric data.
		/// </summary>
		[FieldOffset(0)]
		readonly double Data;

#if LSNR
		public ulong RawData { get
			{
				fixed (double* x = &Data)
				{
					return *(ulong*)x;
				}
			}
		}
#endif
		[FieldOffset(0)]
		public readonly uint HandleData;

		[FieldOffset(0)]
		public readonly float X;

		[FieldOffset(0)]
		public readonly float Y;

		/// <summary>
		/// Value...
		/// </summary>
		[FieldOffset(8)]
		public readonly ILsnValue Value;

		/*/// <summary>
		/// Unused
		/// </summary>
		public int IntValueB => Data.ToInt32Bitwise();*/

		/// <summary>
		/// The signed 32-bit integer value
		/// </summary>
		public int IntValue => (int)Data;

		/// <summary>
		/// The double precision floating point value
		/// </summary>
		public double DoubleValue => Data;

#if LSNR
		/// <summary>
		/// ...
		/// </summary>
		[FieldOffset(16)]
		private readonly TypeId Id;
		public TypeId Type => Id;
#else
		public TypeId Type => throw new NotImplementedException();
#endif
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public LsnValue(double value)
		{
			HandleData = 0;
			X = 0f;
			Y = 0f;
			Data = value;
			Value = null;
#if LSNR
			Id = LsnType.double_.Id;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public LsnValue(int value)
		{
			HandleData = 0;
			X = 0f;
			Y = 0f;
			Data = value;
			Value = null;
#if LSNR
			Id = LsnType.int_.Id;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public LsnValue(ILsnValue value)
		{
			HandleData = 0;
			X = 0f;
			Y = 0f;
			Data = double.NaN;
			Value = value;
#if LSNR
			Id = value.Type;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public LsnValue(bool value)
		{
			HandleData = 0;
			X = 0f;
			Y = 0f;
			Data = value ? 1 : 0;
			Value = null;
#if LSNR
			Id = LsnType.Bool_.Id;
#endif
		}

		public LsnValue(uint handle
#if LSNR
			, TypeId type
#endif
			)
		{
			Data = 0;
			X = 0f;
			Y = 0f;
			HandleData = handle;
			Value = null;
#if LSNR
			Id = type;
#endif
		}

		public LsnValue(float x, float y)
		{
			Data = 0;
			HandleData = 0;
			Value = null;
			X = x;
			Y = y;
#if LSNR
			Id = null;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		LsnValue(double d, ILsnValue v)
		{
			HandleData = 0;
			X = 0f;
			Y = 0f;
			Data = d;
			Value = v;
#if LSNR
			Id = NullType.Instance.Id;
#endif
		}
#if CORE
		/// <summary>
		/// ...
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public LsnValue Eval(IInterpreter i) => this;
#endif
		/// <summary>
		/// ...
		/// </summary>
		/// <returns></returns>
		public IExpression Fold() => this;

		/// <summary>
		/// ...
		/// </summary>
		public LsnValue Clone() => Data == Data ? this : new LsnValue(Value.Clone());

		/// <summary>
		/// ...
		/// </summary>
		/// <returns></returns>
		public bool IsReifyTimeConst() => true;

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="oldExpr"></param>
		/// <param name="newExpr"></param>
		public void Replace(IExpression oldExpr, IExpression newExpr){}

		/// <summary>
		/// ...
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(IExpression other)
		{
			var v = other as LsnValue?;
			if (v != null)
			{
				var val = v.Value;
				var data = val.Data;
				return (Math.Abs(data - Data) < double.Epsilon || (data != data && Data != Data)) && val.Value == Value;
			}
			return false;
		}

		internal void Serialize(BinaryDataWriter writer)
		{
			if (Value == null)
			{
				writer.Write((byte)ConstantCode.DoubleOrInt);
				writer.Write(Data);
			}
			else Value.Serialize(writer);
		}

		public void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			if (Value == null)
			{
				writer.Write((byte)ExpressionCode.DoubleValueConstant);
				writer.Write(Data);
			}
			else
			{
				writer.Write((byte)ExpressionCode.TabledConstant);
				writer.Write(resourceSerializer.TableConstant(Value));
			}
		}

		/// <summary>
		/// Integer addition
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue IntSum(LsnValue a, LsnValue b)
			=> new LsnValue(a.IntValue + b.IntValue);

		/// <summary>
		/// Integer subtraction
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue IntDiff(LsnValue a, LsnValue b)
			=> new LsnValue(a.IntValue - b.IntValue);

		/// <summary>
		/// Integer multiplication
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue IntProduct(LsnValue a, LsnValue b)
			=> new LsnValue(a.IntValue * b.IntValue);


		/// <summary>
		/// Integer division
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue IntQuotient(LsnValue a, LsnValue b)
			=> new LsnValue(a.IntValue / b.IntValue);

		/// <summary>
		/// Integer modulus
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue IntMod(LsnValue a, LsnValue b)
			=> new LsnValue(a.IntValue % b.IntValue);

		/// <summary>
		/// Integer exponentiation
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue IntPow(LsnValue a, LsnValue b)
			=> new LsnValue(Math.Pow(a.IntValue, b.IntValue));

		/// <summary>
		/// Floating point addition
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue DoubleSum(LsnValue a, LsnValue b)
			=> new LsnValue(a.Data + b.Data);

		/// <summary>
		/// Floating point subtraction
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue DoubleDiff(LsnValue a, LsnValue b)
			=> new LsnValue(a.Data - b.Data);

		/// <summary>
		/// Floating point multiplication
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue DoubleProduct(LsnValue a, LsnValue b)
			=> new LsnValue(a.Data * b.Data);

		/// <summary>
		/// Floating point division
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue DoubleQuotient(LsnValue a, LsnValue b)
			=> new LsnValue(a.Data / b.Data);

		/// <summary>
		/// Floating point modulus
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue DoubleMod(LsnValue a, LsnValue b)
			=> new LsnValue(a.Data % b.Data);

		/// <summary>
		/// Floating point exponentiation
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue DoublePow(LsnValue a, LsnValue b)
			=> new LsnValue(Math.Pow(a.Data, b.Data));


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(LsnValue other)
		{
			if (Data == Data)
				return Math.Abs(Data - other.Data) < double.Epsilon;
			if (Value == null) return other.Value == null;
			return Value.Equals(other.Value);
		}

		IEnumerator<IExpression> IEnumerable<IExpression>.GetEnumerator()
		{
			yield return null;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			yield return null;
		}
	}
}
#pragma warning restore RECS0088 // Comparing equal expression for equality is usually useless
#pragma warning restore CS1718 // Comparison made to same variable
