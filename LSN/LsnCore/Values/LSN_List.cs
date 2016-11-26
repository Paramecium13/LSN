using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LsnCore.Types;

namespace LsnCore.Values
{
	[Serializable]
	public class LsnList : LsnValue, ICollectionValue, IWritableCollectionValue
	{
		private readonly List<ILsnValue> Values = new List<ILsnValue>();

		public override bool BoolValue { get { return true; } }

		//public readonly LsnListType _Type;

		
		//public ICollectionType CollectionType => _Type;


		public ILsnValue this[int index] { get { return Values[index]; } set { Values[index] = value; } }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		public LsnList(LsnListType type)
		{
			Type = type.Id;
			//_Type = type;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="values"></param>
		public LsnList(LsnListType type, IEnumerable<ILsnValue> values)
		{
			Type = type.Id;
			//_Type = type;
			Values = values.ToList();
		}
		

		internal void Add(ILsnValue value)
		{
			Values.Add(value);
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
		public ILsnValue GetValue(int index) => this[index];

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public int GetLength() => Values.Count;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		public void SetValue(int index, ILsnValue value)
		{
			Values[index] = value;
		}
	}
}
