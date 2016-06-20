using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LsnCore.Types;

namespace LsnCore.Values
{
	[Serializable]
	public class LSN_List : LsnValue, ICollectionValue 
	{
		private List<ILsnValue> Values = new List<ILsnValue>();

		public override bool BoolValue { get { return true; } }

		public ICollectionType CollectionType => Type as LsnListType;


		public ILsnValue this[int index] { get { return Values[index]; } set { Values[index] = value; } }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		public LSN_List(LsnListType type)
		{
			Type = type;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="values"></param>
		public LSN_List(LsnListType type, IEnumerable<ILsnValue> values)
		{
			Type = type;
			Values = values.ToList();
		}

		internal void Add(ILsnValue value)
		{

		}

		/// <summary>
		/// Get the length of this list.
		/// </summary>
		/// <returns></returns>
		public IntValue Length() => new IntValue(Values.Count);

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override ILsnValue Clone() => this;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public ILsnValue GetValue(ILsnValue index) => this[((IntValue)index).Value];

	}
}
