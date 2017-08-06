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
	public class VectorInstance : LsnValueB, ICollectionValue
	{

		public readonly int Size;

		private readonly LsnValue[] Values;

		public override bool BoolValue { get { return true;/*Values != null;*/ } }

		//private VectorType _Type;

		//public ICollectionType CollectionType => _Type;

		public readonly TypeId GenericId;

		/// <summary>
		/// Get the value at an index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public LsnValue this [int index] { get { return Values[index].Clone(); } }

		public VectorInstance(VectorType type, LsnValue[] values)
		{
			//_Type = type;
			Type = type.Id;
			GenericId = type.GenericType.Id;
			Size = values.Length;
			Values = values;
		}

		public override ILsnValue Clone() => this;


		/// <summary>
		/// Get the length of this list.
		/// </summary>
		/// <returns></returns>
		public LsnValue Length() => new LsnValue(Values.Length);

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public LsnList ToLsnList()
		{
			var vals = new LsnValue[Size];
			for (int i = 0; i < Size; i++) vals[i] = Values[i].Clone();
			return new LsnList(LsnListGeneric.Instance.GetType(new List<TypeId>() { GenericId })
				as LsnListType, vals);
		}


		public LsnValue GetValue(int index)
			=> Values[index];


		public int GetLength() => Size;
	}
}
