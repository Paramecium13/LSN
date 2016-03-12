using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSN_Core.Types;

namespace LSN_Core.Values
{
	[Serializable]
	public class LSN_List : LSN_Value, ICollectionValue 
	{
		private List<ILSN_Value> Values = new List<ILSN_Value>();

		public override bool BoolValue { get { return true; } }

		public ICollectionType CollectionType => Type as LSN_ListType;


		public ILSN_Value this[int index] { get { return Values[index]; } set { Values[index] = value; } }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		public LSN_List(LSN_ListType type)
		{
			Type = type;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="values"></param>
		public LSN_List(LSN_ListType type, IEnumerable<ILSN_Value> values)
		{
			Type = type;
			Values = values.ToList();
		}

		internal void Add(ILSN_Value value)
		{

		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override ILSN_Value Clone() => this;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public ILSN_Value GetValue(ILSN_Value index) => this[((IntValue)index).Value];

	}
}
