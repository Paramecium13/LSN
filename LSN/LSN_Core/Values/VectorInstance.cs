using System;
using System.Collections.Generic;
using System.Text;
using LSN_Core.Types;

namespace LSN_Core.Values
{
	/// <summary>
	/// A readonly collection passed by value.
	/// </summary>
	[Serializable]
	public class VectorInstance : LSN_Value, ICollectionValue
	{

		public readonly int Size;

		private readonly ILSN_Value[] Values;

		public override bool BoolValue { get { return true;/*Values != null;*/ } }

		public ICollectionType CollectionType => Type as VectorType;

		/// <summary>
		/// Get the value at an index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public ILSN_Value this [int index] { get { return Values[index].Clone(); } }

		public VectorInstance(VectorType type, ILSN_Value[] values)
		{
			Type = type;
			Size = values.Length;
			Values = values;
		}

		public override ILSN_Value Clone() => this;


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
			var vals = new ILSN_Value[Size];
			for (int i = 0; i < Size; i++) vals[i] = Values[i].Clone();
			return new LSN_List(LSN_ListGeneric.Instance.GetType(new List<LSN_Type>() { ((VectorType)Type).GenericType })
				as LSN_ListType, vals);
		}

		public ILSN_Value GetValue(ILSN_Value index)
		{
			throw new NotImplementedException();
		}
	}
}
