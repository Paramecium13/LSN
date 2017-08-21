using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LsnCore.Types;

namespace LsnCore.Values
{
	[Serializable]
	public class LsnList : LsnValueB, ICollectionValue, IWritableCollectionValue
	{
		private readonly List<LsnValue> Values = new List<LsnValue>();

		public LsnValue[] GetValues()
			=> Values.ToArray();

		public override bool BoolValue => true;

		//public readonly LsnListType _Type;

		
		//public ICollectionType CollectionType => _Type;


		public LsnValue this[int index] { get { return Values[index]; } set { Values[index] = value; } }

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
		public LsnList(LsnListType type, IEnumerable<LsnValue> values)
		{
			Type = type.Id;
			//_Type = type;
			Values = values.ToList();
		}
		
		public LsnList(TypeId type)
		{
			Type = type;
		}

		public LsnList(TypeId type, IEnumerable<LsnValue> values)
		{
			Type = type;
			Values = values.ToList();
		}

		internal void Add(LsnValue value)
		{
			Values.Add(value);
		}

		/// <summary>
		/// Get the length of this list.
		/// </summary>
		/// <returns></returns>
		public LsnValue Length() => new LsnValue(Values.Count);

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
		public LsnValue GetValue(int index) => this[index];

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
		public void SetValue(int index, LsnValue value)
		{
			Values[index] = value;
		}
	}
}
