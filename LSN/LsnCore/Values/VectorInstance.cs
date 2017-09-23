using System;
using System.Collections.Generic;
using System.Text;
using LsnCore.Types;
using Syroot.BinaryData;

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
			:this(type.Id,type.GenericId,values) {}

		public VectorInstance(TypeId type, TypeId genericType, LsnValue[] values)
		{
			Type = type; GenericId = genericType; Values = values; Size = values.Length;
		}

		public override ILsnValue Clone() => this;

		/// <summary>
		/// Get the length of this vector.
		/// </summary>
		/// <returns></returns>
		public LsnValue Length() => new LsnValue(Values.Length);

		/// <summary>
		/// ...
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

		public override void Serialize(BinaryDataWriter writer)
		{
			writer.Write((byte)ConstantCode.Vector);
			writer.Write(Type.Name);
			writer.Write((ushort)Values.Length);
			for (int i = 0; i < Values.Length; i++)
				Values[i].Serialize(writer);
		}

		public override void Serialize(BinaryDataWriter writer, ResourceSerializer resourceSerializer)
		{
			writer.Write((byte)ExpressionCode.TabledConstant);
			writer.Write(resourceSerializer.TableConstant(this));
		}
	}
}
