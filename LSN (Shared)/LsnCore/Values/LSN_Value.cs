using LsnCore.Expressions;
using LsnCore.Values;
using System;
using System.Collections.Generic;
using System.Text;
using Syroot.BinaryData;
using LsnCore.Types;
using System.Collections;

namespace LsnCore
{
	/// <summary>
	/// An object representing an LSN value.
	/// </summary>
	[Serializable]
	public abstract class LsnValueB : ILsnValue
	{
		private TypeId _Type;

		public /*virtual*/ TypeId Type { get { return _Type; } protected set { _Type = value; } }
		public abstract bool BoolValue { get; }

		public abstract ILsnValue Clone();

		public static bool IsPure => true;

#if CORE
		public LsnValue Eval(IInterpreter i)
		{
			return new LsnValue(this);//Clone() ?
		}
#endif

		public static bool IsReifyTimeConst() => true;
		public abstract void Serialize(BinaryDataWriter writer);
	}
}
