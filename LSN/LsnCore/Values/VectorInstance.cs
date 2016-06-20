using System;
using System.Collections.Generic;
using System.Text;
using LsnCore.Types;

namespace LsnCore.Values
{
	/// <summary>
	/// A readonly collection passed by value.
	/// </summary>
	[Serializable]
	public class VectorInstance : LsnValue, ICollectionValue
	{

		public readonly int Size;

		private readonly ILsnValue[] Values;

		public override bool BoolValue { get { return true;/*Values != null;*/ } }

		public ICollectionType CollectionType => Type as VectorType;

		/// <summary>
		/// Get the value at an index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public ILsnValue this [int index] { get { return Values[index].Clone(); } }

		public VectorInstance(VectorType type, ILsnValue[] values)
		{
			Type = type;
			Size = values.Length;
			Values = values;
		}

		public override ILsnValue Clone() => this;


		/// <summary>
		/// Get the length of this list.
		/// </summary>
		/// <returns></returns>
		public IntValue Length() => new IntValue(Values.Length);

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public LSN_List ToLSN_List()
		{
			var vals = new ILsnValue[Size];
			for (int i = 0; i < Size; i++) vals[i] = Values[i].Clone();
			return new LSN_List(LsnListGeneric.Instance.GetType(new List<LsnType>() { ((VectorType)Type).GenericType })
				as LsnListType, vals);
		}

		public ILsnValue GetValue(ILsnValue index)
		{
			throw new NotImplementedException();
		}
	}
}
