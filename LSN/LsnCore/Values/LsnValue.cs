using LsnCore.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LsnCore.Types;
using System.Runtime.CompilerServices;

namespace LsnCore.Values
{
	[Serializable]
	public unsafe struct LsnValueX : IExpression
	{
		/// <summary>
		/// 
		/// </summary>
		public static readonly LsnValueX Nil = new LsnValueX(double.NaN, null);

		/// <summary>
		/// 
		/// </summary>
		public readonly ILsnValue Value;

		public bool IsPure => true;

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
		/*{
			get
			{
				double x = Data;
				return *((int*)&x);
			}
		}*/

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
		public LsnValueX(double value)
		{
			Data = value;
			Value = null;
			Id = LsnType.double_.Id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public LsnValueX(int value)
		{
			Data = value;
			Value = null;
			Id = LsnType.int_.Id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public LsnValueX(ILsnValue value)
		{
			Data = double.NaN;
			Value = value;
			Id = value.Type;
		}


		private LsnValueX(double d, ILsnValue v)
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
		public ILsnValue Eval(IInterpreter i) => null;//this;

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public IExpression Fold() => this;

		/// <summary>
		/// 
		/// </summary>
#pragma warning disable CS1718 // Comparison made to same variable
		public LsnValueX Clone => Data == Data ? this : new LsnValueX(Value.Clone());
#pragma warning restore CS1718 // Comparison made to same variable

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
		public bool Equals(IExpression other)
		{
			var v = other as LsnValueX?;
			if (v != null)
			{
				var val = v.Value;
				var data = val.Data;
#pragma warning disable CS1718 // Comparison made to same variable
				return (data == Data || (data != data && Data != Data)) && val.Value == Value;
#pragma warning restore CS1718 // Comparison made to same variable
			}
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static LsnValueX IntSum(LsnValueX a, LsnValueX b)
			=> new LsnValueX(a.IntValue + b.IntValue);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static LsnValueX IntDiff(LsnValueX a, LsnValueX b)
			=> new LsnValueX(a.IntValue - b.IntValue);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static LsnValueX IntProduct(LsnValueX a, LsnValueX b)
			=> new LsnValueX(a.IntValue * b.IntValue);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static LsnValueX IntQuotient(LsnValueX a, LsnValueX b)
			=> new LsnValueX(a.IntValue / b.IntValue);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static LsnValueX IntMod(LsnValueX a, LsnValueX b)
			=> new LsnValueX(a.IntValue % b.IntValue);


		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static LsnValueX DoubleSum(LsnValueX a, LsnValueX b)
			=> new LsnValueX(a.Data + b.Data);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static LsnValueX DoubleDiff(LsnValueX a, LsnValueX b)
			=> new LsnValueX(a.Data - b.Data);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static LsnValueX DoubleProduct(LsnValueX a, LsnValueX b)
			=> new LsnValueX(a.Data * b.Data);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static LsnValueX DoubleQuotient(LsnValueX a, LsnValueX b)
			=> new LsnValueX(a.Data / b.Data);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static LsnValueX DoubleMod(LsnValueX a, LsnValueX b)
			=> new LsnValueX(a.Data % b.Data);

	}
}
