using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;
using System.Runtime.CompilerServices;

namespace LsnCore
{
#pragma warning disable CS1718 // Comparison made to same variable
#pragma warning disable RECS0088 // Comparing equal expression for equality is usually useless

	[Serializable]
	public unsafe struct LsnValue : IExpression, IEquatable<LsnValue>
	{
		/// <summary>
		/// 
		/// </summary>
		public static readonly LsnValue Nil = new LsnValue(double.NaN, null);

		/// <summary>
		/// 
		/// </summary>
		public readonly ILsnValue Value;

		public bool IsPure => true;



		public bool IsNull => Data != Data && Value == null;


		public bool BoolValue =>
			Data == Data ? Math.Abs(Data) > double.Epsilon : Value?.BoolValue ?? false;


		/// <summary>
		/// 
		/// </summary>
		private readonly double Data;

		/// <summary>
		/// 
		/// </summary>
		private readonly TypeId Id;

		/// <summary>
		/// 
		/// </summary>
		public int IntValueB => Data.ToInt32Bitwise();

		/// <summary>
		/// 
		/// </summary>
		public int IntValue => (int)Data;

		/// <summary>
		/// 
		/// </summary>
		public double DoubleValue => Data;

		/// <summary>
		/// 
		/// </summary>
		public TypeId Type => Id;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public LsnValue(double value)
		{
			Data = value;
			Value = null;
			Id = LsnType.double_.Id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public LsnValue(int value)
		{
			Data = value;
			Value = null;
			Id = LsnType.int_.Id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public LsnValue(ILsnValue value)
		{
			Data = double.NaN;
			Value = value;
			Id = value.Type;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private LsnValue(double d, ILsnValue v)
		{
			Data = d;
			Value = v;
			Id = null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public LsnValue Eval(IInterpreter i) => this;

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public IExpression Fold() => this;

		/// <summary>
		/// 
		/// </summary>
		public LsnValue Clone() => Data == Data ? this : new LsnValue(Value.Clone());

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool IsReifyTimeConst() => true;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="oldExpr"></param>
		/// <param name="newExpr"></param>
		public void Replace(IExpression oldExpr, IExpression newExpr){}

		/// <summary>
		/// 
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue IntSum(LsnValue a, LsnValue b)
			=> new LsnValue(a.IntValue + b.IntValue);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue IntDiff(LsnValue a, LsnValue b)
			=> new LsnValue(a.IntValue - b.IntValue);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue IntProduct(LsnValue a, LsnValue b)
			=> new LsnValue(a.IntValue * b.IntValue);


		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue IntQuotient(LsnValue a, LsnValue b)
			=> new LsnValue(a.IntValue / b.IntValue);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue IntMod(LsnValue a, LsnValue b)
			=> new LsnValue(a.IntValue % b.IntValue);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue IntPow(LsnValue a, LsnValue b)
			=> new LsnValue(Math.Pow(a.IntValue, b.IntValue));

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue DoubleSum(LsnValue a, LsnValue b)
			=> new LsnValue(a.Data + b.Data);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue DoubleDiff(LsnValue a, LsnValue b)
			=> new LsnValue(a.Data - b.Data);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue DoubleProduct(LsnValue a, LsnValue b)
			=> new LsnValue(a.Data * b.Data);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue DoubleQuotient(LsnValue a, LsnValue b)
			=> new LsnValue(a.Data / b.Data);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue DoubleMod(LsnValue a, LsnValue b)
			=> new LsnValue(a.Data % b.Data);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LsnValue DoublePow(LsnValue a, LsnValue b)
			=> new LsnValue(Math.Pow(a.Data, b.Data));


		public bool Equals(LsnValue other)
		{
			if (Data == Data)
				return Data == other.Data;
			return Value.Equals(other.Value);
		}
	}
}
#pragma warning restore RECS0088 // Comparing equal expression for equality is usually useless
#pragma warning restore CS1718 // Comparison made to same variable
