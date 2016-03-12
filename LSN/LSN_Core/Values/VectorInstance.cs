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

		private ILSN_Value[] Values;

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
			/*var vals = new ILSN_Value[Size];
			for(int i = 0; i < Size; i++) vals[i] = Values[i].Clone();
			return new VectorInstance((VectorType)Type, vals);*/
		

		public LSN_List ToLSN_List()
		{
			var vals = new ILSN_Value[Size];
			for (int i = 0; i < Size; i++) vals[i] = Values[i].Clone();
			return null;
		}

		public ILSN_Value GetValue(ILSN_Value index)
		{
			throw new NotImplementedException();
		}
	}
}
